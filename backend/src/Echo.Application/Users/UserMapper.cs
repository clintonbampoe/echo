using Echo.Domain.Users;
using Riok.Mapperly.Abstractions;

namespace Echo.Application.Users;

[Mapper]
public partial class UserMapper : IUserMapper
{
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.PasswordHash))]
    [MapperIgnoreSource(nameof(entity.LastName))]
    [MapperIgnoreSource(nameof(entity.FirstName))]
    [MapperIgnoreSource(nameof(entity.OtherNames))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(nameof(entity.EmailVerifiedAt), nameof(UserResponseDto.VerifiedAt))]
    public partial UserResponseDto ToDto(User entity);

    [MapperIgnoreTarget(nameof(User.CongregationId))]
    [MapperIgnoreTarget(nameof(User.Id))]
    // Computed column; no CreateDto carries it.
    [MapperIgnoreTarget(nameof(User.Name))]
    [MapperIgnoreTarget(nameof(User.EmailVerifiedAt))]
    [MapperIgnoreTarget(nameof(User.PasswordChangedAt))]
    [MapperIgnoreTarget(nameof(User.CreatedAt))]
    [MapperIgnoreTarget(nameof(User.DeletedAt))]
    [MapProperty(nameof(dto.Password), nameof(User.PasswordHash))]
    public partial User ToEntity(UserCreateDto dto);

    [MapperIgnoreSource(nameof(entity.FirstName))]
    [MapperIgnoreSource(nameof(entity.LastName))]
    [MapperIgnoreSource(nameof(entity.OtherNames))]
    [MapperIgnoreSource(nameof(entity.CreatedAt))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    public partial UserAuthDto ToAuthDto(User entity);

    public partial List<UserResponseDto> ToListDto(List<User> entities);

    public List<UserSearchResultDto> ToSearchDto(List<User> entities)
    {
        var res = entities
            .Select(x => new UserSearchResultDto
            {
                Id = x.Id,
                Name = x.Name,
                EmailAddress = x.EmailAddress,
            })
            .ToList();
        return res;
    }

    public void Patch(UserUpdateDto dto, User entity)
    {
        if (dto.EmailAddress != null)
            entity.EmailAddress = dto.EmailAddress;
        if (dto.Role.HasValue)
            entity.Role = dto.Role.Value;
    }
}
