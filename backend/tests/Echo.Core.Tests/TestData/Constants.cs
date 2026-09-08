namespace Echo.Core.Tests.TestData;

public static class Constants
{
    public static readonly Guid DefaultGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly int DefaultInt = 0;

    public static readonly DateTime DefaultDateTime = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    public static readonly DateOnly DefaultDateOnly = DateOnly.FromDateTime(DefaultDateTime);
    public static readonly TimeOnly DefaultTimeOnly = TimeOnly.FromDateTime(DefaultDateTime);

    public const string DefaultEmailAddress = "user@email.com";
    public const string DefaultName = "John Doe";

    public const string DefaultCity = "City";
    public const string DefaultTown = "Town";
    public const string DefaultLocation = "Location";

    public const string DefaultPhoneNumber = "00000000";
    public const string DefaultPostalAddress = "P.O. BOX 111";
    public const string DefaultGpsAddress = "GW-0000-1111";
    public const string DefaultWebsiteUrl = "url@website.com";
}
