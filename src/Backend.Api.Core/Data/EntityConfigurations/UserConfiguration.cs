using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);

        builder.HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(user => user.CongregationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
