using Echo.Data;
using Echo.Domain.Members;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Members;

public class PersonRepository(AppDbContext context)
{
    private readonly DbSet<Person> _dbSet = context.Set<Person>();

    public async Task<Person?> GetById(Guid congregationId, Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(p => p.Id == id && p.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(Person entity) => _dbSet.Add(entity);

    public void SoftDelete(Person entity) => entity.DeletedAt = DateTime.UtcNow;
}
