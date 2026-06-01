using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Interfaces;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class MemberRepository : IEntityRepository<Member>
{
    private readonly DbContext _context;
    private readonly DbSet<Member> _dbSet;
    private readonly IMapper _mapper;
    private readonly ISoftDeleteService<Member> _softDeleteService;
    private readonly IUpdateRecordService<Member> _updateRecordService;
    private readonly IGetRecordService<Member> _getRecordService;
    private readonly ICreateNewRecordService<Member> _createNewRecordService;
    public MemberRepository(
        AppDbContext context,
        IMapper mapper,
        ISoftDeleteService<Member> softDeleteService,
        IUpdateRecordService<Member> updateRecordService,
        IGetRecordService<Member> getRecordService,
        ICreateNewRecordService<Member> createNewRecordService)
    {
        _context = context;
        _dbSet = context.Set<Member>();
        _softDeleteService = softDeleteService;
        _mapper = mapper;
        _updateRecordService = updateRecordService;
        _getRecordService = getRecordService;
        _createNewRecordService = createNewRecordService;
    }

    public async Task<PagedResponse<Member>> GetPageAsync(
        PaginationParams paginationParameters, QueryParameters queryParameters, CancellationToken cancellationToken = default)
    {
        var totalRecords = _getRecordService.GetTotalRecordsCount(queryParameters);

        var records = await _dbSet
            .AsNoTracking()
            .ApplyFilters(queryParameters)
            .ApplyPagination(paginationParameters)
            .ToListAsync(cancellationToken);

        return _getRecordService.CreateNewPagedResponseObject(records, paginationParameters, totalRecords);
    }

    public async Task<Member?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return await _getRecordService.GetRecordByIdAsync(Id, cancellationToken);
    }

    public async Task<bool> CreateRecord(Member newRecordData, CancellationToken cancellationToken = default)
    {
        var recordCreatedSuccessfully =
            await _createNewRecordService.CreateNewRecord(newRecordData, cancellationToken);

        if (!recordCreatedSuccessfully)
            return false;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateRecord(
        Guid Id, Member updatedRecordData, CancellationToken cancellationToken = default)
    {
        var recordUpdatedSuccessfully =
            await _updateRecordService.UpdateRecord(Id, updatedRecordData, cancellationToken);

        if (!recordUpdatedSuccessfully)
            return false;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteRecord(Guid Id, CancellationToken cancellationToken = default)
    {
        var recordDeletedSuccessfully = await _softDeleteService.SoftDeleteByIdAsync(Id, cancellationToken);

        if (!recordDeletedSuccessfully)
            return false;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
