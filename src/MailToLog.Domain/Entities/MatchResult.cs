using MailToLog.Domain.Enums;

namespace MailToLog.Domain.Entities;

public record MatchResult(MatchKind Kind, Contact? Existing);
