using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.UserMapping;

public interface IUserMapper
{
    UserResponseDto ToDto(User entity);
    User ToEntity(UserCreateDto dto);
    UserAuthDto ToAuthDto(User entity);
    List<UserResponseDto> ToListDto(List<User> entities);

    List<UserSearchResultDto> ToSearchDto(List<User> entities);
    void Patch(UserUpdateDto dto, User entity);
}
