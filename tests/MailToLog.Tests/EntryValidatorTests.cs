using MailToLog.Application.DTOs;
using MailToLog.Application.Validators;

namespace MailToLog.Tests;

public class EntryValidatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 30);

    private static ExtractedEntry Entry(string? email = null, string? due = null, double conf = 0.9) =>
        new("Zusammenfassung",
            [new ContactDto("Anna Müller", email, null, null)],
            [new TaskDto("Angebot senden", null, due)],
            conf);

    [Fact]
    public void Valid_entry_has_no_errors() =>
        Assert.Empty(EntryValidator.Validate(Entry("anna@firma.de", "2026-12-05"), Today));

    [Fact]
    public void Past_due_date_is_rejected() =>
        Assert.NotEmpty(EntryValidator.Validate(Entry(due: "2026-01-01"), Today));

    [Fact]
    public void Non_iso_date_is_rejected() =>
        Assert.NotEmpty(EntryValidator.Validate(Entry(due: "5. Dezember"), Today));

    [Fact]
    public void Invalid_email_is_rejected() =>
        Assert.NotEmpty(EntryValidator.Validate(Entry(email: "anna@"), Today));

    [Fact]
    public void Confidence_out_of_range_is_rejected() =>
        Assert.NotEmpty(EntryValidator.Validate(Entry(conf: 1.5), Today));
}
