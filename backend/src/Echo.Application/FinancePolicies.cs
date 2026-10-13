using Echo.Domain.Users;
using Microsoft.AspNetCore.Authorization;

namespace Echo.Application;

/// <summary>
/// Role access for giving and finance data. Apply per route: a group-level policy is combined
/// with a route's own policy (both must pass), so a route cannot widen what its group requires.
/// </summary>
public static class FinancePolicies
{
    /// <summary>Read, update and delete finance records, and see totals: Admin and Accountant.</summary>
    public const string Manage = "FinanceManage";

    /// <summary>Record new finance entries: Admin, Accountant and Clerk.</summary>
    public const string Record = "FinanceRecord";

    public static AuthorizationOptions AddFinancePolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(
            Manage,
            policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireRole(nameof(UserRole.Admin), nameof(UserRole.Accountant))
        );

        options.AddPolicy(
            Record,
            policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        nameof(UserRole.Admin),
                        nameof(UserRole.Accountant),
                        nameof(UserRole.Clerk)
                    )
        );

        return options;
    }
}
