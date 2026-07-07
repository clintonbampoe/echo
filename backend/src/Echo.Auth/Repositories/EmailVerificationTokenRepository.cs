using Echo.Application.Extensions.QueryMethods;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Repositories;

public class EmailVerificationTokenRepository(AppDbContext context)
{
    private readonly DbSet<EmailVerificationToken>  _tokens =  context.Set<EmailVerificationToken>();

    public async Task<EmailVerificationToken?> GetToken(string token, CancellationToken ct = default)
    {
        var entity = await _tokens
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.TokenHash == token, cancellationToken: ct);

        return entity;
    }

    public async Task<bool> CreateRecord(EmailVerificationToken emailVerificationToken, CancellationToken ct = default)
    {
        await _tokens.AddAsync(emailVerificationToken, ct);
        return true;
    }

    public async Task<bool> UpdateRecord(Guid id, EmailVerificationToken token, CancellationToken ct = default)
    {
        var existing = await _tokens
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        if (existing is null)
            return false;

        _tokens.Entry(existing).CurrentValues.SetValues(token);
        _tokens.Entry(existing).Property(e => e.Id).IsModified = false;
        _tokens.Entry(existing).Property(e => e.CreatedAt).IsModified = false;
        _tokens.Entry(existing).Property(e => e.DeletedAt).IsModified = false;

        return true;
    }
}
