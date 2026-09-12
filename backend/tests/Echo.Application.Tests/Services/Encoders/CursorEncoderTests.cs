using Echo.Application.Services.Encoders;

namespace Echo.Application.Tests.Services.Encoders;

[Trait("Category", "Unit")]
public class CursorEncoderTests
{
    private readonly CursorEncoder _encoder = new();

    public record TestCursor(string Name, Guid Id);

    [Fact]
    public void Encode_ShouldReturnNonEmptyString()
    {
        var cursor = new TestCursor("Test", Guid.NewGuid());

        var result = _encoder.Encode(cursor);

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void Decode_ShouldReturnOriginalObject()
    {
        var original = new TestCursor("Test Name", Guid.NewGuid());
        var encoded = _encoder.Encode(original);

        var decoded = _encoder.Decode<TestCursor>(encoded);

        Assert.NotNull(decoded);
        Assert.Equal(original.Name, decoded.Name);
        Assert.Equal(original.Id, decoded.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Decode_ShouldReturnDefault_WhenCursorIsNullOrEmpty(string? cursor)
    {
        var result = _encoder.Decode<TestCursor>(cursor);

        Assert.Null(result);
    }

    [Fact]
    public void Decode_ShouldReturnDefault_WhenCursorIsInvalidBase64()
    {
        var invalidBase64 = "This is not base64!";

        var result = _encoder.Decode<TestCursor>(invalidBase64);

        Assert.Null(result);
    }

    [Fact]
    public void Decode_ShouldReturnDefault_WhenCursorIsInvalidJson()
    {
        var invalidJsonBase64 = Convert.ToBase64String("NotJSON"u8.ToArray());

        var result = _encoder.Decode<TestCursor>(invalidJsonBase64);

        Assert.Null(result);
    }

    [Fact]
    public void EncodeDecode_ShouldWorkWithComplexTypes()
    {
        var complexCursor = new
        {
            Date = new DateTime(2024, 1, 1),
            Id = Guid.NewGuid(),
            Tags = new[] { "tag1", "tag2" },
        };

        _encoder.Encode(complexCursor);
        // Since we used an anonymous type, we decode to a dynamic or a specific record
        // In real use cases, it's always a typed Cursor record.

        // To verify, let's use a record for the complex case
        var recordCursor = new ComplexCursor(
            new DateTime(2024, 1, 1),
            Guid.NewGuid(),
            ["tag1", "tag2"]
        );
        var encodedRecord = _encoder.Encode(recordCursor);
        var decodedRecord = _encoder.Decode<ComplexCursor>(encodedRecord);

        Assert.NotNull(decodedRecord);
        Assert.Equal(recordCursor.Date, decodedRecord.Date);
        Assert.Equal(recordCursor.Id, decodedRecord.Id);
        Assert.Equal(recordCursor.Tags, decodedRecord.Tags);
    }

    public record ComplexCursor(DateTime Date, Guid Id, string[] Tags);
}
