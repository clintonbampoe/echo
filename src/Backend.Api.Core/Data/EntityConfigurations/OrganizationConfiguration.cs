using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class OrganizationConfiguration : CongregationEntityConfigurationBase<Organization>
{
    public override void ConfigureEntity(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.Id);
    }
}
