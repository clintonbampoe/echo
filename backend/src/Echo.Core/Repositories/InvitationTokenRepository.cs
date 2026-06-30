using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class InvitationTokenRepository(AppDbContext context)
    : PrimaryRepositoryBase<InvitationToken>(context)
{
    public async Task<PagedResponse<InvitationTokenListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplyDateFilters(queryParameters)
            .Where(i => i.CongregationId == congregationId);

        int totalRecordCount = await query.CountAsync(ct);

        var records = await query
            .OrderBy(i => i.Id)
            .Select(i => new InvitationTokenListResponseDto
            {
                Id = i.Id,
                CreatedByUserEmail =  i.CreatedBy.EmailAddress,
                AllowedRole = i.AllowedRole,
                Token = i.Token,
                IsRevoked = i.IsRevoked
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<InvitationTokenListResponseDto>(
            records,
            paginationParameters,
            totalRecordCount
        );
    }

    public async Task<InvitationTokenResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(i => i.Id == id)
            .Select(i => new InvitationTokenResponseDto
            {
                Id = i.Id,
                Congregation = i.Congregation.Name,
                CreatedByUserEmail = i.CreatedBy.EmailAddress,
                AllowedRole = i.AllowedRole,
                Token = i.Token,
                IsRevoked = i.IsRevoked,
                CreatedAt =  i.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
