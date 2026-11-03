using System.ComponentModel.DataAnnotations;

namespace Echo.Shared.Options.Mail;

public class MailClientOptions : IAppSettingsOptions
{
    public static readonly string Section = "MailClient";

    string IAppSettingsOptions.Section => Section;

    [Required(ErrorMessage = "MailClient:Address must be set.")]
    [EmailAddress(ErrorMessage = "MailClient:Address must be a valid email address.")]
    public required string Address { get; init; }
}
