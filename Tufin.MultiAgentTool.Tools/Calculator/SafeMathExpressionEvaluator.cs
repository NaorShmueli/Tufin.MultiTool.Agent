using System.Globalization;

namespace Tufin.MultiAgentTool.Tools.Calculator;

/// <summary>
/// Evaluates a deliberately limited mathematical grammar.
///
/// Supported:
/// +, -, *, /, %, ^, unary +/-, parentheses and decimal numbers.
///
/// This class does not execute code and does not use dynamic evaluation.
/// </summary>
public sealed class SafeMathExpressionEvaluator
{
    private const int MaxExpressionLength = 256;
    private const int MaxParenthesisDepth = 32;

    public double Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new MathExpressionException(
                "Expression cannot be empty.");
        }

        if (expression.Length > MaxExpressionLength)
        {
            throw new MathExpressionException(
                $"Expression cannot exceed {MaxExpressionLength} characters.");
        }

        var parser = new Parser(expression);

        var result = parser.Parse();

        EnsureFinite(result);

        return result;
    }

    private static void EnsureFinite(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new MathExpressionException(
                "Expression produced a non-finite result.");
        }
    }

    private sealed class Parser
    {
        private readonly string _text;
        private int _position;
        private int _depth;

        public Parser(string text)
        {
            _text = text;
        }

        public double Parse()
        {
            var result = ParseAdditive();

            SkipWhiteSpace();

            if (!IsAtEnd)
            {
                throw Error(
                    $"Unexpected character '{CurrentCharacter}'.");
            }

            return result;
        }

        // expression = multiplication (("+" | "-") multiplication)*
        private double ParseAdditive()
        {
            var left = ParseMultiplicative();

            while (true)
            {
                SkipWhiteSpace();

                if (Match('+'))
                {
                    left = CheckedOperation(
                        left + ParseMultiplicative());
                    continue;
                }

                if (Match('-'))
                {
                    left = CheckedOperation(
                        left - ParseMultiplicative());
                    continue;
                }

                return left;
            }
        }

        // multiplication = power (("*" | "/" | "%") power)*
        private double ParseMultiplicative()
        {
            var left = ParsePower();

            while (true)
            {
                SkipWhiteSpace();

                if (Match('*'))
                {
                    left = CheckedOperation(
                        left * ParsePower());
                    continue;
                }

                if (Match('/'))
                {
                    var right = ParsePower();

                    if (right == 0)
                    {
                        throw Error("Division by zero is not allowed.");
                    }

                    left = CheckedOperation(left / right);
                    continue;
                }

                if (Match('%'))
                {
                    var right = ParsePower();

                    if (right == 0)
                    {
                        throw Error("Modulo by zero is not allowed.");
                    }

                    left = CheckedOperation(left % right);
                    continue;
                }

                return left;
            }
        }

        // power = unary ("^" power)?
        // Recursive parsing makes exponentiation right-associative:
        // 2^3^2 = 2^(3^2)
        private double ParsePower()
        {
            var left = ParseUnary();

            SkipWhiteSpace();

            if (!Match('^'))
            {
                return left;
            }

            var right = ParsePower();

            return CheckedOperation(
                Math.Pow(left, right));
        }

        // unary = ("+" | "-") unary | primary
        private double ParseUnary()
        {
            SkipWhiteSpace();

            if (Match('+'))
            {
                return ParseUnary();
            }

            if (Match('-'))
            {
                return CheckedOperation(
                    -ParseUnary());
            }

            return ParsePrimary();
        }

        // primary = number | "(" expression ")"
        private double ParsePrimary()
        {
            SkipWhiteSpace();

            if (Match('('))
            {
                _depth++;

                if (_depth > MaxParenthesisDepth)
                {
                    throw Error(
                        $"Maximum parenthesis depth of " +
                        $"{MaxParenthesisDepth} was exceeded.");
                }

                var result = ParseAdditive();

                SkipWhiteSpace();

                if (!Match(')'))
                {
                    throw Error("Missing closing parenthesis.");
                }

                _depth--;

                return result;
            }

            return ParseNumber();
        }

        private double ParseNumber()
        {
            SkipWhiteSpace();

            var start = _position;
            var hasDigits = false;

            while (!IsAtEnd &&
                   char.IsDigit(CurrentCharacter))
            {
                hasDigits = true;
                _position++;
            }

            if (!IsAtEnd && CurrentCharacter == '.')
            {
                _position++;

                while (!IsAtEnd &&
                       char.IsDigit(CurrentCharacter))
                {
                    hasDigits = true;
                    _position++;
                }
            }

            // Support scientific notation, for example 1.5e3.
            if (!IsAtEnd &&
                CurrentCharacter is 'e' or 'E')
            {
                _position++;

                if (!IsAtEnd &&
                    CurrentCharacter is '+' or '-')
                {
                    _position++;
                }

                var exponentStart = _position;

                while (!IsAtEnd &&
                       char.IsDigit(CurrentCharacter))
                {
                    _position++;
                }

                if (exponentStart == _position)
                {
                    throw Error("Invalid scientific notation.");
                }
            }

            if (!hasDigits)
            {
                throw Error("A number was expected.");
            }

            var numberText = _text[start.._position];

            if (!double.TryParse(
                    numberText,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var value))
            {
                throw Error(
                    $"'{numberText}' is not a valid number.");
            }

            return CheckedOperation(value);
        }

        private double CheckedOperation(double value)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw Error(
                    "Expression produced a non-finite value.");
            }

            return value;
        }

        private bool Match(char expected)
        {
            if (IsAtEnd || CurrentCharacter != expected)
            {
                return false;
            }

            _position++;
            return true;
        }

        private void SkipWhiteSpace()
        {
            while (!IsAtEnd &&
                   char.IsWhiteSpace(CurrentCharacter))
            {
                _position++;
            }
        }

        private MathExpressionException Error(string message)
        {
            return new MathExpressionException(
                $"{message} Position: {_position}.");
        }

        private bool IsAtEnd => _position >= _text.Length;

        private char CurrentCharacter => _text[_position];
    }
}