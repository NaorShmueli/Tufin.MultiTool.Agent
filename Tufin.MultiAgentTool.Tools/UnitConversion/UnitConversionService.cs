namespace Tufin.MultiAgentTool.Tools.UnitConversion;

public sealed class UnitConversionService
{
    private static readonly IReadOnlyDictionary<string, UnitDefinition>
        Units = CreateUnits();

    public UnitConversionResult Convert(
        double value,
        string fromUnit,
        string toUnit)
    {
        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            throw new UnitConversionException(
                "Value must be a finite number.");
        }

        if (string.IsNullOrWhiteSpace(fromUnit))
        {
            throw new UnitConversionException(
                "Source unit is required.");
        }

        if (string.IsNullOrWhiteSpace(toUnit))
        {
            throw new UnitConversionException(
                "Target unit is required.");
        }

        if (!Units.TryGetValue(
                Normalize(fromUnit),
                out var source))
        {
            throw new UnitConversionException(
                $"Unsupported source unit '{fromUnit}'.");
        }

        if (!Units.TryGetValue(
                Normalize(toUnit),
                out var target))
        {
            throw new UnitConversionException(
                $"Unsupported target unit '{toUnit}'.");
        }

        if (!string.Equals(
                source.Category,
                target.Category,
                StringComparison.Ordinal))
        {
            throw new UnitConversionException(
                $"Cannot convert from '{source.CanonicalName}' " +
                $"to '{target.CanonicalName}' because they belong " +
                "to different measurement categories.");
        }

        var valueInBaseUnit = source.ToBaseUnit(value);
        var convertedValue = target.FromBaseUnit(valueInBaseUnit);

        if (double.IsNaN(convertedValue) ||
            double.IsInfinity(convertedValue))
        {
            throw new UnitConversionException(
                "Conversion produced a non-finite result.");
        }

        return new UnitConversionResult(
            InputValue: value,
            FromUnit: source.CanonicalName,
            OutputValue: convertedValue,
            ToUnit: target.CanonicalName,
            Category: source.Category);
    }

    private static string Normalize(string unit)
    {
        return unit
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty);
    }

    private static IReadOnlyDictionary<string, UnitDefinition>
        CreateUnits()
    {
        var units = new Dictionary<string, UnitDefinition>(
            StringComparer.OrdinalIgnoreCase);

        AddUnit(
            units,
            new UnitDefinition(
                "meter",
                "length",
                value => value,
                value => value),
            "m", "meter", "meters", "metre", "metres");

        AddLinearUnit(
            units,
            canonicalName: "kilometer",
            category: "length",
            multiplierToBase: 1_000,
            aliases:
            [
                "km",
                "kilometer",
                "kilometers",
                "kilometre",
                "kilometres"
            ]);

        AddLinearUnit(
            units,
            canonicalName: "centimeter",
            category: "length",
            multiplierToBase: 0.01,
            aliases:
            [
                "cm",
                "centimeter",
                "centimeters",
                "centimetre",
                "centimetres"
            ]);

        AddLinearUnit(
            units,
            canonicalName: "millimeter",
            category: "length",
            multiplierToBase: 0.001,
            aliases:
            [
                "mm",
                "millimeter",
                "millimeters",
                "millimetre",
                "millimetres"
            ]);

        AddLinearUnit(
            units,
            canonicalName: "mile",
            category: "length",
            multiplierToBase: 1_609.344,
            aliases: ["mi", "mile", "miles"]);

        AddLinearUnit(
            units,
            canonicalName: "yard",
            category: "length",
            multiplierToBase: 0.9144,
            aliases: ["yd", "yard", "yards"]);

        AddLinearUnit(
            units,
            canonicalName: "foot",
            category: "length",
            multiplierToBase: 0.3048,
            aliases: ["ft", "foot", "feet"]);

        AddLinearUnit(
            units,
            canonicalName: "inch",
            category: "length",
            multiplierToBase: 0.0254,
            aliases: ["in", "inch", "inches"]);

        AddUnit(
            units,
            new UnitDefinition(
                "kilogram",
                "weight",
                value => value,
                value => value),
            "kg", "kilogram", "kilograms");

        AddLinearUnit(
            units,
            canonicalName: "gram",
            category: "weight",
            multiplierToBase: 0.001,
            aliases: ["g", "gram", "grams"]);

        AddLinearUnit(
            units,
            canonicalName: "pound",
            category: "weight",
            multiplierToBase: 0.45359237,
            aliases: ["lb", "lbs", "pound", "pounds"]);

        AddLinearUnit(
            units,
            canonicalName: "ounce",
            category: "weight",
            multiplierToBase: 0.028349523125,
            aliases: ["oz", "ounce", "ounces"]);

        AddUnit(
            units,
            new UnitDefinition(
                "celsius",
                "temperature",
                value => value,
                value => value),
            "c",
            "°c",
            "celsius",
            "centigrade");

        AddUnit(
            units,
            new UnitDefinition(
                "fahrenheit",
                "temperature",
                value => (value - 32) * 5 / 9,
                value => (value * 9 / 5) + 32),
            "f",
            "°f",
            "fahrenheit");

        AddUnit(
            units,
            new UnitDefinition(
                "kelvin",
                "temperature",
                value => value - 273.15,
                value => value + 273.15),
            "k",
            "kelvin");

        return units;
    }

    private static void AddLinearUnit(
        IDictionary<string, UnitDefinition> units,
        string canonicalName,
        string category,
        double multiplierToBase,
        string[] aliases)
    {
        var definition = new UnitDefinition(
            CanonicalName: canonicalName,
            Category: category,
            ToBaseUnit: value => value * multiplierToBase,
            FromBaseUnit: value => value / multiplierToBase);

        AddUnit(units, definition, aliases);
    }

    private static void AddUnit(
        IDictionary<string, UnitDefinition> units,
        UnitDefinition definition,
        params string[] aliases)
    {
        foreach (var alias in aliases.Append(
                     definition.CanonicalName))
        {
            units[Normalize(alias)] = definition;
        }
    }
}

public sealed record UnitConversionResult(
    double InputValue,
    string FromUnit,
    double OutputValue,
    string ToUnit,
    string Category);

public sealed class UnitConversionException : Exception
{
    public UnitConversionException(string message)
        : base(message)
    {
    }
}