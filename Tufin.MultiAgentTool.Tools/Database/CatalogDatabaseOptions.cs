namespace Tufin.MultiAgentTool.Tools.Database;

public sealed class CatalogDatabaseOptions
{
    public const string SectionName = "CatalogDatabase";

    public string ConnectionString { get; set; } =
        "Data Source=data/catalog.db";
}
