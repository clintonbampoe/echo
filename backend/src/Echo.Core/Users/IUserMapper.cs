using Echo.Domain.Users;

namespace Echo.Core.Users;

public interface IUserMapper
{
    UserResponseDto ToDto(User entity);
    User ToEntity(UserCreateDto dto);
    UserAuthDto ToAuthDto(User entity);
    List<UserResponseDto> ToListDto(List<User> entities);

    List<UserSearchResultDto> ToSearchDto(List<User> entities);
    void Patch(UserUpdateDto dto, User entity);
}
