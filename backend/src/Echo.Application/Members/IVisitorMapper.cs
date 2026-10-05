using Echo.Domain.Members;

namespace Echo.Application.Members;

public interface IVisitorMapper
{
    VisitorResponseDto ToDto(Visitor entity);
    (Person person, Visitor visitor) ToEntity(VisitorCreateDto dto);
    List<VisitorResponseDto> ToListDto(List<Visitor> entities);
    List<VisitorSearchResultDto> ToSearchDto(List<Visitor> entities);
    void Patch(VisitorUpdateDto dto, Person person, Visitor visitor);
    Member ToMemberEntity(MemberCreateDto dto);
}
