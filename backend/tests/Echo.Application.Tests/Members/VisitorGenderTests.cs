using System.ComponentModel.DataAnnotations;
using Echo.Application.Members;
using Echo.Domain.Members;

namespace Echo.Application.Tests.Members;

[Trait("Category", "Unit")]
public class VisitorGenderTests
{
    private static bool IsValid(object dto) =>
        Validator.TryValidateObject(dto, new ValidationContext(dto), [], validateAllProperties: true);

    private static VisitorCreateDto Create(Gender gender) =>
        new()
        {
            FirstName = "Ama",
            LastName = "Mensah",
            Gender = gender,
        };

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.NonBinary)]
    public void ANewVisitor_CanHaveAnyGender(Gender gender)
    {
        Assert.True(IsValid(Create(gender)));
    }

    [Fact]
    public void AnUpdate_MayLeaveGenderOut_OrSetAny()
    {
        Assert.True(IsValid(new VisitorUpdateDto { Notes = "x" }));
        Assert.True(IsValid(new VisitorUpdateDto { Gender = Gender.NonBinary }));
    }
}
