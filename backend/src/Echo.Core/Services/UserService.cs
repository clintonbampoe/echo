using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Dtos.Interfaces;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class UserService(UserRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    : PrimaryServiceBase<User, UserResponseDto>(repository, unitOfWork, mapper)
{
    private readonly UserRepository _userRepository = repository;

    public override async Task<IOperationResult> CreateAsync(
        Guid congregationId,
        IPrimaryCreateDto dto,
        CancellationToken ct = default
    )
    {
        var userDto = (UserCreateDto)dto;

        if (await IsEmailTaken(userDto.EmailAddress, ct))
            return new BadRequestResult("Email already exists.");

        var user = Mapper.Map<User>(userDto);
        user.CongregationId = congregationId;

        var createdSuccessfully = await _userRepository.CreateRecord(user, ct);
        await UnitOfWork.CommitAsync(ct);

        if (!createdSuccessfully)
            return new InternalServerError();

        return new OkResult("Operation completed successfully.");
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _userRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Invalid request.");

        return new SuccessResult<UserResponseDto>(result);
    }

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _userRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );

        return new SuccessResult<PagedResponse<UserListResponseDto>>(result);
    }

    public async Task<IOperationResult> SearchUsersByName(
        Guid congregationId,
        string searchString,
        CancellationToken ct
    )
    {
        var results = await _userRepository.SearchUsersByName(congregationId, searchString, ct);
        return new SuccessResult<List<UserListResponseDto>>(results);
    }

    private async Task<bool> IsEmailTaken(string emailAddress, CancellationToken ct)
    {
        return await _userRepository.IsEmailAddressTaken(emailAddress, ct);
    }
}
