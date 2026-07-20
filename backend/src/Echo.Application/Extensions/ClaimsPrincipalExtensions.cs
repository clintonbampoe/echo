using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Echo.Domain.Enums;

namespace Echo.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? throw new InvalidOperationException("Token is missing a 'sub' claim");

        return Guid.Parse(value);
    }

    public static UserRole GetUserRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("role")
                    ?? throw new InvalidOperationException("Token is missing a 'role' claim");

        return Enum.Parse<UserRole>(value);
    }

    public static Guid GetCongregationId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("congregationId")
                    ?? throw new InvalidOperationException("Token is missing 'congregationId' claim");

        return Guid.Parse(value);
    }
}
