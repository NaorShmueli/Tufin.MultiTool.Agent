using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Tufin.MultiAgentTool.Tools.Database;

public sealed class CatalogDatabaseInitializer
{
    private readonly CatalogDatabaseOptions _options;

    public CatalogDatabaseInitializer(
        IOptions<CatalogDatabaseOptions> options)
    {
        _options = options.Value;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureDatabaseDirectoryExists(
            _options.ConnectionString);

        await using var connection = new SqliteConnection(
            _options.ConnectionString);

        await connection.OpenAsync(cancellationToken);

        await ExecuteNonQueryAsync(
            connection,
            """
            CREATE TABLE IF NOT EXISTS products (
                id INTEGER PRIMARY KEY,
                name TEXT NOT NULL UNIQUE,
                category TEXT NOT NULL,
                price REAL NOT NULL,
                stock_quantity INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS orders (
                id INTEGER PRIMARY KEY,
                customer_name TEXT NOT NULL,
                order_date TEXT NOT NULL,
                status TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS order_items (
                id INTEGER PRIMARY KEY,
                order_id INTEGER NOT NULL,
                product_id INTEGER NOT NULL,
                quantity INTEGER NOT NULL,
                unit_price REAL NOT NULL,
                FOREIGN KEY(order_id) REFERENCES orders(id),
                FOREIGN KEY(product_id) REFERENCES products(id)
            );
            """,
            cancellationToken);

        var productCount = await ExecuteScalarAsync(
            connection,
            "SELECT COUNT(*) FROM products;",
            cancellationToken);

        if (Convert.ToInt32(productCount) > 0)
        {
            return;
        }

        await ExecuteNonQueryAsync(
            connection,
            """
            INSERT INTO products (id, name, category, price, stock_quantity) VALUES
                (1, 'iPhone 17', 'Smartphones', 1199.00, 42),
                (2, 'iPhone 17 Pro', 'Smartphones', 1499.00, 18),
                (3, 'MacBook Air 15', 'Laptops', 1299.00, 12),
                (4, 'ThinkPad X1 Carbon', 'Laptops', 1699.00, 7),
                (5, 'Sony WH-1000XM6', 'Audio', 399.00, 31),
                (6, 'USB-C Cable', 'Accessories', 19.00, 4),
                (7, 'Laptop Stand', 'Accessories', 49.00, 6),
                (8, 'Dell UltraSharp 27', 'Monitors', 549.00, 11);

            INSERT INTO orders (id, customer_name, order_date, status) VALUES
                (1001, 'Miri Cohen', '2026-07-01', 'pending'),
                (1002, 'Daniel Levi', '2026-07-03', 'shipped'),
                (1003, 'Amit Bar', '2026-07-05', 'delivered'),
                (1004, 'Noa Green', '2026-07-08', 'pending');

            INSERT INTO order_items (id, order_id, product_id, quantity, unit_price) VALUES
                (1, 1001, 1, 2, 1199.00),
                (2, 1001, 6, 3, 19.00),
                (3, 1002, 3, 1, 1299.00),
                (4, 1002, 5, 1, 399.00),
                (5, 1003, 8, 2, 549.00),
                (6, 1004, 4, 1, 1699.00),
                (7, 1004, 7, 2, 49.00);
            """,
            cancellationToken);
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<object?> ExecuteScalarAsync(
        SqliteConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;

        return await command.ExecuteScalarAsync(cancellationToken);
    }

    private static void EnsureDatabaseDirectoryExists(
        string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(
            connectionString);

        if (string.IsNullOrWhiteSpace(builder.DataSource) ||
            builder.DataSource.Equals(
                ":memory:",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var directory = Path.GetDirectoryName(
            Path.GetFullPath(builder.DataSource));

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
