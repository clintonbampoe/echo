using System.ComponentModel.DataAnnotations;

namespace Echo.Shared.Options.Api;

public class ApiOptions : IAppSettingsOptions
{
    public static string SectionName => "Api";

    public string Section => SectionName;

    [Required]
    public required string BaseUrl { get; init; }

    public string ErrorBaseUrl => $"{BaseUrl}/errors";
}
