using MailToLog.Domain.Enums;

namespace MailToLog.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ReviewStatus Status { get; set; }
    public string Summary { get; set; } = "";
    public double Confidence { get; set; }
    public Guid? PrimaryContactId { get; set; }
    public string? ReviewReason { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public string Model { get; set; } = "";
    public string PromptVersion { get; set; } = "";
    public int Attempts { get; set; }
    public long LatencyMs { get; set; }
}
