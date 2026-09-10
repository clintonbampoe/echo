using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.MemberMapping;

public interface IMemberMapper
{
    MemberResponseDto ToDto(Member entity);
    Member ToEntity(MemberCreateDto dto);
    List<MemberResponseDto> ToListDto(List<Member> entities);

    List<MemberSearchResultDto> ToSearchDto(List<Member> entities);
    void Patch(MemberUpdateDto dto, Member entity);
}
