using Echo.Application.HttpResults;
using Echo.Application.Services.Generators;
using Echo.Application.Services.Hashing;
using Echo.Auth.Dtos;
using Echo.Auth.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Echo.Domain.Enums;

namespace Echo.Auth.Services;

public class InvitationService(
    InvitationTokenRepository invitationTokenRepository,
    ITokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    ITokenHasher hashService,
    TimeProvider timeProvider
)
{
    private const int _defaultExpiryDays = 30;

    public async Task<IOperationResult> CreateInvitationToken(
        Guid congregationId,
        Guid createdByUserId,
        UserRole allowedRole,
        int? expiryDays,
        CancellationToken ct = default
    )
    {
        var token = tokenGenerator.GenerateToken();

        var tokenEntity = new InvitationToken
        {
            CongregationId = congregationId,
            CreatedByUserId = createdByUserId,
            AllowedRole = allowedRole,
            TokenHash = hashService.Hash(token),
            ExpiresAt = timeProvider
                .GetUtcNow()
                .UtcDateTime.AddDays(expiryDays ?? _defaultExpiryDays),
        };

        await invitationTokenRepository.Create(tokenEntity, ct);
        await unitOfWork.CommitAsync(ct);

        return new SuccessResult<InviteResponseDto>(
            new InviteResponseDto
            {
                Token = token,
                AllowedRole = tokenEntity.AllowedRole,
                ExpiresAt = tokenEntity.ExpiresAt,
            }
        );
    }

    public async Task<InvitationToken?> Validate(string token, CancellationToken ct = default)
    {
        var hashedInput = hashService.Hash(token);
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
