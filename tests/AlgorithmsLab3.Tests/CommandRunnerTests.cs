using AlgorithmsLab3.Domain.Execution;
using AlgorithmsLab3.Domain.Queues;

namespace AlgorithmsLab3.Tests;

public sealed class StackCommandRunnerTests
{
    [Fact]
    public void Run_SampleFromTask_ProducesExpectedState()
    {
        var result = StackCommandRunner.Run("3 4 1,56 1,7 1,cat 2 5 4");

        Assert.Equal(8, result.Operations);
        Assert.Equal(1, result.Errors);
        Assert.Equal(3, result.PushCount);
        Assert.Equal(1, result.PopCount);
        Assert.Equal(0, result.TopCount);
        Assert.Equal(1, result.PrintCount);
        Assert.Equal(2, result.IsEmptyCount);
        Assert.Equal(2, result.FinalCount);
        Assert.Equal("[7, 56]", result.FinalState);
    }

    [Fact]
    public void Run_LogsErrorForTopOnEmptyStack()
    {
        var result = StackCommandRunner.Run("3");

        Assert.Equal(1, result.Errors);
        var entry = Assert.Single(result.Log);
        Assert.True(entry.IsError);
        Assert.Equal("3", entry.Command);
        Assert.Equal("Top(): стек пуст", entry.Result);
    }

    [Fact]
    public void Run_LogsPopAndPrint()
    {
        var result = StackCommandRunner.Run("1,a 1,b 2 5");

        Assert.Equal(0, result.Errors);
        Assert.Equal("Pop(): b", result.Log[2].Result);
        Assert.Equal("Print(): [a]", result.Log[3].Result);
        Assert.Equal("[a]", result.FinalState);
    }

    [Fact]
    public void Run_PushWithoutValue_IsReportedAsError()
    {
        var result = StackCommandRunner.Run("1 1,");

        Assert.Equal(2, result.Errors);
        Assert.All(result.Log, entry => Assert.True(entry.IsError));
    }

    [Fact]
    public void Run_UnknownCommand_IsReportedAsError()
    {
        var result = StackCommandRunner.Run("9");

        Assert.Equal(1, result.Errors);
        Assert.Contains("Неизвестная команда", result.Log[0].Result);
    }

    [Fact]
    public void Run_EmptyStackPop_IsReportedOnce()
    {
        var result = StackCommandRunner.Run("2");

        Assert.Equal(1, result.Errors);
        Assert.Equal("Pop(): стек пуст", result.Log[0].Result);
    }

    [Fact]
    public void Run_EmptyInput_ReturnsEmptyResult()
    {
        var result = StackCommandRunner.Run(null);

        Assert.Equal(0, result.Operations);
        Assert.Empty(result.Log);
        Assert.Equal("[]", result.FinalState);
    }

    [Fact]
    public void Run_IsDeterministic()
    {
        const string input = "1,1 1,2 3 2 4 5";

        var first = StackCommandRunner.Run(input);
        var second = StackCommandRunner.Run(input);

        Assert.Equal(first.Signature, second.Signature);
        Assert.Equal(first.FinalState, second.FinalState);
    }
}

public sealed class CommandInputTests
{
    [Theory]
    [InlineData("1, 56 2", new[] { "1,56", "2" })]
    [InlineData("1,56\n1,7\r\n2 5", new[] { "1,56", "1,7", "2", "5" })]
    [InlineData("  3   4  ", new[] { "3", "4" })]
    [InlineData("", new string[0])]
    public void Tokenize_SplitsCommands(string input, string[] expected)
    {
        Assert.Equal(expected, CommandInput.Tokenize(input));
    }

    [Fact]
    public void Tokenize_NullInput_ReturnsEmpty()
    {
        Assert.Empty(CommandInput.Tokenize(null));
    }
}

public sealed class QueueCommandRunnerTests
{
    private const string Sample = "3 4 1,56 1,7 1,cat 2 5 4";

    [Fact]
    public void Run_LinkedQueue_ProducesExpectedState()
    {
        var result = QueueCommandRunner.Run(new LinkedQueue<string>(), Sample);

        Assert.Equal(8, result.Operations);
        Assert.Equal(1, result.Errors);
        Assert.Equal(3, result.EnqueueCount);
        Assert.Equal(1, result.DequeueCount);
        Assert.Equal(1, result.PrintCount);
        Assert.Equal(2, result.FinalCount);
        Assert.Equal("[7, cat]", result.FinalState);
    }

    [Fact]
    public void Run_BothImplementations_Match()
    {
        var comparison = QueueComparisonRunner.Run(Sample);

        Assert.True(comparison.ResultsMatch);
        Assert.Equal("[7, cat]", comparison.Linked.FinalState);
        Assert.Equal("[7, cat]", comparison.Standard.FinalState);
        Assert.Equal(8, comparison.Entries.Count);
        Assert.All(comparison.Entries, entry => Assert.True(entry.Match));
        Assert.Equal(1, comparison.Linked.Errors);
        Assert.Equal(1, comparison.Standard.Errors);
    }

    [Fact]
    public void Run_BothImplementations_MatchOnLargerScenario()
    {
        var commands = "1,1 1,2 1,3 2 3 4 1,4 2 5 2 5";

        var comparison = QueueComparisonRunner.Run(commands);

        Assert.True(comparison.ResultsMatch);
        Assert.Equal(comparison.Linked.Signature, comparison.Standard.Signature);
        Assert.Equal(11, comparison.Entries.Count);
    }

    [Fact]
    public void Run_EmptyQueueDequeueAndPeek_CountErrorsOnce()
    {
        var comparison = QueueComparisonRunner.Run("2 3");

        Assert.Equal(2, comparison.Linked.Errors);
        Assert.Equal(2, comparison.Standard.Errors);
        Assert.True(comparison.ResultsMatch);
    }

    [Fact]
    public void Run_StandardAdapter_UsesDisplayName()
    {
        var result = QueueCommandRunner.Run(new StandardQueueAdapter<string>(), "1,x");

        Assert.Equal("Стандартный Queue<T>", result.Implementation);
    }
}
