using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Entities;

/// <summary>Future-ready, minimal for now — no payment gateway integration yet.</summary>
public class Payment
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = default!;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionRef { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Booking Booking { get; set; } = default!;
}
