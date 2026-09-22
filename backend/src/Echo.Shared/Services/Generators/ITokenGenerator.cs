namespace Echo.Shared.Services.Generators;

public interface ITokenGenerator
{
    string GenerateToken(int size = 8);
}
