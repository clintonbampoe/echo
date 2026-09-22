namespace Echo.Shared.Services.Hashing;

public interface IPasswordHasher
{
    Task<string> HashAsync(string password);
    Task<bool> VerifyAsync(string password, string hash);
}
