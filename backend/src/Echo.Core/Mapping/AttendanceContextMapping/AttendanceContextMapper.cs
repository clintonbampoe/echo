using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.AttendanceContextMapping;

[Mapper]
public partial class AttendanceContextMapper : IAttendanceContextMapper
{
    [MapProperty(nameof(AttendanceContext.AttendanceType.Name),
        nameof(AttendanceContextResponseDto.AttendanceTypeName))]
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.AttendanceTypeId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapperIgnoreSource(nameof(entity.CreatedAt))]
    public partial AttendanceContextResponseDto ToDto(AttendanceContext entity);

    [MapperIgnoreTarget(nameof(AttendanceContext.Congregation))]
    [MapperIgnoreTarget(nameof(AttendanceContext.CongregationId))]
    [MapperIgnoreTarget(nameof(AttendanceContext.Id))]
    [MapperIgnoreTarget(nameof(AttendanceType))]
    [MapperIgnoreTarget(nameof(AttendanceContext.CreatedAt))]
    [MapperIgnoreTarget(nameof(AttendanceContext.DeletedAt))]
    public partial AttendanceContext ToEntity(AttendanceContextCreateDto dto);

    public void Patch(AttendanceContextUpdateDto dto, AttendanceContext entity)
    {
        if (dto.Name != null) entity.Name = dto.Name;
    }
}
