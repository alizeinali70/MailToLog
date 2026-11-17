namespace MailToLog.Application.Interfaces;

public interface IMessageQueue
{
    ValueTask EnqueueAsync(Guid messageId, CancellationToken ct);
    IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct);
}
