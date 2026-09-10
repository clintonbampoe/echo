using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.EventRegistrationMapping;

[Mapper]
public partial class EventRegistrationMapper : IEventRegistrationMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(
        nameof(EventRegistration.Member.Name),
        nameof(EventRegistrationResponseDto.MemberName)
    )]
    [MapProperty(
        nameof(EventRegistration.Event.Name),
        nameof(EventRegistrationResponseDto.EventName)
    )]
    public partial EventRegistrationResponseDto ToDto(EventRegistration entity);

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
