using System.Globalization;
using AlgorithmsLab3.Domain.Stacks;

namespace AlgorithmsLab3.Domain.Expressions;

public sealed record PostfixConversionResult(
    IReadOnlyList<ExpressionToken> Tokens,
    string Postfix,
    IReadOnlyList<string> Steps);

/// <summary>Перевод инфиксной записи в постфиксную алгоритмом Дейкстры (сортировочная станция).</summary>
public static class InfixToPostfixConverter
{
    public static PostfixConversionResult Convert(string? infix)
    {
        var tokens = ExpressionTokenizer.Tokenize(infix);
        if (tokens.Count == 0)
        {
            throw new ExpressionException("Выражение пусто.");
        }

        var output = new List<ExpressionToken>();
        var operators = new LinkedStack<ExpressionToken>();
        var steps = new List<string>();
        ExpressionToken? previous = null;

        foreach (var sourceToken in tokens)
        {
            var token = sourceToken;
            switch (token.Kind)
            {
                case TokenKind.Number:
                    output.Add(token);
                    steps.Add($"число {token.Text} → выход");
                    break;

                case TokenKind.Function:
                    operators.Push(token);
                    steps.Add($"функция {token.Text} → стек операторов");
                    break;

                case TokenKind.LeftParenthesis:
                    operators.Push(token);
                    steps.Add("( → стек операторов");
                    break;

                case TokenKind.RightParenthesis:
                    var foundLeft = false;
                    while (!operators.IsEmpty)
                    {
                        var top = operators.Pop();
                        if (top.Kind == TokenKind.LeftParenthesis)
                        {
                            foundLeft = true;
                            break;
                        }

                        output.Add(top);
                    }

                    if (!foundLeft)
                    {
                        throw new ExpressionException("Несогласованные скобки: лишняя закрывающая скобка.");
                    }

                    if (!operators.IsEmpty && operators.Top().IsFunction)
                    {
                        output.Add(operators.Pop());
                    }

                    steps.Add(") → выталкиваем операторы до (");
                    break;

                case TokenKind.Operator:
                    var text = token.Text;
                    var unary = (text == "-" || text == "+") && IsUnaryPosition(previous);
                    if (unary)
                    {
                        if (text == "+")
                        {
                            steps.Add("унарный + пропущен");
                            previous = token;
                            continue;
                        }

                        token = ExpressionToken.OperatorToken("neg");
                        text = "neg";
                    }

                    while (!operators.IsEmpty)
                    {
                        var top = operators.Top();
                        if (top.Kind == TokenKind.LeftParenthesis)
                        {
                            break;
                        }

                        var topPrecedence = Precedence(top);
                        var currentPrecedence = Precedence(token);
                        var shouldPop = topPrecedence > currentPrecedence
                            || (topPrecedence == currentPrecedence && !IsRightAssociative(token));
                        if (!shouldPop)
                        {
                            break;
                        }

                        output.Add(operators.Pop());
                    }

                    operators.Push(token);
                    steps.Add($"оператор {text} → стек операторов");
                    break;

                default:
                    throw new ExpressionException($"Неизвестный токен «{token.Text}».");
            }

            previous = token;
        }

        while (!operators.IsEmpty)
        {
            var top = operators.Pop();
            if (top.Kind is TokenKind.LeftParenthesis or TokenKind.RightParenthesis)
            {
                throw new ExpressionException("Несогласованные скобки: не закрыта открывающая скобка.");
            }

            output.Add(top);
        }

        var postfix = string.Join(' ', output.Select(token => token.Text));
        return new PostfixConversionResult(output, postfix, steps);
    }

    private static bool IsUnaryPosition(ExpressionToken? previous) =>
        previous is null
        || previous.Kind is TokenKind.Operator or TokenKind.Function or TokenKind.LeftParenthesis;

    private static int Precedence(ExpressionToken token) => token.Text switch
    {
        "neg" => 4,
        "^" => 5,
        "*" or "/" or ":" => 3,
        "+" or "-" => 2,
        _ when token.IsFunction => 6,
        _ => 0,
    };

    private static bool IsRightAssociative(ExpressionToken token) => token.Text is "^" or "neg";
}
