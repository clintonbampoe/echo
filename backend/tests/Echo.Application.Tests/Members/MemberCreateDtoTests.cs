using System.Text.Json;
using System.Text.Json.Serialization;
using Echo.Application.Members;
using Echo.Domain.Members;

namespace Echo.Application.Tests.Members;

[Trait("Category", "Unit")]
public class MemberCreateDtoTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private const string Everything = """
        { "firstName": "Ama", "lastName": "Mensah", "phoneNumber": "0244000000",
          "dateOfBirth": "1990-04-02", "gender": "Female" }
        """;

    [Fact]
    public void AMember_NeedsOnlyANamePhoneGenderAndDateOfBirth()
    {
        var dto = JsonSerializer.Deserialize<MemberCreateDto>(Everything, Options)!;

        Assert.Equal((Gender.Female, new DateOnly(1990, 4, 2)), (dto.Gender, dto.DateOfBirth));
        Assert.Null(dto.ResidentialAddress);
        Assert.Null(dto.City);
        Assert.Null(dto.Hometown);
        Assert.Null(dto.Region);
        Assert.Null(dto.MaritalStatus);
        Assert.Null(dto.NextOfKin);
        Assert.Null(dto.EmergencyContactName);
        Assert.Null(dto.EmergencyContactPhoneNumber);
        Assert.Equal(MemberStatus.Active, dto.Status);
    }

    [Theory]
    [InlineData("gender")]
    [InlineData("dateOfBirth")]
    public void AMissingGenderOrDateOfBirth_IsRejected_NotSilentlyDefaulted(string field)
    {
        var json = JsonSerializer.Serialize(
            JsonSerializer
                .Deserialize<Dictionary<string, JsonElement>>(Everything)!
                .Where(p => p.Key != field)
                .ToDictionary(p => p.Key, p => p.Value)
        );

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<MemberCreateDto>(json, Options));
    }
}
