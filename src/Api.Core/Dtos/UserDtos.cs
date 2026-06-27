using Api.Core.Dtos.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Dtos;

public record UserCreateDto(
    string Name,
    string UserName,
    string EmailAddress,
    string PasswordHash,
    UserRole Role
) : IPrimaryCreateDto;

public record UserUpdateDto(
    string Name,
    string UserName,
    string EmailAddress,
    string PasswordHash,
    UserRole Role
) : IPrimaryUpdateDto;

public record UserListResponseDto(Guid Id, string UserName, string EmailAddress, UserRole Role)
    : IPrimaryListResponseDto;

public record UserResponseDto(
    Guid Id,
    string Name,
    string UserName,
    string EmailAddress,
    string PasswordHash,
    UserRole Role,
    DateTime CreatedAt
) : IPrimaryResponseDto;
