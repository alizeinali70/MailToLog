using MailToLog.Application.Handlers;
using MailToLog.Application.Interfaces;
using MailToLog.Infrastructure.DI;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMailToLog(builder.Configuration);
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSwaggerGen();

var app = builder.Build();
await app.Services.InitializeMailToLogAsync();

app.MapHealthChecks("/health");

// Submit an email or transcript. Returns 202 and processes in the background.
app.MapPost("/messages", async (SubmitRequest req, SubmitMessageHandler handler, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Text))
        return Results.BadRequest("Text is required.");

    var (id, duplicate) = await handler.HandleAsync(req.Text, ct);
    return duplicate
        ? Results.Ok(new { id, duplicate = true })
        : Results.Accepted($"/messages/{id}", new { id, duplicate = false });
});

// Status of a message plus its ActivityLog entry (once processed).
app.MapGet("/messages/{id:guid}", async (
    Guid id, IMessageRepository messages, IActivityLogRepository logs, CancellationToken ct) =>
{
    var message = await messages.GetAsync(id, ct);
    if (message is null) return Results.NotFound();

    var log = await logs.GetByMessageIdAsync(id, ct);
    return Results.Ok(new { message.Id, status = message.Status.ToString(), activityLog = log });
});

// Human review queue.
app.MapGet("/review", async (IActivityLogRepository logs, CancellationToken ct) =>
    Results.Ok(await logs.ListPendingAsync(ct)));

app.MapPost("/review/{id:guid}/approve", async (Guid id, ReviewHandler handler, CancellationToken ct) =>
    await handler.ApproveAsync(id, ct) ? Results.NoContent() : Results.NotFound());

app.MapPost("/review/{id:guid}/reject", async (Guid id, ReviewHandler handler, CancellationToken ct) =>
    await handler.RejectAsync(id, ct) ? Results.NoContent() : Results.NotFound());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

public record SubmitRequest(string Text);
public partial class Program;