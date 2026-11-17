using AlgorithmsLab3.Domain.Benchmarks;

namespace AlgorithmsLab3.Tests;

public sealed class BenchmarkTests
{
    [Fact]
    public void RunStackBenchmarks_ReturnsRowsWithConsistentData()
    {
        var rows = BenchmarkRunner.RunStackBenchmarks();

        Assert.Equal(6, rows.Count);
        Assert.Equal(6, rows.Select(row => row.Name).Distinct().Count());
        Assert.All(rows, row =>
        {
            Assert.True(row.Operations > 0);
            Assert.True(row.ReadMilliseconds >= 0);
            Assert.True(row.ExecuteMilliseconds >= 0);
            Assert.True(row.TotalMilliseconds >= row.ExecuteMilliseconds);
            Assert.True(row.MicrosecondsPerOperation >= 0);
        });

        var sample = Assert.Single(rows, row => row.Name == "Пример из задания");
        Assert.Equal(8, sample.Operations);
        Assert.Equal(1, sample.Errors);

        var largePush = Assert.Single(rows, row => row.Name == "50 000 push");
        Assert.Equal(50_000, largePush.Operations);
        Assert.Equal(50_000, largePush.FinalCount);
        Assert.Equal(0, largePush.Errors);

        var pushPop = Assert.Single(rows, row => row.Name == "1 000 push/pop");
        Assert.Equal(1_000, pushPop.Operations);
        Assert.Equal(0, pushPop.FinalCount);
        Assert.Equal(500, pushPop.PopCount);
    }

    [Fact]
    public void RunQueueBenchmarks_ComparesBothImplementations()
    {
        var rows = BenchmarkRunner.RunQueueBenchmarks();

        Assert.Equal(6, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.True(row.ResultsMatch, $"Результаты не совпали в наборе «{row.Name}»");
            Assert.True(row.Operations > 0);
            Assert.True(row.LinkedTotalMilliseconds >= 0);
            Assert.True(row.StandardTotalMilliseconds >= 0);
            Assert.True(row.Ratio >= 0);
        });

        var large = Assert.Single(rows, row => row.Name == "20 000 вставок");
        Assert.Equal(20_000, large.FinalCount);
        Assert.Equal(0, large.Errors);

        var alternating = Assert.Single(rows, row => row.Name == "20 000: вставка/удаление");
        Assert.Equal(0, alternating.FinalCount);

        Assert.Contains(rows, row => row.Group == "Разная длина");
        Assert.Contains(rows, row => row.Group == "Одинаковая длина");
        Assert.Contains(rows, row => row.Group == "Пример");
    }
}
