using Echo.Domain.Members;

namespace Echo.Application.Members;

public interface IMemberMapper
{
    MemberResponseDto ToDto(Member entity);
    (Person person, Member member) ToEntity(MemberCreateDto dto);
    List<MemberResponseDto> ToListDto(List<Member> entities);
    List<MemberSearchResultDto> ToSearchDto(List<Member> entities);
    void Patch(MemberUpdateDto dto, Person person, Member member);
}
