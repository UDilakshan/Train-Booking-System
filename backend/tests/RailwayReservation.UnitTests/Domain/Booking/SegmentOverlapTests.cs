using FluentAssertions;
using RailwayReservation.Domain.Booking;
using Xunit;

namespace RailwayReservation.UnitTests.Domain.Booking;

public class SegmentOverlapTests
{
    private static readonly StationRange ColomboToKandy = new(0, 9);
    private static readonly StationRange KandyToNanuOya = new(9, 14);
    private static readonly StationRange NanuOyaToBadulla = new(14, 18);
    private static readonly StationRange KandyToElla = new(9, 17);
    private static readonly StationRange HattonToBadulla = new(12, 18);

    [Fact]
    public void Adjacent_segments_sharing_a_boundary_station_do_not_overlap()
    {
        SegmentOverlap.Overlaps(ColomboToKandy, KandyToNanuOya).Should().BeFalse();
        SegmentOverlap.Overlaps(KandyToNanuOya, NanuOyaToBadulla).Should().BeFalse();
    }

    [Fact]
    public void Adjacency_check_is_symmetric()
    {
        SegmentOverlap.Overlaps(KandyToNanuOya, ColomboToKandy).Should().BeFalse();
    }

    [Fact]
    public void Rejects_the_exact_spec_scenario_Kandy_to_Ella_vs_Hatton_to_Badulla()
    {
        SegmentOverlap.Overlaps(KandyToElla, HattonToBadulla).Should().BeTrue();
        SegmentOverlap.Overlaps(HattonToBadulla, KandyToElla).Should().BeTrue();
    }

    [Fact]
    public void Detects_a_segment_fully_containing_another()
    {
        var colomboToBadulla = new StationRange(0, 18);
        SegmentOverlap.Overlaps(colomboToBadulla, KandyToNanuOya).Should().BeTrue();
        SegmentOverlap.Overlaps(KandyToNanuOya, colomboToBadulla).Should().BeTrue();
    }

    [Fact]
    public void Detects_two_identical_segments_as_overlapping()
    {
        SegmentOverlap.Overlaps(ColomboToKandy, ColomboToKandy with { }).Should().BeTrue();
    }

    [Fact]
    public void Detects_a_partial_overlap_that_is_not_pure_containment()
    {
        var colomboToNanuOya = new StationRange(0, 14);
        SegmentOverlap.Overlaps(colomboToNanuOya, KandyToElla).Should().BeTrue();
    }

    [Fact]
    public void Disjoint_segments_with_a_gap_do_not_overlap()
    {
        SegmentOverlap.Overlaps(ColomboToKandy, HattonToBadulla).Should().BeFalse();
    }

    [Fact]
    public void Three_adjacent_segments_can_tile_the_same_seat_across_a_full_journey()
    {
        SegmentOverlap.Overlaps(ColomboToKandy, KandyToNanuOya).Should().BeFalse();
        SegmentOverlap.Overlaps(KandyToNanuOya, NanuOyaToBadulla).Should().BeFalse();
        SegmentOverlap.Overlaps(ColomboToKandy, NanuOyaToBadulla).Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 5, true)]
    [InlineData(5, 5, false)]
    [InlineData(5, 2, false)]
    [InlineData(-1, 5, false)]
    public void IsValidRange_validates_forward_nonzero_nonnegative_ranges(int origin, int destination, bool expected)
    {
        SegmentOverlap.IsValidRange(new StationRange(origin, destination)).Should().Be(expected);
    }

    [Fact]
    public void Legs_enumerates_each_unit_leg_the_range_spans()
    {
        SegmentOverlap.Legs(new StationRange(9, 14)).Should().BeEquivalentTo([9, 10, 11, 12, 13], options => options.WithStrictOrdering());
    }
}
