using MailToLog.Application.Interfaces;
using MailToLog.Domain.Entities;
using MailToLog.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace MailToLog.Application.Handlers;

public sealed class SubmitMessageHandler(IMessageRepository messages, IMessageQueue queue, IUnitOfWork uow)
{
    public async Task<(Guid Id, bool Duplicate)> HandleAsync(string text, CancellationToken ct)
    {
        var clean = text.Trim();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(clean)));

        var existing = await messages.FindByHashAsync(hash, ct);   // idempotency
        if (existing is not null)
        {
            if (existing.Status == MessageStatus.Processed) return (existing.Id, true);

            // Still Queued or Failed: retry instead of silently ignoring
            existing.Status = MessageStatus.Queued;
            await uow.SaveChangesAsync(ct);
            await queue.EnqueueAsync(existing.Id, ct);
            return (existing.Id, true);
        }

        var message = new IncomingMessage { ContentHash = hash, Text = clean };
        await messages.AddAsync(message, ct);
        await uow.SaveChangesAsync(ct);
        await queue.EnqueueAsync(message.Id, ct);
        return (message.Id, false);
    }
}
