using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.AttendanceTypeMapping;

[Mapper]
public partial class AttendanceTypeMapper : IAttendanceTypeMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.CreatedAt))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    public partial AttendanceTypeResponseDto ToDto(AttendanceType entity);

    [MapperIgnoreTarget(nameof(AttendanceType.Congregation))]
    [MapperIgnoreTarget(nameof(AttendanceType.CongregationId))]
    [MapperIgnoreTarget(nameof(AttendanceType.Id))]
    [MapperIgnoreTarget(nameof(AttendanceType.CreatedAt))]
    [MapperIgnoreTarget(nameof(AttendanceType.DeletedAt))]
    public partial AttendanceType ToEntity(AttendanceTypeCreateDto dto);

    public void Patch(AttendanceTypeUpdateDto dto, AttendanceType entity)
    {
        if (dto.Name != null) entity.Name = dto.Name;
    }
}
