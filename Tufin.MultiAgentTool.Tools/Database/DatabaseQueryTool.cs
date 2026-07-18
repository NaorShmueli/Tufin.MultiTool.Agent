using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Tufin.MultiAgentTool.Application.Tools;

namespace Tufin.MultiAgentTool.Tools.Database;

public sealed class DatabaseQueryTool : IAgentTool
{
    private const int MaximumQueryLength = 2_000;
    private const int MaximumRows = 50;
    private readonly CatalogDatabaseOptions _options;

    public DatabaseQueryTool(
        IOptions<CatalogDatabaseOptions> options)
    {
        _options = options.Value;
    }

    public AgentToolDefinition Definition { get; } =
        new(
            "database_query",
            @"
            Executes a read-only SQL SELECT query against a pre-seeded
            fictional product catalog SQLite database.

            Use this tool for questions about catalog products, prices,
            inventory, orders, customers, order status, and order totals.

            Available schema:
            - products(id, name, category, price, stock_quantity)
            - orders(id, customer_name, order_date, status)
            - order_items(id, order_id, product_id, quantity, unit_price)

            Known seeded product names include:
            'iPhone 17', 'iPhone 17 Pro', 'MacBook Air 15',
            'ThinkPad X1 Carbon', 'Sony WH-1000XM6', 'USB-C Cable',
            'Laptop Stand', and 'Dell UltraSharp 27'.

            SQL examples:
            - SELECT name, price FROM products WHERE name = 'iPhone 17'
            - SELECT name, stock_quantity FROM products WHERE stock_quantity < 10
            - SELECT SUM(quantity * unit_price) AS total FROM order_items WHERE order_id = 1001

            Only SELECT queries are allowed. Do not call this tool for
            calculations unrelated to catalog data.
            ",
            JsonSerializer.SerializeToElement(
                new
                {
                    type = "object",
                    properties = new
                    {
                        query = new
                        {
                            type = "string",
                            description = @"
                                          One complete, read-only SQLite SELECT statement.
                                          For text literals, use only the ASCII apostrophe ' (U+0027).
                                          Never use typographic quotes, Unicode escape sequences, or an
                                          incomplete condition ending with an operator.
                                          Example:
                                          SELECT name, price FROM products WHERE name = 'iPhone 17'
                                          ",
                            minLength = 1,
                            maxLength = MaximumQueryLength
                        }
                    },
                    required = new[] { "query" },
                    additionalProperties = false
                }));

    public async Task<AgentToolExecutionResult> ExecuteAsync(
        JsonElement arguments,
        AgentToolExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!arguments.TryGetProperty(
                "query",
                out var queryProperty) ||
            queryProperty.ValueKind != JsonValueKind.String)
        {
            return AgentToolExecutionResult.Failure(
                "invalid_arguments",
                "A string property named 'query' is required.");
        }

        var query = queryProperty.GetString()?.Trim();
        query = NormalizeQuery(query);

        var validationError = ValidateQuery(query);
        if (validationError is not null)
        {
            return AgentToolExecutionResult.Failure(
                "unsafe_query",
                validationError);
        }

        try
        {
            var connectionString = CreateReadOnlyConnectionString(
                _options.ConnectionString);

            await using var connection = new SqliteConnection(
                connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = query;

            await using var reader = await command.ExecuteReaderAsync(
                cancellationToken);

            var columns = Enumerable
                .Range(0, reader.FieldCount)
                .Select(reader.GetName)
                .ToArray();

            var rows = new List<Dictionary<string, object?>>();

            while (rows.Count < MaximumRows &&
                   await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>(
                    StringComparer.OrdinalIgnoreCase);

                for (var index = 0; index < columns.Length; index++)
                {
                    row[columns[index]] = await reader.IsDBNullAsync(
                        index,
                        cancellationToken)
                        ? null
                        : reader.GetValue(index);
                }

                rows.Add(row);
            }

            var output = JsonSerializer.SerializeToElement(
                new
                {
                    query,
                    columns,
                    rows,
                    rowCount = rows.Count,
                    truncated = rows.Count == MaximumRows,
                    maxRows = MaximumRows
                });

            return AgentToolExecutionResult.Success(output);
        }
        catch (Exception exception)
        {
            return AgentToolExecutionResult.Failure(
                "query_failed",
                exception.Message);
        }
    }
    private static string NormalizeQuery(string query)
    {
        return query
            // Literal escape sequences returned as text
            .Replace("\\u2018", "'", StringComparison.OrdinalIgnoreCase)
            .Replace("\\u2019", "'", StringComparison.OrdinalIgnoreCase)

            // Actual Unicode quotation characters
            .Replace("\u2018", "'")
            .Replace("\u2019", "'")
            .Trim();
    }
    private static string? ValidateQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Query cannot be empty.";
        }

        if (query.Length > MaximumQueryLength)
        {
            return $"Query cannot exceed {MaximumQueryLength} characters.";
        }

        if (!query.StartsWith(
                "select",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Only SELECT queries are allowed.";
        }

        if (query.Contains(';'))
        {
            return "Only a single SQL statement without semicolons is allowed.";
        }

        if (query.EndsWith(
                "=",
                StringComparison.Ordinal) ||
            query.Contains(
                "= ",
                StringComparison.Ordinal) &&
            query.TrimEnd().EndsWith(
                "=",
                StringComparison.Ordinal))
        {
            return "The SQL query is incomplete. Include the value after '=' and quote text values, for example: SELECT name, price FROM products WHERE name = 'iPhone 17'.";
        }

        var forbidden = new[]
        {
            "insert",
            "update",
            "delete",
            "drop",
            "alter",
            "create",
            "attach",
            "detach",
            "pragma",
            "vacuum",
            "replace"
        };

        return forbidden.Any(keyword => ContainsKeyword(query, keyword))
            ? "Only read-only catalog SELECT queries are allowed."
            : null;
    }

    private static bool ContainsKeyword(
        string query,
        string keyword)
    {
        return query.Split(
                [' ', '\t', '\r', '\n', '(', ')', ','],
                StringSplitOptions.RemoveEmptyEntries)
            .Any(part => string.Equals(
                part,
                keyword,
                StringComparison.OrdinalIgnoreCase));
    }

    private static string CreateReadOnlyConnectionString(
        string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(
            connectionString)
        {
            Mode = SqliteOpenMode.ReadOnly
        };

        return builder.ToString();
    }
}
