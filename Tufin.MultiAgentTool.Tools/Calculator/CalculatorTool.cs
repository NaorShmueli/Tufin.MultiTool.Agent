using System.Text.Json;
using Tufin.MultiAgentTool.Application.Tools;

namespace Tufin.MultiAgentTool.Tools.Calculator;

public sealed class CalculatorTool : IAgentTool
{
    private readonly SafeMathExpressionEvaluator _evaluator;

    public CalculatorTool(
        SafeMathExpressionEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    public AgentToolDefinition Definition { get; } =
        new(
            "calculator",
            @"
                Safely evaluates a mathematical expression.

                Use this tool for arithmetic calculations after all required
                numeric inputs are known.

                Supported operators:
                +, -, *, /, %, ^ and parentheses.

                For percentages, express the percentage explicitly.
                Example: 15 percent of 68 should be sent as 68 * 0.15.

                Do not use this tool to fetch external or current data.
                ",
            JsonSerializer.SerializeToElement(
                new
                {
                    type = "object",
                    properties = new
                    {
                        expression = new
                        {
                            type = "string",
                            description =
                                "A mathematical expression, for example " +
                                "'(20 * 9 / 5) + 32' or '68 * 0.15'.",
                            minLength = 1,
                            maxLength = 256
                        }
                    },
                    required = new[] { "expression" },
                    additionalProperties = false
                }));

    public Task<AgentToolExecutionResult> ExecuteAsync(
        JsonElement arguments,
        AgentToolExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!arguments.TryGetProperty(
                "expression",
                out var expressionProperty) ||
            expressionProperty.ValueKind != JsonValueKind.String)
        {
            return Task.FromResult(
                AgentToolExecutionResult.Failure(
                    "invalid_arguments",
                    "A string property named 'expression' is required."));
        }

        var expression = expressionProperty.GetString();

        if (string.IsNullOrWhiteSpace(expression))
        {
            return Task.FromResult(
                AgentToolExecutionResult.Failure(
                    "invalid_arguments",
                    "Expression cannot be empty."));
        }

        try
        {
            var result = _evaluator.Evaluate(expression);

            var output = JsonSerializer.SerializeToElement(
                new
                {
                    expression,
                    result
                });

            return Task.FromResult(
                AgentToolExecutionResult.Success(output));
        }
        catch (MathExpressionException exception)
        {
            return Task.FromResult(
                AgentToolExecutionResult.Failure(
                    "invalid_expression",
                    exception.Message));
        }
    }
}