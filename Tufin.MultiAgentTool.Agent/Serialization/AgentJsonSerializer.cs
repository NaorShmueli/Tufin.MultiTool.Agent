using System.Text.Json;
using System.Text.Json.Serialization;
using Tufin.MultiAgentTool.Application.Tools;

namespace Tufin.MultiAgentTool.Agent.Serialization;

public sealed class AgentJsonSerializer
{
    private readonly JsonSerializerOptions _options;

    public AgentJsonSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public string SerializeToolResult(
        AgentToolExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            return JsonSerializer.Serialize(
                new
                {
                    success = true,
                    output = result.Output
                },
                _options);
        }

        return JsonSerializer.Serialize(
            new
            {
                success = false,
                error = new
                {
                    code = result.ErrorCode,
                    message = result.ErrorMessage
                }
            },
            _options);
    }

    public string SerializeJsonElement(JsonElement value)
    {
        return JsonSerializer.Serialize(
            value,
            _options);
    }
}