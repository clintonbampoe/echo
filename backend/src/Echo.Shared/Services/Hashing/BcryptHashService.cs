namespace Echo.Shared.Services.Hashing;

using BCrypt.Net;

public class BcryptHashService : IPasswordHasher
{
    private const int _costFactor = 10;

    // Computed on the calling thread: a request already runs on a threadpool thread, so
    // Task.Run would only add a hop. The Task return type is the interface's.
    public Task<string> HashAsync(string input) =>
        Task.FromResult(BCrypt.HashPassword(input, _costFactor));

    public Task<bool> VerifyAsync(string input, string hash) =>
        Task.FromResult(BCrypt.Verify(input, hash));
}
