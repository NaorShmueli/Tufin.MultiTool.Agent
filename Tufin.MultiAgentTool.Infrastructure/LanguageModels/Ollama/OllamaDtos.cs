using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tufin.MultiAgentTool.Infrastructure.LanguageModels.Ollama;

internal sealed class OllamaChatRequest
{
    [JsonPropertyName("model")] public string Model { get; init; } = string.Empty;

    [JsonPropertyName("messages")]
    public IReadOnlyList<OllamaMessage> Messages { get; init; } =
        Array.Empty<OllamaMessage>();

    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<OllamaToolDefinition>? Tools { get; init; }

    [JsonPropertyName("stream")] public bool Stream { get; init; }

    [JsonPropertyName("think")] public bool Think { get; init; }

    [JsonPropertyName("keep_alive")] public string? KeepAlive { get; init; }

    [JsonPropertyName("options")] public OllamaGenerationOptions Options { get; init; } = new();
}

internal sealed class OllamaGenerationOptions
{
    [JsonPropertyName("temperature")] public double Temperature { get; init; }
}

internal sealed class OllamaMessage
{
    [JsonPropertyName("role")] public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; init; }

    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<OllamaToolCall>? ToolCalls { get; init; }

    [JsonPropertyName("tool_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolName { get; init; }
}

internal sealed class OllamaToolDefinition
{
    [JsonPropertyName("type")] public string Type { get; init; } = "function";

    [JsonPropertyName("function")] public OllamaToolFunctionDefinition Function { get; init; } = new();
}

internal sealed class OllamaToolFunctionDefinition
{
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")] public string Description { get; init; } = string.Empty;

    [JsonPropertyName("parameters")] public JsonElement Parameters { get; init; }
}

internal sealed class OllamaToolCall
{
    [JsonPropertyName("type")] public string Type { get; init; } = "function";

    [JsonPropertyName("function")] public OllamaToolCallFunction Function { get; init; } = new();
}

internal sealed class OllamaToolCallFunction
{
    [JsonPropertyName("index")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Index { get; init; }

    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    [JsonPropertyName("arguments")] public JsonElement Arguments { get; init; }
}

internal sealed class OllamaChatResponse
{
    [JsonPropertyName("model")] public string Model { get; init; } = string.Empty;

    [JsonPropertyName("message")] public OllamaMessage Message { get; init; } = new();

    [JsonPropertyName("done")] public bool Done { get; init; }

    [JsonPropertyName("done_reason")] public string? DoneReason { get; init; }

    [JsonPropertyName("total_duration")] public long TotalDurationNanoseconds { get; init; }

    [JsonPropertyName("load_duration")] public long LoadDurationNanoseconds { get; init; }

    [JsonPropertyName("prompt_eval_count")]
    public int PromptEvalCount { get; init; }

    [JsonPropertyName("prompt_eval_duration")]
    public long PromptEvalDurationNanoseconds { get; init; }

    [JsonPropertyName("eval_count")] public int EvalCount { get; init; }

    [JsonPropertyName("eval_duration")] public long EvalDurationNanoseconds { get; init; }
}