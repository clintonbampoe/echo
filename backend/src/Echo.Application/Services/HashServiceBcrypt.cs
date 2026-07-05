namespace Echo.Application.Services;

public class HashServiceBcrypt : IHashService
{
    public Task<string> HashPasswordAsync(string password)
    {
        return Task.Run(() => BCrypt.Net.BCrypt.HashPassword(password, 12));
    }

    public Task<bool> VerifyPasswordAsync(string password, string hashedPassword)
    {
        return Task.Run(() => BCrypt.Net.BCrypt.Verify(password, hashedPassword));
    }
}
