using MailToLog.Application.Interfaces;
using MailToLog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MailToLog.Infrastructure.Persistence;

public sealed class ContactRepository(AppDbContext db) : IContactRepository
{
    public Task<Contact?> FindByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        db.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.Email == normalizedEmail, ct);

    public async Task<IReadOnlyList<Contact>> ListAsync(CancellationToken ct) =>
        await db.Contacts.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Contact contact, CancellationToken ct) =>
        await db.Contacts.AddAsync(contact, ct);
}
