using Echo.Application.Options.Frontend;
using Echo.Application.Services;
using Microsoft.Extensions.Options;

namespace Echo.Auth.Services;

public class AuthLinkBuilder(IOptions<FrontendOptions> options) : LinkBuilder
{
    private readonly string _baseUrl = options.Value.BaseUrl;

    public string BuildEmailVerificationLink(string token)
    {
        var link = $"{_baseUrl}/verify-email?token={Uri.EscapeDataString(token)}";
        return link;
    }

    public string BuildPasswordResetLink(string token)
    {
        var link = $"{_baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        return link;
    }
}
