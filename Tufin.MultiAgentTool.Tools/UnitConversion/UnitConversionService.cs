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
            value,
            source.CanonicalName,
            convertedValue,
            target.CanonicalName,
            source.Category);
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
            "kilometer",
            "length",
            1_000,
            [
                "km",
                "kilometer",
                "kilometers",
                "kilometre",
                "kilometres"
            ]);

        AddLinearUnit(
            units,
            "centimeter",
            "length",
            0.01,
            [
                "cm",
                "centimeter",
                "centimeters",
                "centimetre",
                "centimetres"
            ]);

        AddLinearUnit(
            units,
            "millimeter",
            "length",
            0.001,
            [
                "mm",
                "millimeter",
                "millimeters",
                "millimetre",
                "millimetres"
            ]);

        AddLinearUnit(
            units,
            "mile",
            "length",
            1_609.344,
            ["mi", "mile", "miles"]);

        AddLinearUnit(
            units,
            "yard",
            "length",
            0.9144,
            ["yd", "yard", "yards"]);

        AddLinearUnit(
            units,
            "foot",
            "length",
            0.3048,
            ["ft", "foot", "feet"]);

        AddLinearUnit(
            units,
            "inch",
            "length",
            0.0254,
            ["in", "inch", "inches"]);

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
            "gram",
            "weight",
            0.001,
            ["g", "gram", "grams"]);

        AddLinearUnit(
            units,
            "pound",
            "weight",
            0.45359237,
            ["lb", "lbs", "pound", "pounds"]);

        AddLinearUnit(
            units,
            "ounce",
            "weight",
            0.028349523125,
            ["oz", "ounce", "ounces"]);

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
                value => value * 9 / 5 + 32),
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
            canonicalName,
            category,
            value => value * multiplierToBase,
            value => value / multiplierToBase);

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