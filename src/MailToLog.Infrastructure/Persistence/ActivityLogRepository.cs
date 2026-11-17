using MailToLog.Application.Interfaces;
using MailToLog.Domain.Entities;
using MailToLog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MailToLog.Infrastructure.Persistence;

public sealed class ActivityLogRepository(AppDbContext db) : IActivityLogRepository
{
    public Task<ActivityLog?> GetAsync(Guid id, CancellationToken ct) =>
        db.ActivityLogs.FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<ActivityLog?> GetByMessageIdAsync(Guid messageId, CancellationToken ct) =>
        db.ActivityLogs.AsNoTracking().FirstOrDefaultAsync(l => l.MessageId == messageId, ct);

    public async Task<IReadOnlyList<ActivityLog>> ListPendingAsync(CancellationToken ct) =>
        await db.ActivityLogs.AsNoTracking()
            .Where(l => l.Status == ReviewStatus.PendingReview)
            .OrderBy(l => l.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task AddAsync(ActivityLog log, CancellationToken ct) =>
        await db.ActivityLogs.AddAsync(log, ct);
}
