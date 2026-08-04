using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RailwayReservation.IntegrationTests.Infrastructure;
using Xunit;

namespace RailwayReservation.IntegrationTests.Api;

public class BookingsApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private TestFixtureData _fixture = null!;

    public BookingsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await using var db = _factory.CreateDbContext();
        _fixture = await TestFixture.SeedAsync(db);
    }

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        await TestFixture.ResetBookingsAsync(db, _fixture);
    }

    private object BookingPayload(Guid? origin = null, Guid? destination = null, Guid? seatId = null) => new
    {
        journeyId = _fixture.JourneyId,
        originStationId = origin ?? _fixture.StationA,
        destinationStationId = destination ?? _fixture.StationC,
        passengerName = "Kamala Silva",
        passengerContact = "0712345678",
        seatIds = new[] { seatId ?? _fixture.SeatIds[0] },
    };

    [Fact]
    public async Task Creates_a_booking_and_returns_a_booking_reference()
    {
        var res = await _client.PostAsJsonAsync("/bookings", BookingPayload());
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var data = await res.DataAsync();
        data.GetProperty("bookingReference").GetString().Should().StartWith("RWY-");
        data.GetProperty("status").GetString().Should().Be("Confirmed");
        data.GetProperty("segments").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task Rejects_an_overlapping_booking_on_the_same_seat_with_409()
    {
        (await _client.PostAsJsonAsync("/bookings", BookingPayload())).StatusCode.Should().Be(HttpStatusCode.Created);

        // A->C overlaps B->D on the same seat.
        var res = await _client.PostAsJsonAsync("/bookings", BookingPayload(_fixture.StationB, _fixture.StationD));
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var (code, _) = await res.ErrorAsync();
        code.Should().Be("SEGMENT_OVERLAP");
    }

    [Fact]
    public async Task Allows_an_adjacent_non_overlapping_booking_on_the_same_seat()
    {
        // First passenger: A->C. Second passenger: C->D. Same seat, adjacent segments.
        (await _client.PostAsJsonAsync("/bookings", BookingPayload())).StatusCode.Should().Be(HttpStatusCode.Created);

        var res = await _client.PostAsJsonAsync("/bookings", BookingPayload(_fixture.StationC, _fixture.StationD));
        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Looks_up_a_booking_by_its_reference()
    {
        var created = await (await _client.PostAsJsonAsync("/bookings", BookingPayload())).DataAsync();
        var reference = created.GetProperty("bookingReference").GetString();

        var res = await _client.GetAsync($"/bookings/{reference}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        (await res.DataAsync()).GetProperty("bookingReference").GetString().Should().Be(reference);
    }

    [Fact]
    public async Task Returns_404_for_an_unknown_booking_reference()
    {
        var res = await _client.GetAsync("/bookings/RWY-DOESNOTEXIST");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await res.ErrorAsync()).Code.Should().Be("BOOKING_NOT_FOUND");
    }

    [Fact]
    public async Task Cancelling_a_booking_frees_the_seat_for_the_same_segment_again()
    {
        var created = await (await _client.PostAsJsonAsync("/bookings", BookingPayload())).DataAsync();
        var reference = created.GetProperty("bookingReference").GetString();

        var cancelRes = await _client.DeleteAsync($"/bookings/{reference}");
        cancelRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var rebooked = await _client.PostAsJsonAsync("/bookings", BookingPayload());
        rebooked.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Rejects_booking_a_segment_where_origin_is_not_before_destination()
    {
        var res = await _client.PostAsJsonAsync("/bookings", BookingPayload(_fixture.StationC, _fixture.StationA));
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await res.ErrorAsync()).Code.Should().Be("INVALID_SEGMENT");
    }

    [Fact]
    public async Task Rejects_booking_with_a_malformed_request_body()
    {
        var res = await _client.PostAsJsonAsync("/bookings", new { journeyId = "not-a-guid" });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.ErrorAsync()).Code.Should().Be("VALIDATION_ERROR");
    }
}
