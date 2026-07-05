namespace Echo.Application.Services;

public interface IHashService
{
    Task<string>  HashPasswordAsync(string password);
    Task<bool> VerifyPasswordAsync(string password, string hashedPassword);
}
