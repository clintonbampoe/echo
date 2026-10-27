using Echo.Domain.Attendances;

namespace Echo.Application.Attendances;

public class SessionMapper : ISessionMapper
{
    public SessionResponseDto ToDto(Session entity) =>
        new()
        {
            Id = entity.Id,
            SessionLabelId = entity.SessionLabelId,
            SessionLabelName = entity.SessionLabel.Name,
            Category = entity.SessionLabel.Category,
            Date = entity.Date,
            Name = entity.Name,
            Topic = entity.Topic,
            Notes = entity.Notes,
            Location = entity.Location,
            StartsAt = entity.StartsAt,
            EndsAt = entity.EndsAt,
            Status = entity.Status,
            CreatedBy = entity.CreatedBy,
            CreatedByName = entity.Creator?.Name,
            CreatedAt = entity.CreatedAt,
            // The attendance counts are populated by the service after a batched
            // GetAttendance call. Left at 0 here.
        };

    public Session ToEntity(SessionCreateDto dto) =>
        new()
        {
            SessionLabelId = dto.SessionLabelId,
            Date = dto.Date,
            Name = dto.Name,
            Topic = dto.Topic,
            Notes = dto.Notes,
            Location = dto.Location,
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt,
        };

    public List<SessionResponseDto> ToListDto(List<Session> entities) =>
        [.. entities.Select(ToDto)];

    public void Patch(SessionUpdateDto dto, Session entity)
    {
        if (dto.Name != null)
            entity.Name = dto.Name;
        if (dto.Topic != null)
            entity.Topic = dto.Topic;
        if (dto.Notes != null)
            entity.Notes = dto.Notes;
        if (dto.Location != null)
            entity.Location = dto.Location;
        if (dto.StartsAt.HasValue)
            entity.StartsAt = dto.StartsAt.Value;
        if (dto.EndsAt.HasValue)
            entity.EndsAt = dto.EndsAt.Value;
        if (dto.Status.HasValue)
            entity.Status = dto.Status.Value;
    }
}
