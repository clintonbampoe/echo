using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Entities.Dtos;
using Backend.Api.Core.Entities.Dtos.Interfaces;
using Backend.Api.Core.Repositories.Interfaces;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class MemberRepository : IEntityRepository<Member>
{
    private readonly DbContext _context;
    private readonly DbSet<Member> _dbSet;
    private readonly ISoftDeleteService<Member> _softDeleteService;
    public MemberRepository(AppDbContext context, ISoftDeleteService<Member> softDeleteService)
    {
        _context = context;
        _dbSet = context.Set<Member>();
        _softDeleteService = softDeleteService;
    }

    public async Task<PagedResponse<IResponseDto<Member>>> GetPageAsync(
        PaginationParams paginationParameters, QueryParameters queryParameters, CancellationToken cancellationToken = default)
    {
        var totalRecords = await _dbSet
            .AsNoTracking()
            .ApplyFilters(queryParameters)
            .CountAsync(cancellationToken);

        var records = await _dbSet
            .AsNoTracking()
            .ApplyFilters(queryParameters)
            .ApplyPagination(paginationParameters)
            .Select(m => new MemberListResponseDto
            {
                Id = m.Id,
                CongregationId = m.CongregationId,
                FullName = $"{m.FirstName} {m.LastName} {m.OtherNames}"
            })
            .ToListAsync(cancellationToken);

        var responseAsDtos = records.Cast<IResponseDto<Member>>().ToList();

        return new PagedResponse<IResponseDto<Member>>(
            responseAsDtos, paginationParameters.PageNumber, paginationParameters.PageSize, totalRecords);
    }

    public async Task<IResponseDto<Member>?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.Id == Id)
            .Select(m => (IResponseDto<Member>)new MemberResponseDto
            {
                Id = m.Id,
                CongregationId = m.CongregationId,
                FirstName = m.FirstName,
                LastName = m.LastName,
                OtherNames = m.OtherNames,
                DateOfBirth = m.DateOfBirth,
                Email = m.EmailAddress,
                PhoneNumber = m.PhoneNumber
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CreateRecord(ICreateDto<Member> recordData, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(new Member
        {
            Id = recordData.Id,
            CongregationId = recordData.CongregationId
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateRecord(
        Guid Id, IUpdateDto<Member> recordData, CancellationToken cancellationToken = default)
    {
        if (recordData is null)
            return false;

        var projectAsConcreteObject = (MemberUpdateDto)recordData;

        await _dbSet.Where(m => m.Id == Id).ExecuteUpdateAsync(
            s => s.SetProperty(m => m.FirstName, projectAsConcreteObject.FirstName)
                .SetProperty(m => m.LastName, projectAsConcreteObject.LastName),
            cancellationToken);

        return true;
    }

    public async Task DeleteRecord(Guid Id, CancellationToken cancellationToken = default)
    {
        await _softDeleteService.SoftDeleteByIdAsync(Id, cancellationToken);
    }
}
