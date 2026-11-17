using MailToLog.Domain.Entities;

namespace MailToLog.Tests;

public class ReviewPolicyTests
{
    private readonly ReviewPolicy _policy = new(0.7);

    [Fact]
    public void Clean_entry_needs_no_review() =>
        Assert.Empty(_policy.Evaluate(0.9, [], []));

    [Fact]
    public void Low_confidence_needs_review() =>
        Assert.Single(_policy.Evaluate(0.5, [], []));

    [Fact]
    public void Possible_duplicate_needs_review() =>
        Assert.Single(_policy.Evaluate(0.9, [], ["Possible duplicate"]));
}
