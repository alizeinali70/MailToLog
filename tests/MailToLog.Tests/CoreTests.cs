using MailToLog.Domain.Entities;

namespace MailToLog.Tests;

public class TextNormalizerTests
{
    [Theory]
    [InlineData("Müller, Anna", "anna muller")]
    [InlineData("  Anna   Müller ", "anna muller")]
    public void NormalizeName_handles_order_and_diacritics(string input, string expected) =>
        Assert.Equal(expected, TextNormalizer.NormalizeName(input));

    [Fact]
    public void Mueller_is_one_edit_from_Muller() =>
        Assert.Equal(1, TextNormalizer.Levenshtein("mueller", "muller"));
}
