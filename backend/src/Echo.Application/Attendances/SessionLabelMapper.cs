using Echo.Domain.Attendances;

namespace Echo.Application.Attendances;

public class SessionLabelMapper : ISessionLabelMapper
{
    public SessionLabelResponseDto ToDto(SessionLabel entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Category = entity.Category,
        };

    public SessionLabel ToEntity(SessionLabelCreateDto dto) =>
        new() { Name = dto.Name, Category = dto.Category };

    public List<SessionLabelResponseDto> ToListDto(List<SessionLabel> entities) =>
        [.. entities.Select(ToDto)];

    public List<SessionLabelSearchResultDto> ToSearchDto(List<SessionLabel> entities) =>
        [
            .. entities.Select(e => new SessionLabelSearchResultDto
            {
                Id = e.Id,
                Name = e.Name,
                Category = e.Category,
            }),
        ];

    public void Patch(SessionLabelUpdateDto dto, SessionLabel entity)
    {
        if (dto.Name != null)
            entity.Name = dto.Name;
    }
}
