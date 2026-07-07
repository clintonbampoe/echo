namespace Echo.Application.Services.Hashing;

public interface IHashService
{
    Task<string>  HashPasswordAsync(string input);
    Task<bool> VerifyPasswordAsync(string input, string hash);
}
