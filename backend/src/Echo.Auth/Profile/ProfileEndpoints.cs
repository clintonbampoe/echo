using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Auth.Profile;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(
        this RouteGroupBuilder group,
        AuthInstrumentation instrumentation
    )
    {
        // Any signed-in user may read and edit their own profile; /users itself is Admin only.
        var me = group.MapGroup("/users/me").WithTags("Profile").RequireAuthorization();

        me.MapGet(
                "/",
                async (ProfileService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.profile.get"
                    );
                    var result = await service.Get(
                        context.User.GetCongregationId(),
                        context.User.GetUserId(),
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetMyProfile")
            .WithSummary("Returns the signed-in user's own profile.")
            .WithDescription(
                """
                Returns the profile of the user the bearer token belongs to: id, name, email address, role, when the email was verified, when the account was created and `passwordChangedAt` (when the password was last set or changed; `null` for accounts that have not changed it since this field was added).

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — the account no longer exists.
                """
            )
            .Produces<Application.Users.UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        me.MapPut(
                "/",
                async (
                    [FromBody] ProfileUpdateDto dto,
                    ProfileService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.profile.update"
                    );
                    var result = await service.Update(
                        context.User.GetCongregationId(),
                        context.User.GetUserId(),
                        dto,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .RequireRateLimiting("auth")
            .WithName("UpdateMyProfile")
            .WithSummary("Updates the signed-in user's own name or password.")
            .WithDescription(
                """
                Changes the caller's own details. Only the fields that are sent change.

                ### Request body (all optional)
                - `firstName`, `lastName` — cannot be blank when sent.
                - `otherNames` — send an empty string to clear it.
                - `currentPassword` and `newPassword` — to change the password, send **both**. The current password must be correct and the new one must meet the password policy.

                The role and the email address cannot be changed here. Changing the password sets `passwordChangedAt` and ends the user's sessions: their refresh tokens stop working, and the access token they hold expires on its own (within about 15 minutes), after which they must log in again.

                ### Failure modes
                - `400 BAD_REQUEST` — a blank name, only one of the two password fields, a weak new password, or a wrong current password.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — the account no longer exists.
                - `429` — too many attempts; this route shares the limit of the other password routes.
                """
            )
            .Produces<Application.Users.UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
