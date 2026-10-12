using Echo.Domain.Attendances;

namespace Echo.Application.Attendances;

public interface ISessionLabelMapper
{
    SessionLabelResponseDto ToDto(SessionLabel entity);
    SessionLabel ToEntity(SessionLabelCreateDto dto);
    List<SessionLabelResponseDto> ToListDto(List<SessionLabel> entities);
    List<SessionLabelSearchResultDto> ToSearchDto(List<SessionLabel> entities);
    void Patch(SessionLabelUpdateDto dto, SessionLabel entity);
}
