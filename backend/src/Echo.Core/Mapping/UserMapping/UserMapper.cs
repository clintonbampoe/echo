using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.UserMapping;

[Mapper]
public partial class UserMapper : IUserMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.PasswordHash))]
    [MapperIgnoreSource(nameof(entity.LastName))]
    [MapperIgnoreSource(nameof(entity.FirstName))]
    [MapperIgnoreSource(nameof(entity.OtherNames))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(nameof(entity.EmailVerifiedAt), nameof(UserResponseDto.VerifiedAt))]
    public partial UserResponseDto ToDto(User entity);

    [MapperIgnoreTarget(nameof(User.Congregation))]
    [MapperIgnoreTarget(nameof(User.CongregationId))]
    [MapperIgnoreTarget(nameof(User.Id))]
    [MapperIgnoreTarget(nameof(User.EmailVerifiedAt))]
    [MapperIgnoreTarget(nameof(User.CreatedAt))]
    [MapperIgnoreTarget(nameof(User.DeletedAt))]
    [MapProperty(nameof(dto.Password), nameof(User.PasswordHash))]
    public partial User ToEntity(UserCreateDto dto);

    public void Patch(UserUpdateDto dto, User entity)
    {
        if (dto.EmailAddress != null) entity.EmailAddress = dto.EmailAddress;
        if (dto.Password != null) entity.PasswordHash = dto.Password; // Note: In real app, this would be hashed
        if (dto.Role.HasValue) entity.Role = dto.Role.Value;
    }
}
