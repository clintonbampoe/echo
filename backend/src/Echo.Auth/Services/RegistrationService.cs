using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Services.Hashing;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Services;

public class RegistrationService(
    AppDbContext context,
    CongregationRepository congregationRepository,
    UserRepository userRepository,
    IMapper mapper,
    [FromKeyedServices("Bcrypt")] IHashService hashService
)
{
    public async Task<IOperationResult> RegisterCongregation(
        CongregationCreateDto congregationDto,
        UserCreateDto userDto,
        CancellationToken ct
    )
    {
        var congregation = mapper.Map<Congregation>(congregationDto);
        var user = mapper.Map<User>(userDto);
        user.Role = UserRole.Admin;
        user.CongregationId = congregation.Id;

        if (await IsEmailTaken(user.EmailAddress, ct))
            return new OkResult("Email already in use");

        await HashPassword(user, userDto);

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            await congregationRepository.CreateRecord(congregation, ct);
            await userRepository.CreateRecord(user, ct);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(ct);
            return new InternalServerError();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return new InternalServerError();
        }

        return new OkResult("Operation completed successfully.");
    }

    private async Task<bool> IsEmailTaken(string emailAddress, CancellationToken ct)
    {
        return await userRepository.IsEmailAddressTaken(emailAddress, ct);
    }

    private async Task HashPassword(User user, UserCreateDto userDto)
    {
        user.PasswordHash = await hashService.HashPasswordAsync(userDto.Password);
    }
}
