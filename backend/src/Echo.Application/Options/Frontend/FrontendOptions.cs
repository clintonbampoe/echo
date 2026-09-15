using System.ComponentModel.DataAnnotations;

namespace Echo.Application.Options.Frontend;

public class FrontendOptions : IAppSettingsOptions
{
    public static readonly string Section = "Frontend";

    string IAppSettingsOptions.Section => Section;

    [Required(ErrorMessage = "FrontendClient:BaseUrl must be set.")]
    [Url(ErrorMessage = "FrontendClient:BaseUrl must be an absolute http(s) URL.")]
    public required string BaseUrl { get; init; }
}
