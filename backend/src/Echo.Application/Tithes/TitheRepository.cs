using Echo.Data;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Tithes;

public class TitheRepository(AppDbContext context)
{
    private readonly DbSet<Tithe> _dbSet = context.Set<Tithe>();

    public async Task<List<Tithe>> List(
        Guid congregationId,
        TitheFilters filters,
        TitheCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Include(t => t.Member)
                .ThenInclude(m => m.Person)
            .Filter(filters)
            .OrderByDescending(t => t.CollectionDate)
            .ThenBy(t => t.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Tithe?> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .Include(t => t.Member)
                .ThenInclude(m => m.Person)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Tithe>> GetByMemberId(
        Guid congregationId,
        Guid memberId,
        TitheFilters filters,
        TitheCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(t => t.MemberId == memberId && t.CongregationId == congregationId)
            .Include(t => t.Member)
                .ThenInclude(t => t.Person)
            .Filter(filters)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public Task<decimal> SumCollected(
        Guid congregationId,
        TitheFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Filter(filters)
            .SumAsync(t => t.Amount, ct);

    public Task<int> CountUniqueTithers(
        Guid congregationId,
        TitheFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Filter(filters)
            .Select(t => t.MemberId)
            .Distinct()
            .CountAsync(ct);

    public Task<PaymentMethod?> MostUsedPaymentMethod(
        Guid congregationId,
        TitheFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Filter(filters)
            .GroupBy(t => t.PaymentMethod)
            .OrderByDescending(g => g.Count())
            .Select(g => (PaymentMethod?)g.Key)
            .FirstOrDefaultAsync(ct);

    public Task<decimal> AveragePerMember(
        Guid congregationId,
        TitheFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Filter(filters)
            .AverageAsync(t => (decimal?)t.Amount, ct)
            .ContinueWith(t => t.Result ?? 0m, ct);

    public void Create(Tithe entity) => _dbSet.Add(entity);

    public void SoftDelete(Tithe entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class TitheQueryExtensions
{
    internal static IQueryable<Tithe> Filter(
        this IQueryable<Tithe> query,
        TitheFilters filters
    )
    {
        if (filters.MemberId is not null)
            query = query.Where(t => t.MemberId == filters.MemberId);

        if (filters.PaymentMethod is not null)
            query = query.Where(t => t.PaymentMethod == filters.PaymentMethod);

        if (filters.Year is not null)
            query = query.Where(t => t.ForYear == filters.Year);

        if (filters.Month is not null)
            query = query.Where(t => t.ForMonth == filters.Month);

        if (filters.From is not null)
            query = query.Where(t => t.CollectionDate >= filters.From);

        if (filters.To is not null)
            query = query.Where(t => t.CollectionDate <= filters.To);

        return query;
    }

    internal static IQueryable<Tithe> Paginate(
        this IQueryable<Tithe> query,
        TitheCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(t =>
                t.CollectionDate < cursor.CollectionDate
                || (t.CollectionDate == cursor.CollectionDate && t.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
