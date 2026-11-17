using AlgorithmsLab3.Domain.Expressions;

namespace AlgorithmsLab3.Tests;

public sealed class PostfixEvaluatorTests
{
    [Theory]
    [InlineData("7 8 + 3 2 + /", 3)]
    [InlineData("5 2 -", 3)]
    [InlineData("6 3 /", 2)]
    [InlineData("6 3 :", 2)]
    [InlineData("2 3 ^", 8)]
    [InlineData("1,5 2 *", 3)]
    [InlineData("4 sqrt", 2)]
    [InlineData("0 cos", 1)]
    [InlineData("0 sin", 0)]
    [InlineData("2 ~ abs", 2)]
    [InlineData("3 ~", -3)]
    [InlineData("2 3 * 4 +", 10)]
    public void Evaluate_ComputesExpectedValues(string postfix, double expected)
    {
        var result = PostfixEvaluator.Evaluate(postfix);

        Assert.Equal(expected, result.Value, 10);
    }

    [Fact]
    public void Evaluate_ReturnsMetadataAndTrace()
    {
        var result = PostfixEvaluator.Evaluate("7 8 + 3 2 + /");

        Assert.Equal(7, result.TokenCount);
        Assert.Equal(3, result.OperationCount);
        Assert.Equal(3, result.MaxStackDepth);
        Assert.Equal("3", result.FormattedValue);
        Assert.NotEmpty(result.Trace);
        Assert.Equal("результат = 3", result.Trace[^1]);
    }

    [Fact]
    public void Evaluate_SupportsFunctions()
    {
        Assert.Equal(1.0, PostfixEvaluator.Evaluate("2.718281828459045 ln").Value, 6);
        Assert.Equal(1.0, PostfixEvaluator.Evaluate("0 cos").Value, 10);
        Assert.Equal(0.0, PostfixEvaluator.Evaluate("0 sin").Value, 10);
    }

    [Fact]
    public void Evaluate_DivisionByZero_Throws()
    {
        var exception = Assert.Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("1 0 /"));
        Assert.Contains("ноль", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Evaluate_MissingOperand_Throws()
    {
        Assert.Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("1 +"));
    }

    [Fact]
    public void Evaluate_ExtraOperands_Throws()
    {
        var exception = Assert.Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("1 2 3 +"));
        Assert.Contains("лишние", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Evaluate_EmptyExpression_Throws()
    {
        Assert.Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("   "));
    }

    [Fact]
    public void Evaluate_SqrtOfNegative_Throws()
    {
        Assert.Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("4 neg sqrt"));
    }

    [Fact]
    public void Evaluate_UnknownOperation_Throws()
    {
        Assert.Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("1 2 #"));
    }
}

public sealed class InfixToPostfixConverterTests
{
    [Theory]
    [InlineData("2 + 2 * (3 - 1) ^ 2 + sqrt(16)", "2 2 3 1 - 2 ^ * + 16 sqrt +", 14)]
    [InlineData("2 ^ 3 ^ 2", "2 3 2 ^ ^", 512)]
    [InlineData("-3 + 5", "3 neg 5 +", 2)]
    [InlineData("2 * -3", "2 3 neg *", -6)]
    [InlineData("(1 + 2) * 3", "1 2 + 3 *", 9)]
    [InlineData("1,5 + 1,5", "1.5 1.5 +", 3)]
    [InlineData("sin(0) + cos(0)", "0 sin 0 cos +", 1)]
    [InlineData("10 / 4", "10 4 /", 2.5)]
    public void Convert_ProducesPostfixAndValue(string infix, string expectedPostfix, double expectedValue)
    {
        var conversion = InfixToPostfixConverter.Convert(infix);
        var evaluation = PostfixEvaluator.Evaluate(conversion.Tokens);

        Assert.Equal(expectedPostfix, conversion.Postfix);
        Assert.Equal(expectedValue, evaluation.Value, 10);
        Assert.NotEmpty(conversion.Steps);
    }

    [Fact]
    public void Convert_RespectsLeftAssociativity()
    {
        var conversion = InfixToPostfixConverter.Convert("10 - 4 - 2");

        Assert.Equal("10 4 - 2 -", conversion.Postfix);
        Assert.Equal(4, PostfixEvaluator.Evaluate(conversion.Tokens).Value, 10);
    }

    [Fact]
    public void Convert_UnmatchedOpenParenthesis_Throws()
    {
        Assert.Throws<ExpressionException>(() => InfixToPostfixConverter.Convert("(1 + 2"));
    }

    [Fact]
    public void Convert_UnmatchedCloseParenthesis_Throws()
    {
        Assert.Throws<ExpressionException>(() => InfixToPostfixConverter.Convert("1 + 2)"));
    }

    [Fact]
    public void Convert_EmptyExpression_Throws()
    {
        Assert.Throws<ExpressionException>(() => InfixToPostfixConverter.Convert(" "));
    }

    [Fact]
    public void Convert_UnknownFunction_Throws()
    {
        Assert.Throws<ExpressionException>(() => InfixToPostfixConverter.Convert("exp(1)"));
    }
}

public sealed class ExpressionTokenizerTests
{
    [Fact]
    public void Tokenize_ReadsNumbersOperatorsAndParentheses()
    {
        var tokens = ExpressionTokenizer.Tokenize("1,5 + (2 - 3)");

        Assert.Equal(7, tokens.Count);
        Assert.Equal(1.5, tokens[0].Number, 10);
        Assert.Equal(TokenKind.Operator, tokens[1].Kind);
        Assert.Equal(TokenKind.LeftParenthesis, tokens[2].Kind);
        Assert.Equal(TokenKind.RightParenthesis, tokens[6].Kind);
    }

    [Fact]
    public void Tokenize_ReadsConstantsAndFunctions()
    {
        var tokens = ExpressionTokenizer.Tokenize("pi e sqrt(9)");

        Assert.Equal(Math.PI, tokens[0].Number, 10);
        Assert.Equal(Math.E, tokens[1].Number, 10);
        Assert.Equal(TokenKind.Function, tokens[2].Kind);
        Assert.Equal("sqrt", tokens[2].Text);
    }

    [Fact]
    public void Tokenize_ReadsExponentNotation()
    {
        var tokens = ExpressionTokenizer.Tokenize("2,5e2");

        Assert.Single(tokens);
        Assert.Equal(250, tokens[0].Number, 10);
    }

    [Fact]
    public void Tokenize_EmptyText_ReturnsNoTokens()
    {
        Assert.Empty(ExpressionTokenizer.Tokenize(null));
        Assert.Empty(ExpressionTokenizer.Tokenize("   "));
    }

    [Fact]
    public void Tokenize_InvalidCharacter_Throws()
    {
        Assert.Throws<ExpressionException>(() => ExpressionTokenizer.Tokenize("1 $ 2"));
    }
}
