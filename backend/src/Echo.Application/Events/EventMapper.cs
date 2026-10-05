using Echo.Domain.Events;
using Riok.Mapperly.Abstractions;

namespace Echo.Application.Events;

[Mapper]
public partial class EventMapper : IEventMapper
{
    public EventResponseDto ToDto(Event entity) =>
        new EventResponseDto
        {
            Id = entity.Id,
            OrganizationId = entity.OrganizationId,
            OrganizationName = entity.Organization.Name,
            OrganizerId = entity.OrganizerId,
            OrganizerName = entity.Organizer.Person.Name,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Location = entity.Location,
            Capacity = entity.Capacity,
            Description = entity.Description,
        };

    [MapperIgnoreTarget(nameof(Event.Congregation))]
    [MapperIgnoreTarget(nameof(Event.CongregationId))]
    [MapperIgnoreTarget(nameof(Event.Id))]
    [MapperIgnoreTarget(nameof(Event.Organization))]
    [MapperIgnoreTarget(nameof(Event.Organizer))]
    [MapperIgnoreTarget(nameof(Event.CreatedAt))]
    [MapperIgnoreTarget(nameof(Event.DeletedAt))]
    public partial Event ToEntity(EventCreateDto dto);

    public partial List<EventResponseDto> ToListDto(List<Event> entities);

    public List<EventSearchResultDto> ToSearchDto(List<Event> entities)
    {
        var res = entities
            .Select(e => new EventSearchResultDto() { Id = e.Id, Name = e.Name })
            .ToList();
        return res;
    }

    public void Patch(EventUpdateDto dto, Event entity)
    {
        if (dto.OrganizationId.HasValue)
            entity.OrganizationId = dto.OrganizationId.Value;
        if (dto.OrganizerId.HasValue)
            entity.OrganizerId = dto.OrganizerId.Value;
        if (dto.Name != null)
            entity.Name = dto.Name;
        if (dto.StartDate.HasValue)
            entity.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue)
            entity.EndDate = dto.EndDate.Value;
        if (dto.StartTime != null)
            entity.StartTime = dto.StartTime;
        if (dto.EndTime != null)
            entity.EndTime = dto.EndTime;
        if (dto.Location != null)
            entity.Location = dto.Location;
        if (dto.Capacity.HasValue)
            entity.Capacity = dto.Capacity.Value;
        if (dto.Description != null)
            entity.Description = dto.Description;
    }
}
