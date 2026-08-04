using RailwayReservation.Application.Fare.Ports;
using RailwayReservation.Domain.Enums;
using RailwayReservation.Domain.Fare;
using RailwayReservation.Domain.Fare.Modifiers;

namespace RailwayReservation.Application.Fare;

public sealed record FareQuoteInput(
    decimal OriginDistanceKm,
    decimal DestinationDistanceKm,
    CoachType CoachType,
    string DepartureTime,
    bool IsExpress,
    DateTime JourneyDate);

public sealed class FareCalculationService(IFareRuleRepository fareRuleRepository)
{
    /// <summary>Executed in order — see Domain.Fare.Modifiers for what each step does and how to extend the pipeline.</summary>
    private readonly IFareModifier[] _pipeline =
    [
        new BaseDistanceFareModifier(),
        new ClassMultiplierModifier(),
        new PeakSurchargeModifier(),
        new ExpressSurchargeModifier(),
        new DiscountModifier(),
    ];

    public async Task<decimal> CalculateAsync(FareQuoteInput input, CancellationToken ct = default)
    {
        var distanceKm = input.DestinationDistanceKm - input.OriginDistanceKm;
        if (distanceKm <= 0)
        {
            throw new InvalidOperationException("Destination must be further along the route than the origin.");
        }

        var context = new FareContext(distanceKm, input.CoachType, input.DepartureTime, input.IsExpress, input.JourneyDate);
        var rules = await fareRuleRepository.GetApplicableRulesAsync(input.JourneyDate, ct);

        var fare = 0m;
        foreach (var modifier in _pipeline)
        {
            fare = modifier.Apply(context, fare, rules);
        }

        // Never let discounts push the fare below a nominal minimum.
        fare = Math.Max(fare, 0m);
        return Math.Round(fare, 2, MidpointRounding.AwayFromZero);
    }
}
