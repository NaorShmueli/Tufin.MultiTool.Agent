namespace Tufin.MultiAgentTool.Infrastructure.LanguageModels.Ollama;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } =
        "http://localhost:11434";

    public string Model { get; set; } =
        "llama3.2:3b";

    public int TimeoutSeconds { get; set; } = 120;

    public string KeepAlive { get; set; } = "10m";

    public void Validate()
    {
        if (!Uri.TryCreate(
                BaseUrl,
                UriKind.Absolute,
                out _))
        {
            throw new InvalidOperationException(
                "Ollama BaseUrl must be an absolute URL.");
        }

        if (string.IsNullOrWhiteSpace(Model))
        {
            throw new InvalidOperationException(
                "Ollama model is required.");
        }

        if (TimeoutSeconds is < 1 or > 600)
        {
            throw new InvalidOperationException(
                "Ollama TimeoutSeconds must be between 1 and 600.");
        }

        if (string.IsNullOrWhiteSpace(KeepAlive))
        {
            throw new InvalidOperationException(
                "Ollama KeepAlive is required.");
        }
    }
}