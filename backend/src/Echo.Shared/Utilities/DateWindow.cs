namespace Echo.Shared.Utilities;

public readonly record struct DateWindow(DateOnly From, DateOnly To, bool IsDefault)
{
    /// <summary>The window of the same number of days that ends the day before this one starts.</summary>
    public DateWindow Previous()
    {
        var days = To.DayNumber - From.DayNumber + 1;
        return this with { From = From.AddDays(-days), To = From.AddDays(-1) };
    }
}
