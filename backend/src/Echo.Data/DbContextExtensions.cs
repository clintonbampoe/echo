using Microsoft.EntityFrameworkCore;

namespace Echo.Data;

public static class DbContextExtensions
{
    public static void ConfigureEchoDatabase(this DbContextOptionsBuilder options)
    {
        options.UseSnakeCaseNamingConvention();
    }
}
