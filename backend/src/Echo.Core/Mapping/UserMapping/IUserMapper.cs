using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.UserMapping;

public interface IUserMapper
{
    UserResponseDto ToDto(User entity);
    User ToEntity(UserCreateDto dto);
    void Patch(UserUpdateDto dto, User entity);
}
