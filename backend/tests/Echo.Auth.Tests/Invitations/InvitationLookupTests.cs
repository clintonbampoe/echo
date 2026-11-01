using Echo.Auth.Invitations;
using Echo.Domain.Auth;
using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Echo.Auth.Tests.Invitations;

[Trait("Category", "Unit")]
public class InvitationLookupTests
{
    private static readonly DateTime Now = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

    private static InvitationToken Token(
        DateTime? expiresAt = null,
        bool revoked = false,
        bool neverExpires = false,
        int useCount = 0,
        int? maxUses = null
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            CongregationId = Guid.NewGuid(),
            TokenHash = Guid.NewGuid().ToString(),
            AllowedRole = UserRole.Clerk,
            ExpiresAt = neverExpires ? null : expiresAt ?? Now.AddDays(10),
            IsRevoked = revoked,
            UseCount = useCount,
            MaxUses = maxUses,
            Congregation = new Congregation
            {
                Name = "Grace Chapel",
                City = "Kumasi",
                Region = Region.Ashanti,
            },
        };

    [Fact]
    public void AnActiveInvitation_ShowsTheCongregationAndRole()
    {
        var expiresAt = Now.AddDays(3);

        var result = InvitationService.ToLookupResult(Token(expiresAt), Now).ToResult();

        var dto = Assert.IsType<Ok<InvitationLookupDto>>(result).Value!;
        Assert.Equal("Grace Chapel", dto.CongregationName);
        Assert.Equal("Kumasi", dto.City);
        Assert.Equal(Region.Ashanti, dto.Region);
        Assert.Equal(UserRole.Clerk, dto.AllowedRole);
        Assert.Equal(expiresAt, dto.ExpiresAt);
    }

    [Fact]
    public void AnInvitationThatNeverExpires_HasNoExpiry()
    {
        var result = InvitationService.ToLookupResult(Token(neverExpires: true), Now).ToResult();

        Assert.Null(Assert.IsType<Ok<InvitationLookupDto>>(result).Value!.ExpiresAt);
    }

    [Fact]
    public void AnInvitationWithUsesLeft_IsStillShown()
    {
        var result = InvitationService.ToLookupResult(Token(useCount: 2, maxUses: 3), Now).ToResult();

        Assert.IsType<Ok<InvitationLookupDto>>(result);
    }

    [Fact]
    public void AnUnknownCode_IsNotFound() =>
        AssertNotFound(InvitationService.ToLookupResult(null, Now));

    [Fact]
    public void AnExpiredInvitation_IsNotFound() =>
        AssertNotFound(InvitationService.ToLookupResult(Token(Now.AddDays(-1)), Now));

    [Fact]
    public void ARevokedInvitation_IsNotFound() =>
        AssertNotFound(InvitationService.ToLookupResult(Token(revoked: true), Now));

    [Fact]
    public void AnInvitationWithNoUsesLeft_IsNotFound() =>
        AssertNotFound(InvitationService.ToLookupResult(Token(useCount: 3, maxUses: 3), Now));

    [Fact]
    public void EveryReasonForRefusing_GivesTheSameAnswer()
    {
        var answers = new[]
        {
            InvitationService.ToLookupResult(null, Now),
            InvitationService.ToLookupResult(Token(Now.AddDays(-1)), Now),
            InvitationService.ToLookupResult(Token(revoked: true), Now),
            InvitationService.ToLookupResult(Token(useCount: 3, maxUses: 3), Now),
        }.Select(r => Assert.IsType<ProblemHttpResult>(r.ToResult()).ProblemDetails.Detail);

        Assert.Single(answers.Distinct());
    }

    private static void AssertNotFound(IOperationResult result)
    {
        var problem = Assert.IsType<ProblemHttpResult>(result.ToResult());
        Assert.Equal(404, problem.StatusCode);
    }
}
