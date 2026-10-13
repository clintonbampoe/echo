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
        DateTime? createdAt = null,
        Guid? id = null
    ) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            CongregationId = Guid.NewGuid(),
            TokenHash = Guid.NewGuid().ToString(),
            ExpiresAt = expiresAt ?? Now.AddDays(10),
            IsRevoked = revoked,
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
}
