using Api.Core.Dtos.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Dtos;

public record InvitationTokenCreateDto(
    Guid CreatedByUserId,
    UserRole AllowedRole,
    string Token,
    DateTime ExpiryDate,
    bool IsRevoked
) : IPrimaryCreateDto;

public record InvitationTokenListResponseDto(
    Guid Id,
    string CreatedByUserEmail,
    UserRole AllowedRole,
    string Token,
    bool IsRevoked
) : IPrimaryListResponseDto;

public record InvitationTokenResponseDto(
    Guid Id,
    string Congregation,
    string CreatedByUserEmail,
    UserRole AllowedRole,
    string Token,
    DateTime ExpiryDate,
    bool IsRevoked,
    DateTime CreatedAt
) : IPrimaryResponseDto;
