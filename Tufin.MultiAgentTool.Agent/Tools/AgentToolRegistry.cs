using Tufin.MultiAgentTool.Application.Tools;

namespace Tufin.MultiAgentTool.Agent.Tools;

/// <summary>
///     Registry of backend capabilities exposed to the language model.
/// </summary>
public sealed class AgentToolRegistry : IAgentToolRegistry
{
    private readonly IReadOnlyDictionary<string, IAgentTool> _tools;

    public AgentToolRegistry(IEnumerable<IAgentTool> tools)
    {
        ArgumentNullException.ThrowIfNull(tools);

        var toolArray = tools.ToArray();

        var duplicateNames = toolArray
            .GroupBy(
                tool => tool.Definition.Name,
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateNames.Length > 0)
        {
            throw new InvalidOperationException(
                "Duplicate agent tool names were registered: " +
                string.Join(", ", duplicateNames));
        }

        _tools = toolArray.ToDictionary(
            tool => tool.Definition.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<AgentToolDefinition> GetDefinitions()
    {
        return _tools.Values
            .Select(tool => tool.Definition)
            .OrderBy(definition => definition.Name)
            .ToArray();
    }

    public bool TryResolve(
        string toolName,
        out IAgentTool? tool)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            tool = null;
            return false;
        }

        return _tools.TryGetValue(
            toolName.Trim(),
            out tool);
    }
}