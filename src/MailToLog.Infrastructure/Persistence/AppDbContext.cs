using MailToLog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MailToLog.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<IncomingMessage> Messages => Set<IncomingMessage>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<IncomingMessage>().HasIndex(m => m.ContentHash).IsUnique();
        b.Entity<IncomingMessage>().Property(m => m.Status).HasConversion<string>();

        b.Entity<Contact>().HasIndex(c => c.Email).IsUnique();   // safety net against races

        b.Entity<ActivityLog>().Property(l => l.Status).HasConversion<string>();
        b.Entity<ActivityLog>().HasIndex(l => l.MessageId);
    }
}
