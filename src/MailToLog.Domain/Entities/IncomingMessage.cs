using MailToLog.Domain.Enums;

namespace MailToLog.Domain.Entities
{
    public class IncomingMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ContentHash { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
        public MessageStatus Status { get; set; } = MessageStatus.Queued;
    }
}
