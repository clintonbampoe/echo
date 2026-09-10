using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Tests.TestData.Factories;

public static class AttendanceTypeFactory
{
    public static AttendanceType NewEntity()
    {
        return new AttendanceType()
        {
            Id = Constants.DefaultInt,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            Name = "AttendanceType-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<AttendanceType> NewEntityList()
    {
        var res = new List<AttendanceType>();
        for (int i = 0; i < 5; i++)
        {
            res.Add(NewEntity());
        }

        return res;
    }

    public static AttendanceTypeCreateDto NewCreateDto()
    {
        return new AttendanceTypeCreateDto() { Name = "AttendanceTypeCreate-01" };
    }

    public static AttendanceTypeUpdateDto NewUpdateDto()
    {
        return new AttendanceTypeUpdateDto() { Name = "AttendanceTypeUpdate-01" };
    }

    public static AttendanceTypeUpdateDto NewUpdateDtoWithNullFields()
    {
        return new AttendanceTypeUpdateDto() { Name = null };
    }
}
