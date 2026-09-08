using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.EventMapping;

public interface IEventMapper
{
    EventResponseDto ToDto(Event entity);
    Event ToEntity(EventCreateDto dto);
    void Patch(EventUpdateDto dto, Event entity);
}
