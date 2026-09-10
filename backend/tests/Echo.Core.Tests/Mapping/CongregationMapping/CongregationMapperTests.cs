using Echo.Core.Mapping.CongregationMapping;
using Echo.Core.Tests.TestData.Factories;

namespace Echo.Core.Tests.Mapping.CongregationMapping;

public class CongregationMapperTests
{
    private readonly ICongregationMapper _mapper = new CongregationMapper();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = CongregationFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.OrgType, entity.OrgType);
        Assert.Equal(dto.PhoneNumber, entity.PhoneNumber);
        Assert.Equal(dto.EmailAddress, entity.EmailAddress);
        Assert.Equal(dto.PostalAddress, entity.PostalAddress);
        Assert.Equal(dto.WebsiteUrl, entity.WebsiteUrl);
        Assert.Equal(dto.Region, entity.Region);
        Assert.Equal(dto.City, entity.City);
        Assert.Equal(dto.Town, entity.Town);
        Assert.Equal(dto.GpsAddress, entity.GpsAddress);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = CongregationFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = CongregationFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.Name, dto.Name);
        Assert.Equal(entity.OrgType, dto.OrgType);
        Assert.Equal(entity.PhoneNumber, dto.PhoneNumber);
        Assert.Equal(entity.EmailAddress, dto.EmailAddress);
        Assert.Equal(entity.PostalAddress, dto.PostalAddress);
        Assert.Equal(entity.WebsiteUrl, dto.WebsiteUrl);
        Assert.Equal(entity.Region, dto.Region);
        Assert.Equal(entity.City, dto.City);
        Assert.Equal(entity.Town, dto.Town);
        Assert.Equal(entity.GpsAddress, dto.GpsAddress);
        Assert.Equal(entity.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public void ToDto_ShouldNotMap_DeletedAt_FromEntity()
    {
        var entity = CongregationFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.False(dto.GetType().GetProperty("DeletedAt") != null);
    }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = CongregationFactory.NewUpdateDto();
        var entity = CongregationFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.OrgType, entity.OrgType);
        Assert.Equal(dto.PhoneNumber, entity.PhoneNumber);
        Assert.Equal(dto.EmailAddress, entity.EmailAddress);
        Assert.Equal(dto.PostalAddress, entity.PostalAddress);
        Assert.Equal(dto.WebsiteUrl, entity.WebsiteUrl);
        Assert.Equal(dto.Region, entity.Region);
        Assert.Equal(dto.City, entity.City);
        Assert.Equal(dto.Town, entity.Town);
        Assert.Equal(dto.GpsAddress, entity.GpsAddress);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = CongregationFactory.NewUpdateDtoWithNullFields();
        var entity = CongregationFactory.NewEntity();

        // snapshot original values before patch
        var original = CongregationFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.Name, entity.Name);
        Assert.Equal(original.OrgType, entity.OrgType);
        Assert.Equal(original.PhoneNumber, entity.PhoneNumber);
        Assert.Equal(original.EmailAddress, entity.EmailAddress);
        Assert.Equal(original.PostalAddress, entity.PostalAddress);
        Assert.Equal(original.WebsiteUrl, entity.WebsiteUrl);
        Assert.Equal(original.Region, entity.Region);
        Assert.Equal(original.City, entity.City);
        Assert.Equal(original.Town, entity.Town);
        Assert.Equal(original.GpsAddress, entity.GpsAddress);
    }

    [Fact]
    public void ToSearchDto_ShouldMapFields_FromEntityList()
    {
        var entityList = CongregationFactory.NewEntityList();
        var dtoList = _mapper.ToSearchDto(entityList);

        var firstEntity = entityList.First();
        var firstDto = dtoList.First();

        var lastEntity = entityList.Last();
        var lastDto = dtoList.Last();

        Assert.Equal(firstEntity.Id, firstDto.Id);
        Assert.Equal(firstEntity.Name, firstDto.Name);

        Assert.Equal(lastEntity.Id, lastDto.Id);
        Assert.Equal(lastEntity.Name, lastDto.Name);
    }
}
