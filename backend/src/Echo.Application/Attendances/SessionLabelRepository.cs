using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Attendances;

public class SessionLabelRepository(AppDbContext context)
{
    private readonly DbSet<SessionLabel> _dbSet = context.Set<SessionLabel>();

    public async Task<List<SessionLabel>> List(Guid congregationId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(l => l.CongregationId == congregationId)
            .OrderBy(l => l.Name)
            .ToListAsync(ct);
    }

    public async Task<SessionLabel?> GetById(
        Guid congregationId,
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(l => l.Id == id && l.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<SessionLabel>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(l => l.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(SessionLabel entity) => _dbSet.Add(entity);

    public void BatchCreate(IList<SessionLabel> entities) => _dbSet.AddRange(entities);

    public void SoftDelete(SessionLabel entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class SessionLabelQueryExtensions
{
    internal static IQueryable<SessionLabel> SearchName(
        this IQueryable<SessionLabel> query,
        string name
    )
    {
        return query
            .Where(l => EF.Functions.ILike(l.Name, $"%{name}%"))
            .OrderByDescending(l => EF.Functions.TrigramsSimilarity(l.Name, name))
            .Take(5);
    }
}
