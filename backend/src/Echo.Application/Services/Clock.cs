namespace Echo.Application.Services;

public static class Clock
{
    /// <summary>
    /// Returns the current UTC time offset by the given duration.
    /// </summary>
    public static DateTime UtcNowPlus(TimeSpan duration) =>
        DateTime.UtcNow + duration;
}
