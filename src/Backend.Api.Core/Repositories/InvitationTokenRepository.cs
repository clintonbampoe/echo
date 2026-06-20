using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
            .Select(i => new InvitationTokenListResponseDto(
                i.Id,
                i.CreatedBy.EmailAddress,
                i.AllowedRole,
                i.Token,
                i.IsRevoked
            ))
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
            .Select(i => new InvitationTokenResponseDto(
                i.Id,
                i.Congregation.Name,
                i.CreatedBy.EmailAddress,
                i.AllowedRole,
                i.Token,
                i.ExpiryDate,
                i.IsRevoked,
                i.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
