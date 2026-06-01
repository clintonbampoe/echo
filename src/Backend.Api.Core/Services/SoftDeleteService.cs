using Backend.Api.Core.Data;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Services;

public class SoftDeleteService<T> : ISoftDeleteService<T> where T : class, ICongregationEntity, ISoftDeletableEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public SoftDeleteService(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<int> SoftDeleteByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        var entity = Activator.CreateInstance<T>();

        _context.Entry(entity).Property(nameof(ICongregationEntity.Id)).CurrentValue = Id;
        _context.Entry(entity).Property(nameof(ISoftDeletableEntity.DeletedAt)).CurrentValue = DateTime.UtcNow;

        _context.Entry(entity).State = EntityState.Modified;

        return await _context.SaveChangesAsync(cancellationToken);
    }



}
