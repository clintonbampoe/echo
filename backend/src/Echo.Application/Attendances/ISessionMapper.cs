using Echo.Domain.Attendances;

namespace Echo.Application.Attendances;

public interface ISessionMapper
{
    SessionResponseDto ToDto(Session entity);
    Session ToEntity(SessionCreateDto dto);
    List<SessionResponseDto> ToListDto(List<Session> entities);
    void Patch(SessionUpdateDto dto, Session entity);
}
