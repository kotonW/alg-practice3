using AlgorithmsLab3.Domain.Applications;

namespace AlgorithmsLab3.Tests;

public sealed class FloydCycleTests
{
    [Fact]
    public void Detect_FindsCycleAndItsParameters()
    {
        var list = new CycleLinkedList();
        list.Build([3, 1, 4, 1, 5, 9, 2, 6], 3);

        var result = list.Detect();

        Assert.True(result.HasCycle);
        Assert.Equal(3, result.CycleStartIndex);
        Assert.Equal(1, result.CycleStartValue);
        Assert.Equal(5, result.CycleLength);
        Assert.Equal(5, result.MeetingIndex);
        Assert.Equal(9, result.MeetingValue);
        Assert.Equal(8, result.NodeCount);
        Assert.NotEmpty(result.Trace);
        Assert.True(result.Trace[^1].Met);
        Assert.Contains("Цикл есть", result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_ReportsNoCycleWhenListEnds()
    {
        var list = new CycleLinkedList();
        list.Build([1, 2, 3, 4, 5], null);

        var result = list.Detect();

        Assert.False(result.HasCycle);
        Assert.Null(result.CycleStartIndex);
        Assert.Null(result.CycleLength);
        Assert.Equal(5, result.NodeCount);
        Assert.All(result.Trace, step => Assert.False(step.Met));
        Assert.Contains("Цикла нет", result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_HandlesSelfLoop()
    {
        var list = new CycleLinkedList();
        list.Build([7], 0);

        var result = list.Detect();

        Assert.True(result.HasCycle);
        Assert.Equal(0, result.CycleStartIndex);
        Assert.Equal(1, result.CycleLength);
        Assert.Equal(1, result.Iterations);
        Assert.Equal(7, result.MeetingValue);
    }

    [Fact]
    public void Detect_HandlesCycleAtHead()
    {
        var list = new CycleLinkedList();
        list.Build([1, 2, 3], 0);

        var result = list.Detect();

        Assert.True(result.HasCycle);
        Assert.Equal(0, result.CycleStartIndex);
        Assert.Equal(3, result.CycleLength);
    }

    [Fact]
    public void Detect_EmptyList_ReturnsNoCycle()
    {
        var list = new CycleLinkedList();
        list.Build([], 0);

        var result = list.Detect();

        Assert.False(result.HasCycle);
        Assert.Equal(0, result.NodeCount);
        Assert.Empty(result.Trace);
    }
}

public sealed class GraphTests
{
    [Fact]
    public void Parse_UndirectedGraph_AddsBothDirections()
    {
        var graph = Graph.Parse("1 2\n2 3", directed: false);

        Assert.Equal([1, 2, 3], graph.Vertices);
        Assert.Equal(2, graph.EdgeCount);
        Assert.Equal([2], graph.Neighbors(1));
        Assert.Equal([1, 3], graph.Neighbors(2));
    }

    [Fact]
    public void Parse_DirectedGraph_KeepsDirection()
    {
        var graph = Graph.Parse("1 2\n2 3", directed: true);

        Assert.True(graph.IsDirected);
        Assert.Equal([2], graph.Neighbors(1));
        Assert.Empty(graph.Neighbors(3));
    }

    [Fact]
    public void Parse_IsolatedVertexAndCommentsAreSupported()
    {
        var graph = Graph.Parse("# comment\n1\n2 3", directed: false);

        Assert.Equal([1, 2, 3], graph.Vertices);
        Assert.Empty(graph.Neighbors(1));
        Assert.Equal(1, graph.EdgeCount);
    }

    [Fact]
    public void Parse_OddVertexCount_Throws()
    {
        Assert.Throws<FormatException>(() => Graph.Parse("1 2 3", directed: false));
    }

    [Fact]
    public void CreateRandom_IsDeterministicWithSeed()
    {
        var first = Graph.CreateRandom(8, 10, directed: false, seed: 42);
        var second = Graph.CreateRandom(8, 10, directed: false, seed: 42);

        Assert.Equal(first.Vertices, second.Vertices);
        Assert.Equal(first.Edges, second.Edges);
        Assert.Equal(8, first.Vertices.Count);
    }

    [Fact]
    public void Describe_IncludesKindAndCounts()
    {
        var graph = Graph.Parse("1 2\n2 3", directed: false);

        var text = graph.Describe();

        Assert.Contains("неориентированный", text, StringComparison.Ordinal);
        Assert.Contains("3 вершин", text, StringComparison.Ordinal);
        Assert.Contains("2 рёбер", text, StringComparison.Ordinal);
    }
}

public sealed class IterativeDfsTests
{
    private static Graph Diamond() => Graph.Parse("1 2\n1 3\n2 4\n3 4", directed: false);

    [Fact]
    public void Run_VisitsVerticesInExpectedOrder()
    {
        var result = IterativeDfs.Run(Diamond(), 1);

        Assert.Equal([1, 2, 4, 3], result.VisitOrder);
        Assert.Equal(5, result.PushCount);
        Assert.Equal(5, result.PopCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(1, result.MaxStackDepth);
        Assert.Contains("push 1", result.Trace);
        Assert.Contains("visit 4", result.Trace);
    }

    [Fact]
    public void Run_ReachesAllVerticesOfConnectedGraph()
    {
        var result = IterativeDfs.Run(Diamond(), 1);

        Assert.Equal(4, result.VisitOrder.Count);
        Assert.Equal(4, result.VisitOrder.Distinct().Count());
    }

    [Fact]
    public void Run_LeavesUnreachableVerticesUnvisited()
    {
        var graph = Graph.Parse("1 2\n3", directed: false);

        var result = IterativeDfs.Run(graph, 1);

        Assert.Equal([1, 2], result.VisitOrder);
        Assert.Contains(result.Trace, line => line.Contains("достижимо 2 из 3", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_DirectedGraphFollowsEdgeDirection()
    {
        var graph = Graph.Parse("1 2\n2 3", directed: true);

        var result = IterativeDfs.Run(graph, 1);

        Assert.Equal([1, 2, 3], result.VisitOrder);
    }

    [Fact]
    public void Run_UnknownStartVertex_Throws()
    {
        Assert.Throws<ArgumentException>(() => IterativeDfs.Run(Diamond(), 99));
    }

    [Fact]
    public void Run_TruncatesLongTrace()
    {
        var graph = Graph.CreateRandom(30, 40, directed: false, seed: 7);

        var result = IterativeDfs.Run(graph, 1, maxTraceEntries: 10);

        Assert.True(result.Trace.Count <= 11);
        Assert.Contains("журнал обрезан", result.Trace[^1]);
    }
}

public sealed class BinarySearchTreeTests
{
    private static BinarySearchTree SampleTree()
    {
        var tree = new BinarySearchTree();
        tree.InsertRange([50, 30, 70, 20, 40, 60, 80]);
        return tree;
    }

    [Fact]
    public void Insert_AndContains_Work()
    {
        var tree = SampleTree();

        Assert.Equal(7, tree.Count);
        Assert.True(tree.Contains(40));
        Assert.False(tree.Contains(41));
        Assert.False(tree.Insert(50));
        Assert.Equal(7, tree.Count);
        Assert.True(tree.IsValid());
    }

    [Fact]
    public void InsertRange_ReturnsNumberOfNewValues()
    {
        var tree = new BinarySearchTree();

        Assert.Equal(4, tree.InsertRange([5, 5, 3, 7, 3, 9]));
        Assert.Equal(4, tree.Count);
    }

    [Fact]
    public void MinAndMax_ReturnExtremes()
    {
        var tree = SampleTree();

        Assert.Equal(20, tree.Min());
        Assert.Equal(80, tree.Max());
        Assert.Null(new BinarySearchTree().Min());
        Assert.Null(new BinarySearchTree().Max());
    }

    [Fact]
    public void Height_IsComputedIteratively()
    {
        Assert.Equal(3, SampleTree().Height());
        Assert.Equal(0, new BinarySearchTree().Height());

        var chain = new BinarySearchTree();
        chain.InsertRange([1, 2, 3, 4]);
        Assert.Equal(4, chain.Height());
    }

    [Fact]
    public void Traversals_ReturnExpectedOrders()
    {
        var tree = SampleTree();

        Assert.Equal([20, 30, 40, 50, 60, 70, 80], tree.Traverse(TreeTraversal.InOrder));
        Assert.Equal([50, 30, 20, 40, 70, 60, 80], tree.Traverse(TreeTraversal.PreOrder));
        Assert.Equal([20, 40, 30, 60, 80, 70, 50], tree.Traverse(TreeTraversal.PostOrder));
        Assert.Equal([50, 30, 70, 20, 40, 60, 80], tree.Traverse(TreeTraversal.LevelOrder));
    }

    [Fact]
    public void Remove_Leaf_Works()
    {
        var tree = SampleTree();

        Assert.True(tree.Remove(20));
        Assert.False(tree.Contains(20));
        Assert.Equal(6, tree.Count);
        Assert.True(tree.IsValid());
    }

    [Fact]
    public void Remove_NodeWithOneChild_Works()
    {
        var tree = new BinarySearchTree();
        tree.InsertRange([50, 30, 70, 20]);

        Assert.True(tree.Remove(30));
        Assert.False(tree.Contains(30));
        Assert.True(tree.Contains(20));
        Assert.Equal([20, 50, 70], tree.Traverse(TreeTraversal.InOrder));
        Assert.True(tree.IsValid());
    }

    [Fact]
    public void Remove_NodeWithTwoChildren_UsesSuccessor()
    {
        var tree = SampleTree();

        Assert.True(tree.Remove(50));
        Assert.False(tree.Contains(50));
        Assert.Equal([20, 30, 40, 60, 70, 80], tree.Traverse(TreeTraversal.InOrder));
        Assert.Equal(6, tree.Count);
        Assert.True(tree.IsValid());
    }

    [Fact]
    public void Remove_MissingValue_ReturnsFalse()
    {
        var tree = SampleTree();

        Assert.False(tree.Remove(42));
        Assert.Equal(7, tree.Count);
    }

    [Fact]
    public void Remove_AllValues_EmptiesTree()
    {
        var tree = SampleTree();

        foreach (var value in tree.Traverse(TreeTraversal.InOrder))
        {
            Assert.True(tree.Remove(value));
        }

        Assert.True(tree.IsEmpty);
        Assert.Equal(0, tree.Count);
        Assert.Empty(tree.Traverse(TreeTraversal.InOrder));
        Assert.True(tree.IsValid());
    }

    [Fact]
    public void Clear_ResetsTree()
    {
        var tree = SampleTree();

        tree.Clear();

        Assert.True(tree.IsEmpty);
        Assert.Equal(0, tree.Count);
        Assert.Equal("Дерево пусто", tree.Describe());
    }

    [Fact]
    public void Describe_ContainsStatistics()
    {
        var text = SampleTree().Describe();

        Assert.Contains("Узлов: 7", text, StringComparison.Ordinal);
        Assert.Contains("высота: 3", text, StringComparison.Ordinal);
        Assert.Contains("инвариант BST: выполняется", text, StringComparison.Ordinal);
    }
}
