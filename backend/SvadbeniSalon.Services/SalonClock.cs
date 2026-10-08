namespace SvadbeniSalon.Services;

/// <summary>
/// Wall-clock for salon business rules (termini, plaćanja).
/// Docker containers typically run in UTC; clients send local BH times without offset,
/// so comparisons must use Europe/Sarajevo — not <see cref="DateTime.Now"/> / UTC.
/// </summary>
public static class SalonClock
{
    /// <summary>Minimalni broj dana unaprijed za rezervaciju svadbe.</summary>
    public const int MinWeddingLeadDays = 3;

    private static readonly TimeZoneInfo Zone = ResolveZone();

    public static DateTime Now =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);

    public static DateTime Today => Now.Date;

    /// <summary>Najraniji dozvoljeni datum svadbe (danas + lead days).</summary>
    public static DateTime MinWeddingBookingDate =>
        Today.AddDays(MinWeddingLeadDays);

    public static DateTime Combine(DateTime date, TimeSpan time) =>
        date.Date.Add(time);

    public static bool IsInPast(DateTime date, TimeSpan time) =>
        Combine(date, time) <= Now;

    public static bool IsInFuture(DateTime date, TimeSpan time) =>
        Combine(date, time) > Now;

    private static TimeZoneInfo ResolveZone()
    {
        foreach (var id in new[] { "Europe/Sarajevo", "Central European Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Local;
    }
}
