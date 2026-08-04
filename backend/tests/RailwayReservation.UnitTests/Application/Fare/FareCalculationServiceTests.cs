using FluentAssertions;
using RailwayReservation.Application.Fare;
using RailwayReservation.Application.Fare.Ports;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;
using Xunit;

namespace RailwayReservation.UnitTests.Application.Fare;

public class FareCalculationServiceTests
{
    private const string OffPeakTime = "11:00";
    private const string PeakTime = "07:30";
    private static readonly DateTime Day = new(2026, 1, 10);

    private static FareRule Rule(
        FareRuleType ruleType,
        FareValueType valueType,
        decimal value,
        CoachType? coachType = null,
        int priority = 0) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test rule",
        CoachType = coachType,
        RuleType = ruleType,
        ValueType = valueType,
        Value = value,
        Priority = priority,
        IsActive = true,
    };

    private sealed class StubFareRuleRepository(IReadOnlyList<FareRule> rules) : IFareRuleRepository
    {
        public Task<IReadOnlyList<FareRule>> GetApplicableRulesAsync(DateTime asOf, CancellationToken ct = default) =>
            Task.FromResult(rules);
    }

    private static FareCalculationService Service(params FareRule[] rules) => new(new StubFareRuleRepository(rules));

    [Fact]
    public async Task Prices_a_third_class_off_peak_non_express_journey_at_distance_times_base_rate()
    {
        var service = Service(Rule(FareRuleType.Base, FareValueType.PerKm, 6, CoachType.ThirdClass));

        var fare = await service.CalculateAsync(new FareQuoteInput(0, 100, CoachType.ThirdClass, OffPeakTime, false, Day));

        fare.Should().Be(600);
    }

    [Fact]
    public async Task Prefers_coach_specific_base_rule_over_wildcard_fallback()
    {
        var service = Service(
            Rule(FareRuleType.Base, FareValueType.PerKm, 7.5m, coachType: null, priority: 0),
            Rule(FareRuleType.Base, FareValueType.PerKm, 15, CoachType.FirstClass, priority: 10));

        var fare = await service.CalculateAsync(new FareQuoteInput(0, 100, CoachType.FirstClass, OffPeakTime, false, Day));

        fare.Should().Be(1500);
    }

    [Fact]
    public async Task Falls_back_to_wildcard_base_rule_when_no_coach_specific_rule_exists()
    {
        var service = Service(Rule(FareRuleType.Base, FareValueType.PerKm, 7.5m));

        var fare = await service.CalculateAsync(new FareQuoteInput(0, 40, CoachType.SecondClass, OffPeakTime, false, Day));

        fare.Should().Be(300);
    }

    [Fact]
    public async Task Applies_percent_peak_surcharge_only_when_departure_is_in_a_peak_window()
    {
        var rules = new[]
        {
            Rule(FareRuleType.Base, FareValueType.PerKm, 10, CoachType.SecondClass),
            Rule(FareRuleType.Peak, FareValueType.Percent, 15),
        };
        var service = Service(rules);

        var peakFare = await service.CalculateAsync(new FareQuoteInput(0, 100, CoachType.SecondClass, PeakTime, false, Day));
        var offPeakFare = await service.CalculateAsync(new FareQuoteInput(0, 100, CoachType.SecondClass, OffPeakTime, false, Day));

        offPeakFare.Should().Be(1000);
        peakFare.Should().Be(1150);
    }

    [Fact]
    public async Task Compounds_peak_and_express_surcharges_in_pipeline_order()
    {
        var rules = new[]
        {
            Rule(FareRuleType.Base, FareValueType.PerKm, 10, CoachType.SecondClass),
            Rule(FareRuleType.Peak, FareValueType.Percent, 10),
            Rule(FareRuleType.Express, FareValueType.Percent, 20),
        };
        var service = Service(rules);

        var fare = await service.CalculateAsync(new FareQuoteInput(0, 100, CoachType.SecondClass, PeakTime, true, Day));

        // 1000 (base) -> 1100 (+10% peak) -> 1320 (+20% express)
        fare.Should().Be(1320);
    }

    [Fact]
    public async Task Never_returns_negative_fare_even_if_discount_exceeds_running_fare()
    {
        var rules = new[]
        {
            Rule(FareRuleType.Base, FareValueType.PerKm, 6, CoachType.ThirdClass),
            Rule(FareRuleType.Discount, FareValueType.Percent, 500),
        };
        var service = Service(rules);

        var fare = await service.CalculateAsync(new FareQuoteInput(0, 10, CoachType.ThirdClass, OffPeakTime, false, Day));

        fare.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Rejects_a_non_positive_distance()
    {
        var service = Service();

        var act = () => service.CalculateAsync(new FareQuoteInput(50, 50, CoachType.ThirdClass, OffPeakTime, false, Day));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
