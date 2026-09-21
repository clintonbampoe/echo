using Echo.Application.Options.Frontend;
using Microsoft.Extensions.Options;

namespace Echo.Auth;

public class LinkBuilder(IOptions<FrontendOptions> options)
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
