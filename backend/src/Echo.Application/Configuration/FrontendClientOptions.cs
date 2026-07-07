namespace Echo.Application.Configuration;

public class FrontendClientOptions : IAppSettingsOptions
{
    public string SectionName { get; } = "FrontendClient";
    public required string BaseUrl { get; init; }
}
