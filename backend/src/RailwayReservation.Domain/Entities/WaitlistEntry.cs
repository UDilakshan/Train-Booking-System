using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Entities;

public class WaitlistEntry
{
    public Guid Id { get; set; }
    public Guid JourneyId { get; set; }
    public Guid OriginStationId { get; set; }
    public Guid DestinationStationId { get; set; }
    public string PassengerName { get; set; } = default!;
    public string PassengerContact { get; set; } = default!;
    public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Journey Journey { get; set; } = default!;
    public Station OriginStation { get; set; } = default!;
    public Station DestinationStation { get; set; } = default!;
}
