using System.Globalization;
using System.Text;

namespace MailToLog.Domain.Entities;

public static class TextNormalizer
{
    public static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    // "Müller, Anna" == "anna muller" == "Anna  Müller"
    public static string NormalizeName(string name)
    {
        var n = name.Trim();
        var comma = n.IndexOf(',');
        if (comma > 0) n = $"{n[(comma + 1)..].Trim()} {n[..comma].Trim()}";

        return string.Join(' ',
            RemoveDiacritics(n).ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string? NormalizeText(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : RemoveDiacritics(s).Trim().ToLowerInvariant();

    public static int Levenshtein(string a, string b)
    {
        var d = new int[a.Length + 1, b.Length + 1];
        for (var i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (var j = 0; j <= b.Length; j++) d[0, j] = j;

        for (var i = 1; i <= a.Length; i++)
            for (var j = 1; j <= b.Length; j++)
                d[i, j] = Math.Min(Math.Min(
                    d[i - 1, j] + 1,
                    d[i, j - 1] + 1),
                    d[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));

        return d[a.Length, b.Length];
    }

    private static string RemoveDiacritics(string s)
    {
        var sb = new StringBuilder();
        foreach (var ch in s.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
