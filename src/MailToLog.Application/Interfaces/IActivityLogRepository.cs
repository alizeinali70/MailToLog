using MailToLog.Domain.Entities;

namespace MailToLog.Application.Interfaces;

public interface IActivityLogRepository
{
    Task<ActivityLog?> GetAsync(Guid id, CancellationToken ct);
    Task<ActivityLog?> GetByMessageIdAsync(Guid messageId, CancellationToken ct);
    Task<IReadOnlyList<ActivityLog>> ListPendingAsync(CancellationToken ct);
    Task AddAsync(ActivityLog log, CancellationToken ct);
}
