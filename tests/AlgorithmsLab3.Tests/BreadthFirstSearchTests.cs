using AlgorithmsLab3.Domain.Applications;

namespace AlgorithmsLab3.Tests;

public sealed class BreadthFirstSearchTests
{
    private static Graph Diamond() => Graph.Parse("1 2\n1 3\n2 4\n3 4", directed: false);

    [Fact]
    public void Run_VisitsVerticesLevelByLevel()
    {
        var result = BreadthFirstSearch.Run(Diamond(), 1);

        Assert.Equal([1, 2, 3, 4], result.VisitOrder);
        Assert.Equal([0, 1, 1, 2], result.Distances);
    }

    [Fact]
    public void Run_CountsQueueOperations()
    {
        var result = BreadthFirstSearch.Run(Diamond(), 1);

        Assert.Equal(4, result.EnqueueCount);
        Assert.Equal(4, result.DequeueCount);
        Assert.Equal(5, result.SkippedCount);
        Assert.Equal(2, result.MaxQueueDepth);
        Assert.Equal(4, result.ReachableCount);
    }

    [Fact]
    public void Run_LevelsGroupVerticesByDistance()
    {
        var levels = BreadthFirstSearch.Run(Diamond(), 1).Levels();

        Assert.Equal(3, levels.Count);
        Assert.Equal(0, levels[0].Level);
        Assert.Equal([1], levels[0].Vertices);
        Assert.Equal(1, levels[1].Level);
        Assert.Equal([2, 3], levels[1].Vertices);
        Assert.Equal(2, levels[2].Level);
        Assert.Equal([4], levels[2].Vertices);
    }

    [Fact]
    public void Run_DoesNotEnqueueVertexTwice()
    {
        var result = BreadthFirstSearch.Run(Diamond(), 1);

        Assert.Equal(result.VisitOrder.Count, result.VisitOrder.Distinct().Count());
        Assert.Equal(result.VisitOrder.Count, result.EnqueueCount);
        Assert.Single(result.Trace, line => line.Contains("enqueue 4", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_LeavesUnreachableVerticesUnvisited()
    {
        var graph = Graph.Parse("1 2\n3", directed: false);

        var result = BreadthFirstSearch.Run(graph, 1);

        Assert.Equal([1, 2], result.VisitOrder);
        Assert.Equal([0, 1], result.Distances);
        Assert.Contains(result.Trace, line => line.Contains("достижимо 2 из 3", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_DirectedGraphFollowsEdgeDirection()
    {
        var graph = Graph.Parse("1 2\n2 3", directed: true);

        Assert.Equal([1, 2, 3], BreadthFirstSearch.Run(graph, 1).VisitOrder);
        Assert.Equal([3], BreadthFirstSearch.Run(graph, 3).VisitOrder);    }

    [Fact]
    public void Run_UnknownStartVertex_Throws()
    {
        Assert.Throws<ArgumentException>(() => BreadthFirstSearch.Run(Diamond(), 99));
    }

    [Fact]
    public void Run_TruncatesLongTrace()
    {
        var graph = Graph.CreateRandom(30, 40, directed: false, seed: 7);

        var result = BreadthFirstSearch.Run(graph, 1, maxTraceEntries: 10);

        Assert.True(result.Trace.Count <= 11);
        Assert.Contains("журнал обрезан", result.Trace[^1]);
    }
}
