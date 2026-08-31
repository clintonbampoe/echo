using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class CongregationRepository(AppDbContext context)
{
    private readonly DbSet<Congregation> _dbSet = context.Set<Congregation>();

    public async Task<CongregationResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CongregationResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                OrgType = c.OrgType,

                EmailAddress = c.EmailAddress,
                PhoneNumber = c.PhoneNumber,
                PostalAddress = c.PostalAddress,
                WebsiteUrl = c.WebsiteUrl,

                Region = c.Region,
                City = c.City,
                Town = c.Town,
                GpsAddress = c.GpsAddress,

                CreatedAt = c.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public virtual async Task<bool> CreateRecord(
        Congregation entity,
        CancellationToken ct = default
    )
    {
        await _dbSet.AddAsync(entity, ct);
        return true;
    }

    public virtual async Task<bool> UpdateRecordAsync(
        Guid id,
        Congregation entity,
        CancellationToken ct = default
    )
    {
        var existing = await _dbSet
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (existing is null)
            return false;

        _dbSet.Entry(existing).CurrentValues.SetValues(entity);
        return true;
    }

    public virtual async Task<bool> DeleteRecordAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _dbSet
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (existing is null)
            return false;

        existing.DeletedAt = DateTime.UtcNow;
        return true;
    }
}
