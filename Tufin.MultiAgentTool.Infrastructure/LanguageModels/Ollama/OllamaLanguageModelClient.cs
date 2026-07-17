using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tufin.MultiAgentTool.Application.LanguageModels;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Domain.Metrics;

namespace Tufin.MultiAgentTool.Infrastructure.LanguageModels.Ollama;

public sealed class OllamaLanguageModelClient
    : ILanguageModelClient
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaLanguageModelClient> _logger;
    private readonly OllamaOptions _options;

    public OllamaLanguageModelClient(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ILogger<OllamaLanguageModelClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _options.Validate();
    }

    public string ModelName => _options.Model;

    public async Task<LanguageModelResponse> CompleteAsync(
        LanguageModelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ollamaRequest = new OllamaChatRequest
        {
            Model = _options.Model,
            Messages = request.Messages
                .Select(MapMessage)
                .ToArray(),
            Tools = request.Tools.Count == 0
                ? null
                : request.Tools
                    .Select(MapToolDefinition)
                    .ToArray(),
            Stream = false,
            Think = false,
            KeepAlive = _options.KeepAlive,
            Options = new OllamaGenerationOptions
            {
                Temperature = request.Temperature
            }
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response =
                await _httpClient.PostAsJsonAsync(
                    "api/chat",
                    ollamaRequest,
                    JsonOptions,
                    cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                throw new LanguageModelProviderException(
                    $"Ollama returned HTTP {(int)response.StatusCode} " +
                    $"({response.ReasonPhrase}). Response: " +
                    Truncate(responseBody, 1_000));
            }

            var ollamaResponse =
                JsonSerializer.Deserialize<OllamaChatResponse>(
                    responseBody,
                    JsonOptions);

            if (ollamaResponse is null)
            {
                throw new LanguageModelProviderException(
                    "Ollama returned an empty or invalid response.");
            }

            var toolCalls =
                MapToolCalls(ollamaResponse.Message.ToolCalls);

            var content =
                string.IsNullOrWhiteSpace(
                    ollamaResponse.Message.Content)
                    ? null
                    : ollamaResponse.Message.Content.Trim();

            return new LanguageModelResponse(
                string.IsNullOrWhiteSpace(
                    ollamaResponse.Model)
                    ? _options.Model
                    : ollamaResponse.Model,
                content,
                toolCalls,
                new TokenUsage(
                    Math.Max(
                        0,
                        ollamaResponse.PromptEvalCount),
                    Math.Max(
                        0,
                        ollamaResponse.EvalCount)),

                // End-to-end latency as observed by our backend.
                stopwatch.Elapsed,
                ollamaResponse.DoneReason);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException exception)
        {
            stopwatch.Stop();

            throw new LanguageModelProviderException(
                "The Ollama request exceeded the configured timeout.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                exception,
                "Could not communicate with Ollama at {BaseAddress}.",
                _httpClient.BaseAddress);

            throw new LanguageModelProviderException(
                "The local language model service is unavailable.",
                exception);
        }
        catch (JsonException exception)
        {
            stopwatch.Stop();

            throw new LanguageModelProviderException(
                "Ollama returned invalid JSON.",
                exception);
        }
    }

    private static OllamaMessage MapMessage(
        LanguageModelMessage message)
    {
        return message.Role switch
        {
            LanguageModelRole.System =>
                new OllamaMessage
                {
                    Role = "system",
                    Content = message.Content
                },

            LanguageModelRole.User =>
                new OllamaMessage
                {
                    Role = "user",
                    Content = message.Content
                },

            LanguageModelRole.Assistant
                when message.ToolCalls.Count > 0 =>
                new OllamaMessage
                {
                    Role = "assistant",
                    Content = message.Content,
                    ToolCalls = message.ToolCalls
                        .Select(
                            (toolCall, index) =>
                                new OllamaToolCall
                                {
                                    Type = "function",
                                    Function =
                                        new OllamaToolCallFunction
                                        {
                                            Index = index,
                                            Name = toolCall.Name,
                                            Arguments =
                                                toolCall.Arguments.Clone()
                                        }
                                })
                        .ToArray()
                },

            LanguageModelRole.Assistant =>
                new OllamaMessage
                {
                    Role = "assistant",
                    Content = message.Content
                },

            LanguageModelRole.Tool =>
                new OllamaMessage
                {
                    Role = "tool",
                    ToolName = message.ToolName,
                    Content = message.Content
                },

            _ => throw new ArgumentOutOfRangeException(
                nameof(message.Role),
                message.Role,
                "Unsupported language-model message role.")
        };
    }

    private static OllamaToolDefinition MapToolDefinition(
        AgentToolDefinition definition)
    {
        return new OllamaToolDefinition
        {
            Type = "function",
            Function = new OllamaToolFunctionDefinition
            {
                Name = definition.Name,
                Description = definition.Description,
                Parameters = definition.InputSchema.Clone()
            }
        };
    }

    private static IReadOnlyList<LanguageModelToolCall>
        MapToolCalls(
            IReadOnlyList<OllamaToolCall>? ollamaCalls)
    {
        if (ollamaCalls is null ||
            ollamaCalls.Count == 0)
        {
            return Array.Empty<LanguageModelToolCall>();
        }

        var result =
            new List<LanguageModelToolCall>(
                ollamaCalls.Count);

        for (var index = 0;
             index < ollamaCalls.Count;
             index++)
        {
            var call = ollamaCalls[index];

            if (string.IsNullOrWhiteSpace(
                    call.Function.Name))
            {
                throw new LanguageModelProviderException(
                    "Ollama returned a tool call without a tool name.");
            }

            if (call.Function.Arguments.ValueKind !=
                JsonValueKind.Object)
            {
                throw new LanguageModelProviderException(
                    $"Ollama returned invalid arguments for tool " +
                    $"'{call.Function.Name}'.");
            }

            result.Add(
                new LanguageModelToolCall(
                    $"ollama-" +
                    $"{Guid.NewGuid():N}-" +
                    $"{index}",
                    call.Function.Name,
                    call.Function.Arguments));
        }

        return result;
    }

    private static string Truncate(
        string value,
        int maximumLength)
    {
        if (string.IsNullOrEmpty(value) ||
            value.Length <= maximumLength)
        {
            return value;
        }

        return value[..maximumLength] + "...";
    }
}