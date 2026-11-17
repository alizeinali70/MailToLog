using MailToLog.Application.DTOs;
using MailToLog.Application.Interfaces;
using MailToLog.Application.Validators;
using MailToLog.Infrastructure.DI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace MailToLog.Infrastructure.Ollama;

public sealed class OllamaMessageExtractor(IChatClient chat, IOptions<MailToLogOptions> options)
: IMessageExtractor
{
    public const string PromptVersion = "v1";
    private const int MaxAttempts = 2;

    private const string SystemPrompt = """
    Du extrahierst Informationen aus geschäftlichen E-Mails oder Telefontranskripten.
    Regeln:
    - Erfinde nichts. Fehlende Angaben bleiben null.
    - Datumsangaben immer als ISO 8601 (yyyy-MM-dd). Relative Angaben wie "nächsten Freitag"
      nur mit Hilfe des Referenzdatums umrechnen.
    - Nur Personen als Kontakte, die tatsächlich in der Nachricht vorkommen.
    - Confidence niedrig setzen (< 0.6), wenn die Nachricht unklar, widersprüchlich
      oder eine lange weitergeleitete Kette ist.
    """;

    public async Task<ExtractionOutcome> ExtractAsync(string text, DateOnly today, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var messages = new List<ChatMessage>
    {
        new(ChatRole.System, SystemPrompt),
        new(ChatRole.User, $"Referenzdatum (heute): {today:yyyy-MM-dd}\n\nNachricht:\n{text}")
    };
        var chatOptions = new ChatOptions { Temperature = 0f };

        ExtractedEntry? last = null;
        IReadOnlyList<string> errors = [];
        var attempts = 0;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            attempts = attempt;
            try
            {
                // Structured output: the schema of ExtractedEntry is sent to Ollama.
                var response = await chat.GetResponseAsync<ExtractedEntry>(
                    messages, chatOptions, cancellationToken: ct);

                if (response.TryGetResult(out var entry) && entry is not null)
                {
                    last = entry;
                    errors = EntryValidator.Validate(entry, today);
                    if (errors.Count == 0) break;

                    // Retry once and feed the concrete problems back to the model.
                    messages.Add(new ChatMessage(ChatRole.Assistant, response.Text));
                    messages.Add(new ChatMessage(ChatRole.User,
                        "Die Ausgabe hat Fehler. Bitte korrigieren:\n- " + string.Join("\n- ", errors)));
                }
                else
                {
                    errors = ["Model returned no parsable JSON."];
                }
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException)
            {
                errors = [$"Parse error: {ex.Message}"];
            }
        }

        return new ExtractionOutcome(last, errors, attempts, sw.ElapsedMilliseconds,
            options.Value.Model, PromptVersion);
    }
}
