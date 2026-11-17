namespace MailToLog.Application.DTOs;

public record ExtractionOutcome(
ExtractedEntry? Entry,
IReadOnlyList<string> Errors,
int Attempts,
long LatencyMs,
string Model,
string PromptVersion);
