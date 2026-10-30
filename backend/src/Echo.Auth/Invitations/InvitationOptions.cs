using System.ComponentModel.DataAnnotations;

namespace Echo.Auth.Invitations;

public class InvitationOptions
{
    public const string SectionName = "Invitations";

    /// <summary>Base64 of the 32-byte key that encrypts invitation codes so they can be shown again.</summary>
    [Required(ErrorMessage = "Invitations:CodeKey must be set.")]
    public string CodeKey { get; init; } = string.Empty;
}
