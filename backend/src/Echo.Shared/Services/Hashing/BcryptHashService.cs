namespace Echo.Shared.Services.Hashing;

using BCrypt.Net;

public class BcryptHashService : IPasswordHasher
{
    private const int _costFactor = 10;

    // Computed on the calling thread: a request already runs on a threadpool thread, so
    // Task.Run would only add a hop. The Task return type is the interface's.
    public Task<string> HashAsync(string input) =>
        Task.FromResult(BCrypt.HashPassword(input, _costFactor));

    // A stored value that is not a bcrypt hash (a row from before passwords were hashed) matches nothing.
    public Task<bool> VerifyAsync(string input, string hash)
    {
        try
        {
            return Task.FromResult(BCrypt.Verify(input, hash));
        }
        catch (Exception ex) when (ex is SaltParseException or ArgumentException)
        {
            return Task.FromResult(false);
        }
    }
}
