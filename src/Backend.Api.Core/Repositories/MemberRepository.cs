using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Entities.Dtos;
using Backend.Api.Core.Entities.Dtos.Interfaces;
using Backend.Api.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class MemberRepository : IEntityRepository<Member>
{
    private readonly AppDbContext _context;
    private readonly DbSet<Member> _dbSet;
    public MemberRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Member>();
    }

    public async Task<PagedResponse<IResponseDto<Member>>> GetPageAsync(
        PaginationParams paginationParameters,
        QueryParameters queryParameters,
        CancellationToken cancellationToken = default)
    {
        var totalRecords = await _dbSet
            .AsNoTracking()
            .ApplyFilters(queryParameters)
            .CountAsync(cancellationToken);

        var response = await _dbSet
            .AsNoTracking()
            .ApplyFilters(queryParameters)
            .ApplyPagination(paginationParameters)
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
            .ToListAsync(cancellationToken);

        return new PagedResponse<IResponseDto<Member>>(
            response, paginationParameters.PageNumber, paginationParameters.PageSize, totalRecords);
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

    public Task<bool> CreateRecord(ICreateDto<Member> recordData, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateRecord(Guid Id, IUpdateDto<Member> recordData, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRecord(Guid Id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
