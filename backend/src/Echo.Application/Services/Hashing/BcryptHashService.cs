namespace Echo.Application.Services.Hashing;

using BCrypt.Net;

public class BcryptHashService : IPasswordHasher
{
    public Task<string> HashAsync(string input)
    {
        return Task.Run(() => BCrypt.HashPassword(input, 12));
    }

    public Task<bool> VerifyAsync(string input, string hash)
    {
        return Task.Run(() => BCrypt.Verify(input, hash));
    }
}
