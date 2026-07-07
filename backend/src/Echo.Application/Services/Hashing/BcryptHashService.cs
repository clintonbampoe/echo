namespace Echo.Application.Services.Hashing;

public class BcryptHashService : IHashService
{
    public Task<string> HashPasswordAsync(string input)
    {
        return Task.Run(() => BCrypt.Net.BCrypt.HashPassword(input, 12));
    }

    public Task<bool> VerifyPasswordAsync(string input, string hash)
    {
        return Task.Run(() => BCrypt.Net.BCrypt.Verify(input, hash));
    }
}
