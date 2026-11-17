using MailToLog.Application.Interfaces;
using System.Threading.Channels;

namespace MailToLog.Infrastructure.Messaging;

public sealed class ChannelMessageQueue : IMessageQueue
{
    private readonly Channel<Guid> _channel =
        Channel.CreateBounded<Guid>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait });

    public ValueTask EnqueueAsync(Guid messageId, CancellationToken ct) =>
        _channel.Writer.WriteAsync(messageId, ct);

    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
