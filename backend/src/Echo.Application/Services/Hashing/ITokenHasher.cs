namespace Echo.Application.Services.Hashing;

public interface ITokenHasher
{
    Task<string> HashAsync(string token);
    Task<bool> VerifyAsync(string token, string hash);
}
