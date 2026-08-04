using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using RailwayReservation.Application.Bookings.Ports;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Booking;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Infrastructure.Repositories;

/// <summary>
/// Concurrency strategy (see README "Concurrency Strategy" for the full writeup):
///
/// 1. Lock every requested seat row with `SELECT ... FOR UPDATE`, sorted by seat id so two
///    multi-seat bookings can never deadlock on each other. This serializes concurrent attempts
///    on the *same* seat.
/// 2. Re-check for occupied legs *inside* the lock — this is what actually prevents the race,
///    and gives a clean 409 instead of a raw constraint-violation error for the common case.
/// 3. Insert the Booking + BookingSegment + one BookingSegmentLeg row per unit leg covered.
///    booking_segment_legs also carries a UNIQUE(seat_id, journey_id, leg_order) index — a hard
///    DB invariant that holds even if this application-level check were ever bypassed or buggy.
///    Its violation (MySQL error 1062) is caught below and mapped to the same 409.
/// </summary>
public sealed class BookingRepository(AppDbContext db) : IBookingRepository
{
    private const int MySqlDuplicateEntryErrorNumber = 1062;

    public async Task<Booking> CreateAsync(CreateBookingCommand command, CancellationToken ct = default)
    {
        var sortedSegments = command.Segments.OrderBy(s => s.SeatId).ToList();
        var seatIds = sortedSegments.Select(s => s.SeatId).ToList();

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await LockSeatsAsync(seatIds, ct);

            var requestedLegs = Enumerable.Range(command.OriginOrder, command.DestinationOrder - command.OriginOrder).ToHashSet();
            var conflict = await db.BookingSegmentLegs
                .Where(l => seatIds.Contains(l.SeatId) && l.JourneyId == command.JourneyId && requestedLegs.Contains(l.LegOrder))
                .Select(l => l.SeatId)
                .FirstOrDefaultAsync(ct);

            if (conflict != Guid.Empty)
            {
                var seatNumber = await db.Seats.Where(s => s.Id == conflict).Select(s => s.SeatNumber).FirstAsync(ct);
                throw new SegmentOverlapException(seatNumber);
            }

            var booking = new Domain.Entities.Booking
            {
                Id = Guid.NewGuid(),
                BookingReference = command.BookingReference,
                JourneyId = command.JourneyId,
                OriginStationId = command.OriginStationId,
                DestinationStationId = command.DestinationStationId,
                PassengerName = command.PassengerName,
                PassengerContact = command.PassengerContact,
                TotalFare = command.TotalFare,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            db.Bookings.Add(booking);

            foreach (var segment in sortedSegments)
            {
                var segmentEntity = new BookingSegment
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    SeatId = segment.SeatId,
                    JourneyId = command.JourneyId,
                    OriginOrder = command.OriginOrder,
                    DestinationOrder = command.DestinationOrder,
                    Fare = segment.Fare,
                    Status = BookingStatus.Confirmed,
                    CreatedAt = DateTime.UtcNow,
                };
                db.BookingSegments.Add(segmentEntity);

                foreach (var leg in SegmentOverlap.Legs(new StationRange(command.OriginOrder, command.DestinationOrder)))
                {
                    db.BookingSegmentLegs.Add(new BookingSegmentLeg
                    {
                        BookingSegmentId = segmentEntity.Id,
                        SeatId = segment.SeatId,
                        JourneyId = command.JourneyId,
                        LegOrder = leg,
                    });
                }
            }

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsDuplicateKey(ex, "ux_booking_segment_legs_seat_journey_leg"))
            {
                throw new SegmentOverlapException("one of the selected seats");
            }
            catch (DbUpdateException ex) when (IsDuplicateKey(ex, "ux_bookings_booking_reference"))
            {
                throw new ConflictAppException("DUPLICATE_BOOKING_REFERENCE", "Generated booking reference collided, retrying.");
            }

            await transaction.CommitAsync(ct);

            return await FindByIdAsync(booking.Id, ct)
                ?? throw new InvalidOperationException($"Booking {booking.Id} vanished immediately after creation — this should never happen.");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private async Task LockSeatsAsync(IReadOnlyList<Guid> seatIds, CancellationToken ct)
    {
        var parameters = seatIds.Cast<object>().ToArray();
        var placeholders = string.Join(",", Enumerable.Range(0, parameters.Length).Select(i => $"{{{i}}}"));
        var sql = $"SELECT id FROM seats WHERE id IN ({placeholders}) FOR UPDATE";
        await db.Database.SqlQueryRaw<Guid>(sql, parameters).ToListAsync(ct);
    }

    private static bool IsDuplicateKey(DbUpdateException ex, string indexName) =>
        ex.InnerException is MySqlException { Number: MySqlDuplicateEntryErrorNumber } mysqlEx && mysqlEx.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase);

    public Task<Booking?> FindByReferenceAsync(string reference, CancellationToken ct = default) =>
        FullBookingQuery().FirstOrDefaultAsync(b => b.BookingReference == reference, ct);

    public Task<Booking?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        FullBookingQuery().FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<Booking> CancelAsync(Guid id, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await db.Bookings.Where(b => b.Id == id).ExecuteUpdateAsync(
                setters => setters.SetProperty(b => b.Status, BookingStatus.Cancelled).SetProperty(b => b.UpdatedAt, DateTime.UtcNow), ct);

            await db.BookingSegments.Where(s => s.BookingId == id).ExecuteUpdateAsync(
                setters => setters.SetProperty(s => s.Status, BookingStatus.Cancelled), ct);

            // Hard-delete the leg rows: MySQL has no partial/filtered unique index to scope the
            // UNIQUE(seat_id, journey_id, leg_order) constraint to CONFIRMED rows only, so a
            // cancelled segment's legs must be removed for the seat/leg to become bookable again.
            var segmentIds = await db.BookingSegments.Where(s => s.BookingId == id).Select(s => s.Id).ToListAsync(ct);
            await db.BookingSegmentLegs.Where(l => segmentIds.Contains(l.BookingSegmentId)).ExecuteDeleteAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return await FindByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"Booking {id} vanished immediately after cancellation — this should never happen.");
    }

    public async Task<Booking> UpdatePassengerDetailsAsync(Guid id, string? passengerName, string? passengerContact, CancellationToken ct = default)
    {
        await db.Bookings.Where(b => b.Id == id).ExecuteUpdateAsync(setters => setters
            .SetProperty(b => b.PassengerName, b => passengerName ?? b.PassengerName)
            .SetProperty(b => b.PassengerContact, b => passengerContact ?? b.PassengerContact)
            .SetProperty(b => b.UpdatedAt, DateTime.UtcNow), ct);

        return await FindByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"Booking {id} vanished immediately after update — this should never happen.");
    }

    private IQueryable<Booking> FullBookingQuery() => db.Bookings
        .Include(b => b.Journey).ThenInclude(j => j.Train)
        .Include(b => b.OriginStation)
        .Include(b => b.DestinationStation)
        .Include(b => b.Segments).ThenInclude(s => s.Seat).ThenInclude(seat => seat.Coach)
        .AsSplitQuery();
}
