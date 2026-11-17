using System.ComponentModel;

namespace MailToLog.Application.DTOs;

public record TaskDto(
string Description,
string? Assignee,
[property: Description("Due date as ISO 8601 (yyyy-MM-dd) or null if not stated")]
string? DueDate);
