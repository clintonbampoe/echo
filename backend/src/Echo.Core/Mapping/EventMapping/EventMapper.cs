using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.EventMapping;

[Mapper]
public partial class EventMapper : IEventMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(nameof(Event.Organization.Name), nameof(EventResponseDto.OrganizationName))]
    [MapProperty(nameof(Event.Organizer.Name), nameof(EventResponseDto.OrganizerName))]
    public partial EventResponseDto ToDto(Event entity);

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
