namespace Echo.Application.Services.Hashing;

public interface ITokenHasher
{
    string Hash(string token);
    bool Verify(string token, string hash);
}
