using Echo.Application.HttpResults;
using Echo.Application.Services;
using Echo.Application.Services.Hashing;
using Echo.Auth.Dtos;
using Echo.Auth.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Echo.Domain.Enums;

namespace Echo.Auth.Services;

public class InvitationService(
    AppDbContext dbContext,
    InvitationTokenRepository invitationTokenRepository,
    ITokenGenerator tokenGenerator,
    ITokenHasher hashService,
    TimeProvider timeProvider
)
{
    private const int _defaultExpiryDays = 30;

    public async Task<IOperationResult> CreateInvitationAsync(
        Guid congregationId,
        Guid createdByUserId,
        UserRole allowedRole,
        int? expiryDays,
        CancellationToken ct = default
    )
    {
        var token = tokenGenerator.GenerateToken(8);

        var tokenEntity = new InvitationToken
        {
            CongregationId = congregationId,
            CreatedByUserId = createdByUserId,
            AllowedRole = allowedRole,
            TokenHash = await hashService.HashAsync(token),
            ExpiresAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(expiryDays ?? _defaultExpiryDays),
        };

        await invitationTokenRepository.CreateRecord(tokenEntity, ct);
        await dbContext.SaveChangesAsync(ct);

        return new SuccessResult<InviteResponseDto>(
            new InviteResponseDto
            {
                Token = token,
                AllowedRole = tokenEntity.AllowedRole,
                ExpiresAt = tokenEntity.ExpiresAt,
            }
        );
    }

    public async Task<InvitationToken?> ValidateAsync(string token, CancellationToken ct = default)
    {
        var hashedInput = await hashService.HashAsync(token);
        var tokenRecord = await invitationTokenRepository.GetTokenRecordByHash(hashedInput, ct);

        if (
            tokenRecord is null
            || tokenRecord.IsRevoked
            || tokenRecord.ExpiresAt <= timeProvider.GetUtcNow().UtcDateTime
        )
            return null;

        return tokenRecord;
    }
}
