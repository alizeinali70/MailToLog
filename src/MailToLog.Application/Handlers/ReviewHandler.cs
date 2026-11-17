using MailToLog.Application.Contacts;
using MailToLog.Application.DTOs;
using MailToLog.Application.Interfaces;
using MailToLog.Domain.Enums;
using System.Text.Json;

namespace MailToLog.Application.Handlers;

public sealed class ReviewHandler(IActivityLogRepository logs, ContactService contactService, IUnitOfWork uow)
{
    public async Task<bool> ApproveAsync(Guid id, CancellationToken ct)
    {
        var log = await logs.GetAsync(id, ct);
        if (log is null || log.Status != ReviewStatus.PendingReview) return false;

        var entry = JsonSerializer.Deserialize<ExtractedEntry>(log.PayloadJson);
        if (entry?.Contacts is not null)
            log.PrimaryContactId = await contactService.ApplyAsync(entry.Contacts, ct);

        log.Status = ReviewStatus.Approved;
        await uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejectAsync(Guid id, CancellationToken ct)
    {
        var log = await logs.GetAsync(id, ct);
        if (log is null || log.Status != ReviewStatus.PendingReview) return false;

        log.Status = ReviewStatus.Rejected;
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
