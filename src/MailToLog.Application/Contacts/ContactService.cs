using MailToLog.Application.DTOs;
using MailToLog.Application.Interfaces;
using MailToLog.Domain.Entities;
using MailToLog.Domain.Enums;

namespace MailToLog.Application.Contacts;

public class ContactService(IContactRepository contacts, ContactMatcher matcher)
{
    public async Task<Guid?> ApplyAsync(IEnumerable<ContactDto> dtos, CancellationToken ct)
    {
        Guid? primary = null;
        var created = new Dictionary<string, Guid>();   // dedupe inside one message

        foreach (var dto in dtos)
        {
            var key = TextNormalizer.NormalizeEmail(dto.Email) ?? TextNormalizer.NormalizeName(dto.Name);
            if (created.TryGetValue(key, out var knownId)) { primary ??= knownId; continue; }

            var match = await matcher.FindAsync(dto, ct);
            if (match.Kind != MatchKind.New) { primary ??= match.Existing!.Id; continue; }

            var contact = new Contact
            {
                Name = dto.Name.Trim(),
                NormalizedName = TextNormalizer.NormalizeName(dto.Name),
                Email = TextNormalizer.NormalizeEmail(dto.Email),
                Phone = dto.Phone?.Trim(),
                Company = dto.Company?.Trim()
            };
            await contacts.AddAsync(contact, ct);
            created[key] = contact.Id;
            primary ??= contact.Id;
        }

        return primary;
    }
}
