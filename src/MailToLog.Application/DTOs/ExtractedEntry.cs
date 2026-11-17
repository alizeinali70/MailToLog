using System.ComponentModel;

namespace MailToLog.Application.DTOs;

public record ExtractedEntry(
 [property: Description("Summary in German, max 3 sentences")]
string Summary,
 List<ContactDto> Contacts,
 List<TaskDto> Tasks,
 [property: Description("Self-assessed confidence between 0.0 and 1.0")]
double Confidence);
