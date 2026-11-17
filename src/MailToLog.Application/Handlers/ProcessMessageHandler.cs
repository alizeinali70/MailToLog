using MailToLog.Application.Contacts;
using MailToLog.Application.Interfaces;
using MailToLog.Application.Validators;
using MailToLog.Domain.Entities;
using MailToLog.Domain.Enums;
using System.Text.Json;

namespace MailToLog.Application.Handlers;

public sealed class ProcessMessageHandler(
IMessageRepository messages,
IMessageExtractor extractor,
ContactMatcher matcher,
ContactService contactService,
IActivityLogRepository logs,
ReviewPolicy policy,
IClock clock,
IUnitOfWork uow)
{
    public async Task HandleAsync(Guid messageId, CancellationToken ct)
    {
        var message = await messages.GetAsync(messageId, ct);
        if (message is null || message.Status != MessageStatus.Queued) return;

        var today = clock.Today;
        var outcome = await extractor.ExtractAsync(message.Text, today, ct);

        var log = new ActivityLog
        {
            MessageId = message.Id,
            Model = outcome.Model,
            PromptVersion = outcome.PromptVersion,
            Attempts = outcome.Attempts,
            LatencyMs = outcome.LatencyMs
        };
        var reasons = new List<string>();

        if (outcome.Entry is null)
        {
            reasons.AddRange(outcome.Errors);
            log.Summary = "(extraction failed)";
            message.Status = MessageStatus.Failed;
        }
        else
        {
            var entry = outcome.Entry;
            log.Summary = entry.Summary;
            log.Confidence = entry.Confidence;
            log.PayloadJson = JsonSerializer.Serialize(entry);

            var duplicateWarnings = new List<string>();
            foreach (var dto in entry.Contacts)
            {
                var match = await matcher.FindAsync(dto, ct);
                if (match.Kind == MatchKind.Possible)
                    duplicateWarnings.Add($"Possible duplicate: '{dto.Name}' ~ '{match.Existing!.Name}'.");
            }

            reasons.AddRange(policy.Evaluate(
                entry.Confidence, EntryValidator.Validate(entry, today), duplicateWarnings));

            // Only touch contact data when nothing needs review.
            if (reasons.Count == 0)
                log.PrimaryContactId = await contactService.ApplyAsync(entry.Contacts, ct);

            message.Status = MessageStatus.Processed;
        }

        log.Status = reasons.Count == 0 ? ReviewStatus.Approved : ReviewStatus.PendingReview;
        log.ReviewReason = reasons.Count == 0 ? null : string.Join(" | ", reasons);

        await logs.AddAsync(log, ct);
        await uow.SaveChangesAsync(ct);
    }
}
