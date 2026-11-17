using MailToLog.Application.DTOs;
using MailToLog.Application.Interfaces;
using MailToLog.Domain.Entities;
using MailToLog.Domain.Enums;

namespace MailToLog.Application.Contacts;

public class ContactMatcher(IContactRepository contacts)
{
    // 1. same email -> Exact   2. same name + company -> Exact
    // 3. same name or <= 1 typo -> Possible (human decides)   4. otherwise New
    public async Task<MatchResult> FindAsync(ContactDto dto, CancellationToken ct)
    {
        var email = TextNormalizer.NormalizeEmail(dto.Email);
        if (email is not null)
        {
            var byMail = await contacts.FindByEmailAsync(email, ct);
            if (byMail is not null) return new(MatchKind.Exact, byMail);
        }

        var name = TextNormalizer.NormalizeName(dto.Name);
        var company = TextNormalizer.NormalizeText(dto.Company);
        var all = await contacts.ListAsync(ct);

        foreach (var c in all)
            if (c.NormalizedName == name && company is not null &&
                TextNormalizer.NormalizeText(c.Company) == company)
                return new(MatchKind.Exact, c);

        foreach (var c in all)
            if (c.NormalizedName == name || TextNormalizer.Levenshtein(c.NormalizedName, name) <= 1)
                return new(MatchKind.Possible, c);

        return new(MatchKind.New, null);
    }
}
