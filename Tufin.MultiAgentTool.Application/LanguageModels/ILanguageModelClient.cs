namespace Tufin.MultiAgentTool.Application.LanguageModels;

/// <summary>
/// Provider-independent language model client.
///
/// Infrastructure may implement this interface using:
/// Ollama, OpenAI, Anthropic, or another provider.
/// </summary>
public interface ILanguageModelClient
{
    string ModelName { get; }

    Task<LanguageModelResponse> CompleteAsync(
        LanguageModelRequest request,
        CancellationToken cancellationToken);
}