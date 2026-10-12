using Echo.Application.Attendances;
using Echo.Application.Tests.Congregations;
using Echo.Domain.Attendances;

namespace Echo.Application.Tests.Attendances;

public static class SessionLabelFactory
{
    public static SessionLabel NewEntity()
    {
        return new SessionLabel
        {
            Id = Constants.DefaultInt,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            Name = "Sunday Service",
            Category = SessionCategory.Service,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<SessionLabel> NewEntityList()
    {
        var res = new List<SessionLabel>();
        for (int i = 0; i < 5; i++)
            res.Add(NewEntity());
        return res;
    }

    public static SessionLabelCreateDto NewCreateDto()
    {
        return new SessionLabelCreateDto
        {
            Name = "SessionLabelCreate-01",
            Category = SessionCategory.Meeting,
        };
    }

    public static SessionLabelUpdateDto NewUpdateDto()
    {
        return new SessionLabelUpdateDto { Name = "SessionLabelUpdate-01" };
    }

    public static SessionLabelUpdateDto NewUpdateDtoWithNullFields()
    {
        return new SessionLabelUpdateDto { Name = null };
    }
}
