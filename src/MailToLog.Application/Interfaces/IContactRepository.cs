using MailToLog.Domain.Entities;

namespace MailToLog.Application.Interfaces;

public interface IContactRepository
{
    Task<Contact?> FindByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<IReadOnlyList<Contact>> ListAsync(CancellationToken ct);
    Task AddAsync(Contact contact, CancellationToken ct);
}
