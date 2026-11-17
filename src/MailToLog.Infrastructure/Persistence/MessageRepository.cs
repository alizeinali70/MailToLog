using MailToLog.Application.Interfaces;
using MailToLog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MailToLog.Infrastructure.Persistence;

public sealed class MessageRepository(AppDbContext db) : IMessageRepository
{
    public Task<IncomingMessage?> FindByHashAsync(string hash, CancellationToken ct) =>
        db.Messages.FirstOrDefaultAsync(m => m.ContentHash == hash, ct);

    public Task<IncomingMessage?> GetAsync(Guid id, CancellationToken ct) =>
        db.Messages.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task AddAsync(IncomingMessage message, CancellationToken ct) =>
        await db.Messages.AddAsync(message, ct);
}
