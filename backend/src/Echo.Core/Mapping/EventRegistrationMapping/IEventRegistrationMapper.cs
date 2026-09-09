using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.EventRegistrationMapping;

public interface IEventRegistrationMapper
{
    EventRegistrationResponseDto ToDto(EventRegistration entity);
    EventRegistration ToEntity(EventRegistrationCreateDto dto);
    void Patch(EventRegistrationUpdateDto dto, EventRegistration entity);
}
