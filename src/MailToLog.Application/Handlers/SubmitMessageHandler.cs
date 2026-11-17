using MailToLog.Application.Interfaces;
using MailToLog.Domain.Entities;
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
        if (existing is not null) return (existing.Id, true);

        var message = new IncomingMessage { ContentHash = hash, Text = clean };
        await messages.AddAsync(message, ct);
        await uow.SaveChangesAsync(ct);
        await queue.EnqueueAsync(message.Id, ct);
        return (message.Id, false);
    }
}
