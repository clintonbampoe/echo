using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Services;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Services;

public class RegistrationService(
    AppDbContext context,
    CongregationRepository congregationRepository,
    UserRepository userRepository,
    IMapper mapper,
    IHashService hashService
)
{
    private readonly AppDbContext _context = context;
    private readonly CongregationRepository _congregationRepository = congregationRepository;
    private readonly UserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IHashService _hashService = hashService;

    public async Task<IOperationResult> RegisterCongregation(
        CongregationCreateDto congregationDto,
        UserCreateDto userDto,
        CancellationToken ct
    )
    {
        var congregation = _mapper.Map<Congregation>(congregationDto);
        var user = _mapper.Map<User>(userDto);
        user.Role = UserRole.Admin;
        user.CongregationId = congregation.Id;

        if (await IsEmailTaken(user.EmailAddress, ct))
            return new OkResult("Email already in use");

        await HashPassword(user, userDto);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _congregationRepository.CreateRecord(congregation, ct);
            await _userRepository.CreateRecord(user, ct);
            await _context.SaveChangesAsync(ct);
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
        return await _userRepository.IsEmailAddressTaken(emailAddress, ct);
    }

    private async Task HashPassword(User user, UserCreateDto userDto)
    {
        user.PasswordHash = await _hashService.HashPasswordAsync(userDto.Password);
    }
}
