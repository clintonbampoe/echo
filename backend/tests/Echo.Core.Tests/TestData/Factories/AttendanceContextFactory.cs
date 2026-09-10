using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Tests.TestData.Factories;

public static class AttendanceContextFactory
{
    public static AttendanceContext NewEntity()
    {
        return new AttendanceContext()
        {
            Id = Constants.DefaultInt,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            AttendanceTypeId = Constants.DefaultInt,
            AttendanceType = AttendanceTypeFactory.NewEntity(),
            Name = "AttendanceContext-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<AttendanceContext> NewEntityList()
    {
        var res = new List<AttendanceContext>();
        for (int i = 0; i < 5; i++)
        {
            res.Add(NewEntity());
        }

        return res;
    }

    public static AttendanceContextCreateDto NewCreateDto()
    {
        return new AttendanceContextCreateDto()
        {
            Name = "AttendanceContextCreate-01",
            AttendanceTypeId = Constants.DefaultInt,
        };
    }

    public static AttendanceContextUpdateDto NewUpdateDto()
    {
        return new AttendanceContextUpdateDto() { Name = "AttendanceContextUpdate-01" };
    }

    public static AttendanceContextUpdateDto NewUpdateDtoWithNullFields()
    {
        return new AttendanceContextUpdateDto() { Name = null };
    }
}
