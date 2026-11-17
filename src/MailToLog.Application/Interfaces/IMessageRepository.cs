using MailToLog.Domain.Entities;

namespace MailToLog.Application.Interfaces;

public interface IMessageRepository
{
    Task<IncomingMessage?> FindByHashAsync(string hash, CancellationToken ct);
    Task<IncomingMessage?> GetAsync(Guid id, CancellationToken ct);
    Task AddAsync(IncomingMessage message, CancellationToken ct);
}
