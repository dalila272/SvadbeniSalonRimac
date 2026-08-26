namespace SvadbeniSalon.Services;

public static class DnevniSastanakSlotRules
{
    public static readonly TimeSpan Duration = TimeSpan.FromMinutes(30);

    public const int WorkDayStartMinutes = 8 * 60;
    public const int WorkDayLastStartMinutes = 17 * 60 + 30;

    public static bool IsValidStartTime(DateTime dateTime)
    {
        if (dateTime.Minute is not (0 or 30) || dateTime.Second != 0 || dateTime.Millisecond != 0)
        {
            return false;
        }

        var minutes = dateTime.Hour * 60 + dateTime.Minute;
        return minutes >= WorkDayStartMinutes && minutes <= WorkDayLastStartMinutes;
    }

    public static bool Overlaps(DateTime startA, DateTime startB)
    {
        if (startA.Date != startB.Date)
        {
            return false;
        }

        var endA = startA.Add(Duration);
        var endB = startB.Add(Duration);
        return startA < endB && startB < endA;
    }
}
