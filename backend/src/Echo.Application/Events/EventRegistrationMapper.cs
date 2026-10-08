using Echo.Domain.Events;
using Riok.Mapperly.Abstractions;

namespace Echo.Application.Events;

[Mapper]
public partial class EventRegistrationMapper : IEventRegistrationMapper
{
    public EventRegistrationResponseDto ToDto(EventRegistration entity) =>
        new EventRegistrationResponseDto
        {
            Id = entity.Id,
            MemberId = entity.MemberId,
            MemberName = entity.Member.Person.Name,
            EventId = entity.EventId,
            EventName = entity.Event.Name,
            RegistrationDate = entity.RegistrationDate,
            CreatedAt = entity.CreatedAt,
        };

    [MapperIgnoreTarget(nameof(EventRegistration.Congregation))]
    [MapperIgnoreTarget(nameof(EventRegistration.CongregationId))]
    [MapperIgnoreTarget(nameof(EventRegistration.Id))]
    [MapperIgnoreTarget(nameof(EventRegistration.Member))]
    [MapperIgnoreTarget(nameof(EventRegistration.Event))]
    [MapperIgnoreTarget(nameof(EventRegistration.CreatedAt))]
    [MapperIgnoreTarget(nameof(EventRegistration.DeletedAt))]
    public partial EventRegistration ToEntity(EventRegistrationCreateDto dto);

    public partial List<EventRegistrationResponseDto> ToListDto(List<EventRegistration> entities);

    public void Patch(EventRegistrationUpdateDto dto, EventRegistration entity)
    {
        if (dto.RegistrationDate.HasValue)
            entity.RegistrationDate = dto.RegistrationDate.Value;
    }
}
