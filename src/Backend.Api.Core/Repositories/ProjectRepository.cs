using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class ProjectRepository : EntityRepositoryBase<Project>
{
    public ProjectRepository(DbContext context, IMapper mapper, IDatabaseEngine<Project> domainRecordService)
        : base(context, mapper, domainRecordService)
    {
    }

    public override async Task<PagedResponse<Project>> GetPageAsync(
        PaginationParameters paginationParameters, QueryParameters queryParameters, CancellationToken cancellationToken = default)
    {
        var totalRecordCount = await _dbSet
            .AsNoTracking()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .CountAsync(cancellationToken);

        var records = await _dbSet
            .AsNoTracking()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .ApplyPagination(paginationParameters)
            .ToListAsync(cancellationToken);

        return _domainRecordService.CreateNewPagedResponseObject(records, paginationParameters, totalRecordCount);
    }
}
