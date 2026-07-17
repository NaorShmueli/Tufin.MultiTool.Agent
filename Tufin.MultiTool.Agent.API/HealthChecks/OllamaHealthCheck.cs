using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Tufin.MultiAgentTool.Infrastructure.LanguageModels.Ollama;

namespace Tufin.MultiTool.Agent.API.HealthChecks;

public sealed class OllamaHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaHealthCheck(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _options.Validate();

        _httpClient.BaseAddress = new Uri(
            _options.BaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(
            Math.Min(_options.TimeoutSeconds, 10));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                "api/tags",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"Ollama returned HTTP {(int)response.StatusCode}.");
            }

            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken);

            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty(
                    "models",
                    out var models) ||
                models.ValueKind != JsonValueKind.Array)
            {
                return HealthCheckResult.Unhealthy(
                    "Ollama /api/tags response did not contain a models array.");
            }

            var configuredModelExists = models
                .EnumerateArray()
                .Any(model =>
                    model.TryGetProperty("name", out var name) &&
                    string.Equals(
                        name.GetString(),
                        _options.Model,
                        StringComparison.OrdinalIgnoreCase));

            return configuredModelExists
                ? HealthCheckResult.Healthy(
                    $"Ollama is reachable and model '{_options.Model}' is available.")
                : HealthCheckResult.Unhealthy(
                    $"Ollama is reachable but model '{_options.Model}' is not available.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Ollama is not reachable.",
                exception);
        }
    }
}
