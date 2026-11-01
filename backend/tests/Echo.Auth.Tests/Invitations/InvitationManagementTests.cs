using Echo.Auth.Invitations;
using Echo.Domain.Auth;
using Echo.Shared.Services.Encoders;

namespace Echo.Auth.Tests.Invitations;

[Trait("Category", "Unit")]
public class InvitationManagementTests
{
    private static readonly DateTime Now = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

    private static InvitationToken Token(
        DateTime? expiresAt = null,
        bool revoked = false,
        bool neverExpires = false,
        int useCount = 0,
        int? maxUses = null,
        DateTime? createdAt = null,
        Guid? id = null
    ) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            CongregationId = Guid.NewGuid(),
            TokenHash = Guid.NewGuid().ToString(),
            ExpiresAt = neverExpires ? null : expiresAt ?? Now.AddDays(10),
            IsRevoked = revoked,
            UseCount = useCount,
            MaxUses = maxUses,
            CreatedAt = createdAt ?? Now.AddDays(-1),
        };

    [Fact]
    public void StatusAt_IsActive_UntilTheExpiryInstant()
    {
        Assert.Equal(InvitationStatus.Active, Token(Now.AddSeconds(1)).StatusAt(Now));
        Assert.Equal(InvitationStatus.Expired, Token(Now).StatusAt(Now));
        Assert.Equal(InvitationStatus.Expired, Token(Now.AddDays(-3)).StatusAt(Now));
    }

    [Fact]
    public void StatusAt_RevokedWinsOverExpired()
    {
        Assert.Equal(InvitationStatus.Revoked, Token(Now.AddDays(-3), revoked: true).StatusAt(Now));
        Assert.Equal(InvitationStatus.Revoked, Token(Now.AddDays(3), revoked: true).StatusAt(Now));
    }

    [Fact]
    public void WithStatus_KeepsOnlyInvitationsInThatStatus()
    {
        var active = Token(Now.AddDays(5));
        var expired = Token(Now.AddDays(-5));
        var revoked = Token(Now.AddDays(5), revoked: true);
        var all = new[] { active, expired, revoked }.AsQueryable();

        Assert.Equal([active], all.WithStatus(InvitationStatus.Active, Now));
        Assert.Equal([expired], all.WithStatus(InvitationStatus.Expired, Now));
        Assert.Equal([revoked], all.WithStatus(InvitationStatus.Revoked, Now));
        Assert.Equal(3, all.WithStatus(null, Now).Count());
    }

    [Fact]
    public void Paginate_ContinuesAfterTheCursor_NewestFirst()
    {
        var a = Token(createdAt: Now.AddDays(-1), id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var b = Token(createdAt: Now.AddDays(-2), id: Guid.Parse("00000000-0000-0000-0000-000000000002"));
        var c = Token(createdAt: Now.AddDays(-3), id: Guid.Parse("00000000-0000-0000-0000-000000000003"));
        var ordered = new[] { c, a, b }.AsQueryable().OrderByDescending(t => t.CreatedAt).ThenBy(t => t.Id);

        var firstPage = ordered.Paginate(null, 2).ToList();
        var secondPage = ordered.Paginate(new InvitationCursor { CreatedAt = b.CreatedAt, Id = b.Id }, 2).ToList();

        Assert.Equal([a, b], firstPage);
        Assert.Equal([c], secondPage);
    }

    [Fact]
    public void ApplyUpdate_SetsATrimmedLabel_AndAnEmptyLabelClearsIt()
    {
        var token = Token();

        Assert.Null(InvitationService.ApplyUpdate(token, new InvitationUpdateDto { Label = "  Youth group  " }, Now));
        Assert.Equal("Youth group", token.Label);

        Assert.Null(InvitationService.ApplyUpdate(token, new InvitationUpdateDto { Label = "" }, Now));
        Assert.Null(token.Label);
    }

    [Fact]
    public void ApplyUpdate_ExpiryDaysCountFromNow()
    {
        var token = Token(Now.AddDays(-2)); // already expired

        var error = InvitationService.ApplyUpdate(token, new InvitationUpdateDto { ExpiryDays = 7 }, Now);

        Assert.Null(error);
        Assert.Equal(Now.AddDays(7), token.ExpiresAt);
    }

    [Fact]
    public void ApplyUpdate_ARevokedInvitationCannotBeExtended_ButCanBeRelabelled()
    {
        var token = Token(revoked: true);

        var extend = InvitationService.ApplyUpdate(token, new InvitationUpdateDto { ExpiryDays = 7 }, Now);
        var relabel = InvitationService.ApplyUpdate(token, new InvitationUpdateDto { Label = "Old link" }, Now);

        Assert.Contains("revoked", extend, StringComparison.OrdinalIgnoreCase);
        Assert.Null(relabel);
        Assert.Equal("Old link", token.Label);
    }

    [Fact]
    public void ApplyUpdate_NothingSent_ChangesNothing()
    {
        var token = Token();
        var before = (token.Label, token.ExpiresAt);

        Assert.Null(InvitationService.ApplyUpdate(token, new InvitationUpdateDto(), Now));

        Assert.Equal(before, (token.Label, token.ExpiresAt));
    }

    [Fact]
    public void Revoke_IsIdempotent()
    {
        var token = Token();

        InvitationService.ApplyRevoke(token);
        InvitationService.ApplyRevoke(token);

        Assert.True(token.IsRevoked);
    }

    [Fact]
    public void StatusAt_NeverExpiring_StaysActive()
    {
        Assert.Equal(InvitationStatus.Active, Token(neverExpires: true).StatusAt(Now.AddYears(50)));
    }

    [Fact]
    public void StatusAt_UsedUp_IsExhausted_ButExpiredAndRevokedWin()
    {
        Assert.Equal(InvitationStatus.Active, Token(useCount: 2, maxUses: 3).StatusAt(Now));
        Assert.Equal(InvitationStatus.Exhausted, Token(useCount: 3, maxUses: 3).StatusAt(Now));
        Assert.Equal(InvitationStatus.Expired, Token(Now.AddDays(-1), useCount: 3, maxUses: 3).StatusAt(Now));
        Assert.Equal(InvitationStatus.Revoked, Token(revoked: true, useCount: 3, maxUses: 3).StatusAt(Now));
        Assert.Equal(InvitationStatus.Active, Token(useCount: 500).StatusAt(Now));
    }

    [Fact]
    public void WithStatus_AgreesWithStatusAt_ForLimitsAndNoExpiry()
    {
        var tokens = new[]
        {
            Token(),
            Token(neverExpires: true),
            Token(useCount: 1, maxUses: 3),
            Token(useCount: 3, maxUses: 3),
            Token(neverExpires: true, useCount: 1, maxUses: 1),
            Token(Now.AddDays(-1)),
            Token(revoked: true),
        };
        var all = tokens.AsQueryable();

        foreach (var status in Enum.GetValues<InvitationStatus>())
            Assert.Equal(
                tokens.Where(t => t.StatusAt(Now) == status),
                all.WithStatus(status, Now)
            );
    }

    [Fact]
    public void ApplyUpdate_NeverExpires_ClearsTheExpiry()
    {
        var token = Token();

        Assert.Null(InvitationService.ApplyUpdate(token, new InvitationUpdateDto { NeverExpires = true }, Now));

        Assert.Null(token.ExpiresAt);
    }

    [Fact]
    public void ApplyUpdate_ExpiryDaysAndNeverExpires_AreRejected_AndChangeNothing()
    {
        var token = Token(neverExpires: true);

        var problem = InvitationService.ApplyUpdate(
            token,
            new InvitationUpdateDto { ExpiryDays = 5, NeverExpires = true, Label = "x" },
            Now
        );

        Assert.Contains("not both", problem);
        Assert.Null(token.Label);
        Assert.Null(token.ExpiresAt);
    }

    [Fact]
    public void ApplyUpdate_MaxUses_CannotGoBelowThoseWhoJoined()
    {
        var token = Token(useCount: 4, maxUses: 10);

        Assert.Contains("4 people", InvitationService.ApplyUpdate(token, new InvitationUpdateDto { MaxUses = 3 }, Now));
        Assert.Equal(10, token.MaxUses);

        Assert.Null(InvitationService.ApplyUpdate(token, new InvitationUpdateDto { MaxUses = 4 }, Now));
        Assert.Equal(4, token.MaxUses);
        Assert.Equal(InvitationStatus.Exhausted, token.StatusAt(Now));
    }

    [Fact]
    public void ApplyUpdate_RaisingMaxUses_ReactivatesAUsedUpInvitation()
    {
        var token = Token(useCount: 3, maxUses: 3);

        Assert.Null(InvitationService.ApplyUpdate(token, new InvitationUpdateDto { MaxUses = 5 }, Now));

        Assert.Equal(InvitationStatus.Active, token.StatusAt(Now));
    }

    [Fact]
    public void ApplyUpdate_ARevokedInvitation_CannotGetMoreUsesOrNoExpiry()
    {
        Assert.NotNull(InvitationService.ApplyUpdate(Token(revoked: true), new InvitationUpdateDto { MaxUses = 9 }, Now));
        Assert.NotNull(InvitationService.ApplyUpdate(Token(revoked: true), new InvitationUpdateDto { NeverExpires = true }, Now));
        Assert.Null(InvitationService.ApplyUpdate(Token(revoked: true), new InvitationUpdateDto { Label = "x" }, Now));
    }

    [Fact]
    public void ExpiryFor_DefaultsTo30Days_AndNeverMeansNoDate()
    {
        Assert.Equal(Now.AddDays(30), InvitationService.ExpiryFor(Now, null, false));
        Assert.Equal(Now.AddDays(90), InvitationService.ExpiryFor(Now, 90, false));
        Assert.Null(InvitationService.ExpiryFor(Now, null, true));
    }

    [Fact]
    public void ExpiryProblem_OnlyWhenBothAreSent()
    {
        Assert.Null(InvitationService.ExpiryProblem(null, true));
        Assert.Null(InvitationService.ExpiryProblem(7, false));
        Assert.NotNull(InvitationService.ExpiryProblem(7, true));
    }
}
