using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class UserRepository(AppDbContext context) : PrimaryRepositoryBase<User>(context)
{
    public async Task<PagedResponse<UserListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(u => u.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(u => u.Id)
            .Select(u => new UserListResponseDto
            {
                Id = u.Id,
                EmailAddress = u.EmailAddress,
                VerifiedAt = u.EmailVerifiedAt,
                Role = u.Role,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<UserListResponseDto>(records, paginationParameters, totalRecords);
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                EmailAddress = u.EmailAddress,
                VerifiedAt = u.EmailVerifiedAt,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsEmailAddressTaken(string emailAddress, CancellationToken ct)
    {
        var exists = await DbSet
            .ApplySoftDeleteFilter()
            .AnyAsync(u => u.EmailAddress == emailAddress, ct);

        return exists;
    }

    public async Task<UserAuthDto?> GetActiveUserByEmail(
        string emailAddress,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .ApplySoftDeleteFilter()
            .Where(u => u.EmailAddress == emailAddress)
            .Select(u => new UserAuthDto()
            {
                Id = u.Id,
                CongregationId = u.CongregationId,
                EmailAddress = u.EmailAddress,
                Name = u.Name,
                EmailVerifiedAt = u.EmailVerifiedAt,
                PasswordHash = u.PasswordHash,
                Role = u.Role,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<UserAuthDto?> GetActiveUserById(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .ApplySoftDeleteFilter()
            .Where(u => u.Id == id)
            .Select(u => new UserAuthDto()
            {
                Id = u.Id,
                CongregationId = u.CongregationId,
                EmailAddress = u.EmailAddress,
                Name = u.Name,
                EmailVerifiedAt = u.EmailVerifiedAt,
                PasswordHash = u.PasswordHash,
                Role = u.Role,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<UserListResponseDto>> SearchUsersByName(
        Guid congregationId,
        string searchString,
        CancellationToken ct
    )
    {
        var results = await DbSet
            .ApplySoftDeleteFilter()
            .Where(u =>
                u.CongregationId == congregationId
                && EF.Functions.ILike(u.Name, $"%{searchString}%")
            )
            .OrderByDescending(u => EF.Functions.TrigramsSimilarity(u.Name, searchString))
            .Take(5)
            .Select(u => new UserListResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                EmailAddress = u.EmailAddress,
                Role = u.Role,
                VerifiedAt = u.EmailVerifiedAt,
            })
            .ToListAsync(ct);

        return results;
    }
}
