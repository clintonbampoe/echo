using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

public class UserRepository(AppDbContext context) : PrimaryRepositoryBase<User>(context)
{
    public async Task<PagedResponse<UserListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(u => u.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(u => u.Id)
            .Select(u => new UserListResponseDto(u.Id, u.UserName, u.EmailAddress, u.Role))
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<UserListResponseDto>(records, paginationParameters, totalRecords);
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto(
                u.Id,
                u.Name,
                u.UserName,
                u.EmailAddress,
                u.PasswordHash,
                u.Role,
                u.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
