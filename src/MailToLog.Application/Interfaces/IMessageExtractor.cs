using MailToLog.Application.DTOs;

namespace MailToLog.Application.Interfaces;

public interface IMessageExtractor
{
    Task<ExtractionOutcome> ExtractAsync(string text, DateOnly today, CancellationToken ct);
}
