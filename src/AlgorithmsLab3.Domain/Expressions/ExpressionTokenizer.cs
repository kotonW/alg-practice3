using System.Globalization;

namespace AlgorithmsLab3.Domain.Expressions;

public enum TokenKind
{
    Number,
    Operator,
    Function,
    LeftParenthesis,
    RightParenthesis,
}

public sealed record ExpressionToken(string Text, TokenKind Kind, double Number = 0)
{
    public bool IsOperator => Kind == TokenKind.Operator;

    public bool IsFunction => Kind == TokenKind.Function;

    public static ExpressionToken NumberToken(double value) =>
        new(value.ToString("G17", CultureInfo.InvariantCulture), TokenKind.Number, value);

    public static ExpressionToken OperatorToken(string text) => new(text, TokenKind.Operator);

    public static ExpressionToken FunctionToken(string text) => new(text.ToLowerInvariant(), TokenKind.Function);
}

public sealed class ExpressionException : Exception
{
    public ExpressionException(string message)
        : base(message)
    {
    }
}

/// <summary>Лексер арифметических выражений: числа, операции, функции, скобки.</summary>
public static class ExpressionTokenizer
{
    private static readonly HashSet<string> Functions = new(StringComparer.OrdinalIgnoreCase)
    {
        "ln", "cos", "sin", "sqrt", "abs", "tg", "tan",
    };

    public static IReadOnlyList<ExpressionToken> Tokenize(string? text)
    {
        var tokens = new List<ExpressionToken>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return tokens;
        }

        var index = 0;
        while (index < text.Length)
        {
            var current = text[index];

            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            if (char.IsDigit(current) || current == '.' || (current == ',' && index + 1 < text.Length && char.IsDigit(text[index + 1])))
            {
                tokens.Add(ReadNumber(text, ref index));
                continue;
            }

            if (char.IsLetter(current))
            {
                var start = index;
                while (index < text.Length && char.IsLetter(text[index]))
                {
                    index++;
                }

                var name = text[start..index].ToLowerInvariant();
                if (name is "pi" or "π")
                {
                    tokens.Add(ExpressionToken.NumberToken(Math.PI));
                }
                else if (name == "e")
                {
                    tokens.Add(ExpressionToken.NumberToken(Math.E));
                }
                else if (Functions.Contains(name))
                {
                    tokens.Add(ExpressionToken.FunctionToken(name));
                }
                else
                {
                    throw new ExpressionException($"Неизвестная функция «{name}».");
                }

                continue;
            }

            switch (current)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case ':':
                case '^':
                case '×':
                case '÷':
                    tokens.Add(ExpressionToken.OperatorToken(current switch
                    {
                        '×' => "*",
                        '÷' => ":",
                        _ => current.ToString(),
                    }));
                    index++;
                    break;
                case '~':
                    tokens.Add(ExpressionToken.OperatorToken("neg"));
                    index++;
                    break;
                case '(':
                    tokens.Add(new ExpressionToken("(", TokenKind.LeftParenthesis));
                    index++;
                    break;
                case ')':
                    tokens.Add(new ExpressionToken(")", TokenKind.RightParenthesis));
                    index++;
                    break;
                default:
                    throw new ExpressionException($"Недопустимый символ «{current}» в позиции {index + 1}.");
            }
        }

        return tokens;
    }

    private static ExpressionToken ReadNumber(string text, ref int index)
    {
        var start = index;
        var hasDot = false;

        while (index < text.Length)
        {
            var current = text[index];
            if (char.IsDigit(current))
            {
                index++;
                continue;
            }

            if ((current == '.' || current == ',') && !hasDot)
            {
                hasDot = true;
                index++;
                continue;
            }

            break;
        }

        // Экспоненциальная запись: 1e-6, 2.5E+3.
        if (index < text.Length && (text[index] == 'e' || text[index] == 'E'))
        {
            var exponentStart = index;
            index++;
            if (index < text.Length && (text[index] == '+' || text[index] == '-'))
            {
                index++;
            }

            var digitsStart = index;
            while (index < text.Length && char.IsDigit(text[index]))
            {
                index++;
            }

            if (digitsStart == index)
            {
                index = exponentStart;
            }
        }

        var raw = text[start..index].Replace(',', '.');
        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new ExpressionException($"Не удалось прочитать число «{raw}».");
        }

        return ExpressionToken.NumberToken(value);
    }
}
