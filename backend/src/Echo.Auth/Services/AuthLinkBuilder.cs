using Echo.Application.Configuration;
using Echo.Application.Services;
using Microsoft.Extensions.Options;

namespace Echo.Auth.Services;

public class AuthLinkBuilder(IOptions<FrontendClientOptions> options) : LinkBuilder(options)
{
    public string BuildEmailVerificationLink(string rawToken)
    {
        var link = $"{BaseUrl}/verify-email?token={Uri.EscapeDataString(rawToken)}";
        return link;
    }

    public string BuildPasswordResetLink(string rawToken)
    {
        var link = $"{BaseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";
        return link;
    }
}
