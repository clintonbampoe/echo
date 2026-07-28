using Echo.Application.HttpResults;
using Echo.Application.Services;
using Echo.Application.Services.Hashing;
using Echo.Auth.Dtos;
using Echo.Auth.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Echo.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Services;

public class InvitationService(
    AppDbContext dbContext,
    InvitationTokenRepository invitationTokenRepository,
    ITokenGenerator tokenGenerator,
    [FromKeyedServices("Sha256")] IHashService tokenHashService)
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
            TokenHash = await tokenHashService.HashPasswordAsync(token),
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays ?? _defaultExpiryDays),
        };

        await invitationTokenRepository.CreateRecord(tokenEntity, ct);
        await dbContext.SaveChangesAsync(ct);

        return new SuccessResult<InviteResponseDto>(new InviteResponseDto
        {
            Token = token,
            AllowedRole = tokenEntity.AllowedRole,
            ExpiresAt = tokenEntity.ExpiresAt
        });
    }

    public async Task<InvitationToken?> ValidateAsync(string token, CancellationToken ct = default)
    {
        var hashedInput = await tokenHashService.HashPasswordAsync(token);
        var tokenRecord = await invitationTokenRepository.GetTokenRecordByHash(hashedInput, ct);

        if (tokenRecord is null || tokenRecord.IsRevoked || tokenRecord.ExpiresAt <= DateTime.UtcNow)
            return null;

        return tokenRecord;
    }
}
