using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MailToLog.Infrastructure.DI;

public sealed class OllamaHealthCheck(IOptions<MailToLogOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            using var http = new HttpClient
            {
                BaseAddress = new Uri(options.Value.OllamaUrl),
                Timeout = TimeSpan.FromSeconds(3)
            };
            var response = await http.GetAsync("/api/tags", ct);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Ollama returned {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Ollama not reachable", ex);
        }
    }
}
