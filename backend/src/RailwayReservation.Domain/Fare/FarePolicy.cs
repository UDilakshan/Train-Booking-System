namespace RailwayReservation.Domain.Fare;

/// <summary>
/// Non-tabular fare policy knobs that aren't naturally rows in FareRule (which prices *amounts*,
/// not *time windows*). Kept isolated here, rather than scattered through the fare engine, so a
/// future iteration can promote this to an admin-editable policy table without touching modifier
/// logic — see README "Future Improvements".
/// </summary>
public static class FarePolicy
{
    private static readonly (int StartMinute, int EndMinute)[] PeakWindows =
    [
        (6 * 60, 9 * 60),   // 06:00-09:00
        (16 * 60, 19 * 60), // 16:00-19:00
    ];

    public static bool IsPeakDepartureTime(string departureTime)
    {
        var parts = departureTime.Split(':');
        var totalMinutes = int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
        return PeakWindows.Any(w => totalMinutes >= w.StartMinute && totalMinutes < w.EndMinute);
    }
}
