using Echo.Domain.Entities.Core;

namespace Echo.Application.Services;

public interface ITokenGenerator
{
    string GenerateToken(int size = 8);
}
