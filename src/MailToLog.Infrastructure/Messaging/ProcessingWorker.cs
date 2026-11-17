using MailToLog.Application.Handlers;
using MailToLog.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MailToLog.Infrastructure.Messaging;

public sealed class ProcessingWorker(
 IMessageQueue queue, IServiceScopeFactory scopes, ILogger<ProcessingWorker> logger)
 : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var id in queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ProcessMessageHandler>();
                await handler.HandleAsync(id, stoppingToken);
                logger.LogInformation("Processed message {MessageId}", id);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log ids and errors only, never the email body.
                logger.LogError(ex, "Processing failed for message {MessageId}", id);
            }
        }
    }
}
