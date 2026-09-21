using Echo.Domain.Members;

namespace Echo.Core.Members;

public interface IMemberMapper
{
    MemberResponseDto ToDto(Member entity);
    Member ToEntity(MemberCreateDto dto);
    List<MemberResponseDto> ToListDto(List<Member> entities);

    List<MemberSearchResultDto> ToSearchDto(List<Member> entities);
    void Patch(MemberUpdateDto dto, Member entity);
}
