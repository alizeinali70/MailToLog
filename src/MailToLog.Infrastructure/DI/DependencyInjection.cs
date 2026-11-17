using MailToLog.Application.Contacts;
using MailToLog.Application.Handlers;
using MailToLog.Application.Interfaces;
using MailToLog.Domain.Entities;
using MailToLog.Infrastructure.Messaging;
using MailToLog.Infrastructure.Ollama;
using MailToLog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace MailToLog.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddMailToLog(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection("MailToLog");
        services.Configure<MailToLogOptions>(section);
        var opts = section.Get<MailToLogOptions>() ?? new MailToLogOptions();

        // Persistence
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(opts.ConnectionString));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

        // LLM (local Ollama behind the IMessageExtractor port)
        services.AddSingleton<IChatClient>(_ => new OllamaApiClient(
            new HttpClient { BaseAddress = new Uri(opts.OllamaUrl), Timeout = TimeSpan.FromMinutes(5) },
            opts.Model));
        services.AddScoped<IMessageExtractor, OllamaMessageExtractor>();

        // Infrastructure services
        services.AddSingleton<IMessageQueue, ChannelMessageQueue>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddHostedService<ProcessingWorker>();
        services.AddHealthChecks().AddCheck<OllamaHealthCheck>("ollama");

        // Domain + Application
        services.AddSingleton(new ReviewPolicy(opts.MinConfidence));
        services.AddScoped<ContactMatcher>();
        services.AddScoped<ContactService>();
        services.AddScoped<SubmitMessageHandler>();
        services.AddScoped<ProcessMessageHandler>();
        services.AddScoped<ReviewHandler>();

        return services;
    }

    public static async Task InitializeMailToLogAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
    }
}
