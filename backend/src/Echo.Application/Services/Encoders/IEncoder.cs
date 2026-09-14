namespace Echo.Application.Services.Encoders;

public interface IEncoder
{
    public string Encode(object data);
    public T? Decode<T>(string? encodedText);
}
