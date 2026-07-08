using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
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
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                EmailAddress = u.EmailAddress,
                VerifiedAt = u.EmailVerifiedAt,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
            })
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<bool> ExecuteUpdateAsync(User user, CancellationToken ct = default)
    {
        var affectedRows = await DbSet
            .Where(u => u.Id == user.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.Name, user.Name)
                .SetProperty(u => u.EmailAddress, user.EmailAddress)
                .SetProperty(u => u.PasswordHash, user.PasswordHash)
                .SetProperty(u => u.EmailVerifiedAt, user.EmailVerifiedAt)
                .SetProperty(u => u.Role, user.Role), cancellationToken: ct);

        return (affectedRows > 0);
    }

    public async Task<bool> IsEmailAddressTaken(string emailAddress, CancellationToken ct)
    {
        var exists = await DbSet
            .ApplySoftDeleteFilter()
            .AnyAsync(u => u.EmailAddress == emailAddress, ct);

        return exists;
    }
}
