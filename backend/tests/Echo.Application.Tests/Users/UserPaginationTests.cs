using Echo.Application.Users;
using Echo.Domain.Users;

namespace Echo.Application.Tests.Users;

[Trait("Category", "Unit")]
public class UserPaginationTests
{
    private static User Named(string name, string id)
    {
        return new User { Id = Guid.Parse(id), Name = name };
    }

    [Fact]
    public void Paginate_Cursor_KeepsUsersAfterItByNameThenId()
    {
        var cursor = new UserCursor { Name = "Kofi", Id = Guid.Parse("00000000-0000-0000-0000-000000000005") };
        var before = Named("Kofi", "00000000-0000-0000-0000-000000000004");
        var after = Named("Kofi", "00000000-0000-0000-0000-000000000006");
        var earlier = Named("Ama", "00000000-0000-0000-0000-000000000009");
        var later = Named("Yaw", "00000000-0000-0000-0000-000000000001");

        var res = new[] { before, after, earlier, later }.AsQueryable().Paginate(cursor, 10).ToList();

        Assert.Equal([after, later], res);
    }
}
