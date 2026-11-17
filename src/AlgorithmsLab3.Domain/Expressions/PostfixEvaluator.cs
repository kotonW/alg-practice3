using System.Globalization;
using AlgorithmsLab3.Domain.Stacks;

namespace AlgorithmsLab3.Domain.Expressions;

public sealed record PostfixEvaluationResult(
    double Value,
    string FormattedValue,
    IReadOnlyList<string> Trace,
    int MaxStackDepth,
    int TokenCount,
    int OperationCount);

/// <summary>
/// Вычисление выражения в постфиксной записи с помощью собственного стека.
/// Время — O(n), дополнительная память — O(n) в худшем случае.
/// </summary>
public static class PostfixEvaluator
{
    public static PostfixEvaluationResult Evaluate(string? postfix)
    {
        var tokens = ExpressionTokenizer.Tokenize(postfix);
        return Evaluate(tokens);
    }

    public static PostfixEvaluationResult Evaluate(IReadOnlyList<ExpressionToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        if (tokens.Count == 0)
        {
            throw new ExpressionException("Постфиксное выражение пусто.");
        }

        var stack = new LinkedStack<double>();
        var trace = new List<string>();
        var maxDepth = 0;
        var operations = 0;

        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case TokenKind.Number:
                    stack.Push(token.Number);
                    maxDepth = Math.Max(maxDepth, stack.Count);
                    break;

                case TokenKind.Function:
                    ApplyFunction(token.Text, stack, trace);
                    operations++;
                    break;

                case TokenKind.Operator:
                    ApplyOperator(token.Text, stack, trace);
                    operations++;
                    break;

                default:
                    throw new ExpressionException($"В постфиксной записи недопустим токен «{token.Text}».");
            }
        }

        if (stack.Count != 1)
        {
            throw new ExpressionException(stack.Count == 0
                ? "В выражении недостаточно операндов."
                : $"В стеке осталось {stack.Count} значений — в выражении лишние операнды.");
        }

        var value = stack.Pop();
        var formatted = Format(value);
        trace.Add($"результат = {formatted}");
        return new PostfixEvaluationResult(value, formatted, trace, maxDepth, tokens.Count, operations);
    }

    public static string Format(double value) =>
        value.ToString("G15", CultureInfo.InvariantCulture);

    private static void ApplyFunction(string name, LinkedStack<double> stack, List<string> trace)
    {
        if (!stack.TryPop(out var argument))
        {
            throw new ExpressionException($"Для функции {name} не хватает операнда.");
        }

        var result = name switch
        {
            "ln" => argument > 0
                ? Math.Log(argument)
                : throw new ExpressionException("ln определён только для положительных чисел."),
            "cos" => Math.Cos(argument),
            "sin" => Math.Sin(argument),
            "tg" or "tan" => Math.Tan(argument),
            "sqrt" => argument >= 0
                ? Math.Sqrt(argument)
                : throw new ExpressionException("sqrt определён только для неотрицательных чисел."),
            "abs" => Math.Abs(argument),
            _ => throw new ExpressionException($"Неизвестная функция «{name}»."),
        };

        EnsureFinite(result, name);
        stack.Push(result);
        trace.Add($"{name}({Format(argument)}) = {Format(result)}");
    }

    private static void ApplyOperator(string op, LinkedStack<double> stack, List<string> trace)
    {
        if (op == "neg")
        {
            if (!stack.TryPop(out var unary))
            {
                throw new ExpressionException("Для унарного минуса не хватает операнда.");
            }

            var negated = -unary;
            stack.Push(negated);
            trace.Add($"neg {Format(unary)} = {Format(negated)}");
            return;
        }

        if (!stack.TryPop(out var right) || !stack.TryPop(out var left))
        {
            throw new ExpressionException($"Для операции «{op}» не хватает операндов.");
        }

        var result = op switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            "/" or ":" => right == 0
                ? throw new ExpressionException("Деление на ноль.")
                : left / right,
            "^" => Math.Pow(left, right),
            _ => throw new ExpressionException($"Неизвестная операция «{op}»."),
        };

        EnsureFinite(result, op);
        stack.Push(result);
        trace.Add($"{Format(left)} {op} {Format(right)} = {Format(result)}");
    }

    private static void EnsureFinite(double value, string operation)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ExpressionException($"Операция «{operation}» дала недопустимый результат.");
        }
    }
}
