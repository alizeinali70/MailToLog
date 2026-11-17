namespace MailToLog.Application.Interfaces;

public interface IClock
{
    DateOnly Today { get; }
}
