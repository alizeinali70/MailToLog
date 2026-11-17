namespace MailToLog.Domain.Entities;

public sealed class ReviewPolicy(double minConfidence)
{
    public IReadOnlyList<string> Evaluate(
        double confidence,
        IReadOnlyList<string> validationErrors,
        IReadOnlyList<string> duplicateWarnings)
    {
        var reasons = new List<string>(validationErrors);
        if (confidence < minConfidence)
            reasons.Add($"Low confidence ({confidence:0.00}).");
        reasons.AddRange(duplicateWarnings);
        return reasons;
    }
}
