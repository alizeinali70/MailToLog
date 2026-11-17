using MailToLog.Application.DTOs;
using System.Globalization;
using System.Net.Mail;

namespace MailToLog.Application.Validators;

public static class EntryValidator
{
    public static IReadOnlyList<string> Validate(ExtractedEntry e, DateOnly today)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(e.Summary))
            errors.Add("Summary is empty.");

        if (e.Confidence is < 0 or > 1)
            errors.Add("Confidence must be between 0 and 1.");

        foreach (var c in e.Contacts)
        {
            if (string.IsNullOrWhiteSpace(c.Name))
                errors.Add("Contact without a name.");

            if (c.Email is not null && !IsPlausibleEmail(c.Email))
                errors.Add($"Invalid email for '{c.Name}': {c.Email}");
        }

        foreach (var t in e.Tasks)
        {
            if (string.IsNullOrWhiteSpace(t.Description))
                errors.Add("Task without a description.");

            if (t.DueDate is null) continue;

            if (!DateOnly.TryParseExact(t.DueDate, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var due))
                errors.Add($"Task '{t.Description}': due date not ISO 8601: {t.DueDate}");
            else if (due < today.AddDays(-1))
                errors.Add($"Task '{t.Description}': due date {t.DueDate} is in the past.");
        }

        return errors;
    }

    private static bool IsPlausibleEmail(string s) =>
        MailAddress.TryCreate(s, out var a) && a.Address == s;
}
