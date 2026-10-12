using Echo.Application.Attendances;
using Echo.Application.Tests.Congregations;
using Echo.Domain.Attendances;

namespace Echo.Application.Tests.Attendances;

public static class SessionFactory
{
    public static Session NewEntity()
    {
        return new Session
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            SessionLabelId = Constants.DefaultInt,
            SessionLabel = SessionLabelFactory.NewEntity(),
            Date = Constants.DefaultDateOnly,
            Name = "Session-01",
            Topic = "Topic-01",
            Notes = "Notes-01",
            StartsAt = Constants.DefaultTimeOnly,
            EndsAt = Constants.DefaultTimeOnly,
            Status = SessionStatus.Scheduled,
            CreatedBy = null,
            Creator = null,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<Session> NewEntityList()
    {
        var res = new List<Session>();
        for (int i = 0; i < 5; i++)
            res.Add(NewEntity());
        return res;
    }

    public static SessionCreateDto NewCreateDto()
    {
        return new SessionCreateDto
        {
            SessionLabelId = Constants.DefaultInt,
            Date = Constants.DefaultDateOnly,
            Name = "SessionCreate-01",
            Topic = "TopicCreate-01",
            Notes = "NotesCreate-01",
            StartsAt = Constants.DefaultTimeOnly,
            EndsAt = Constants.DefaultTimeOnly,
        };
    }

    public static SessionUpdateDto NewUpdateDto()
    {
        return new SessionUpdateDto
        {
            Name = "SessionUpdate-01",
            Topic = "TopicUpdate-01",
            Notes = "NotesUpdate-01",
            StartsAt = Constants.DefaultTimeOnly,
            EndsAt = Constants.DefaultTimeOnly,
            Status = SessionStatus.Completed,
        };
    }

    public static SessionUpdateDto NewUpdateDtoWithNullFields()
    {
        return new SessionUpdateDto
        {
            Name = null,
            Topic = null,
            Notes = null,
            StartsAt = null,
            EndsAt = null,
            Status = null,
        };
    }
}
