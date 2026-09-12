using System.Text.Json;

namespace Echo.Application.Services.Encoders;

public class CursorEncoder : IEncoder
{
    public string Encode(object cursorData)
    {
        var res = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(cursorData));
        return res;
    }

    public T? Decode<T>(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return default;

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var res = JsonSerializer.Deserialize<T>(bytes);
            return res;
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            // Invalid or tampered cursor
            // Treat as empty cursor
            Console.WriteLine(ex);
            return default;
        }
    }
}
