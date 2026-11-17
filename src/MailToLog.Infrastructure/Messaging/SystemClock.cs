using MailToLog.Application.Interfaces;

namespace MailToLog.Infrastructure.Messaging;

public sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
