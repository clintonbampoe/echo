using Echo.Domain.Events;

namespace Echo.Application.Events;

public interface IEventRegistrationMapper
{
    EventRegistrationResponseDto ToDto(EventRegistration entity);
    EventRegistration ToEntity(EventRegistrationCreateDto dto);
    List<EventRegistrationResponseDto> ToListDto(List<EventRegistration> entities);
    void Patch(EventRegistrationUpdateDto dto, EventRegistration entity);
}
