namespace Tufin.MultiAgentTool.Tools.UnitConversion;

internal sealed record UnitDefinition(
    string CanonicalName,
    string Category,
    Func<double, double> ToBaseUnit,
    Func<double, double> FromBaseUnit);