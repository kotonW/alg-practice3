using AlgorithmsLab3.Domain.Queues;
using AlgorithmsLab3.Domain.Stacks;

namespace AlgorithmsLab3.Tests;

public sealed class LinkedStackTests
{
    [Fact]
    public void PushAndPop_WorkAsLifo()
    {
        var stack = new LinkedStack<string>();

        stack.Push("first");
        stack.Push("second");
        stack.Push("third");

        Assert.Equal(3, stack.Count);
        Assert.False(stack.IsEmpty);
        Assert.Equal("third", stack.Top());
        Assert.Equal("third", stack.Pop());
        Assert.Equal("second", stack.Pop());
        Assert.Equal("first", stack.Pop());
        Assert.True(stack.IsEmpty);
        Assert.Equal(0, stack.Count);
    }

    [Fact]
    public void Top_DoesNotRemoveItem()
    {
        var stack = new LinkedStack<int>();
        stack.Push(42);

        Assert.Equal(42, stack.Top());
        Assert.Equal(1, stack.Count);
        Assert.Equal(42, stack.Pop());
    }

    [Fact]
    public void Pop_OnEmptyStack_Throws()
    {
        var stack = new LinkedStack<int>();

        Assert.Throws<InvalidOperationException>(() => stack.Pop());
        Assert.Throws<InvalidOperationException>(() => stack.Top());
        Assert.False(stack.TryPop(out _));
        Assert.False(stack.TryTop(out _));
    }

    [Fact]
    public void SnapshotAndPrint_ReturnTopToBottom()
    {
        var stack = new LinkedStack<int>();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        Assert.Equal([3, 2, 1], stack.Snapshot());
        Assert.Equal("[3, 2, 1]", stack.Print());
    }

    [Fact]
    public void Print_OnEmptyStack_ReturnsEmptyBrackets()
    {
        var stack = new LinkedStack<int>();

        Assert.Equal("[]", stack.Print());
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var stack = new LinkedStack<int>();
        stack.Push(1);
        stack.Push(2);

        stack.Clear();

        Assert.True(stack.IsEmpty);
        Assert.Equal(0, stack.Count);
        Assert.Equal("[]", stack.Print());
    }
}

public sealed class LinkedQueueTests
{
    [Fact]
    public void EnqueueAndDequeue_WorkAsFifo()
    {
        IQueue<string> queue = new LinkedQueue<string>();

        queue.Enqueue("first");
        queue.Enqueue("second");
        queue.Enqueue("third");

        Assert.Equal(3, queue.Count);
        Assert.Equal("[first, second, third]", queue.Print());
        Assert.Equal("first", Peek(queue));
        Assert.Equal("first", Dequeue(queue));
        Assert.Equal("second", Dequeue(queue));
        Assert.Equal("third", Dequeue(queue));
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void DequeueAndPeek_OnEmptyQueue_ReturnFalse()
    {
        IQueue<int> queue = new LinkedQueue<int>();

        Assert.False(queue.TryDequeue(out _));
        Assert.False(queue.TryPeek(out _));
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void Queue_KeepsTailAfterPartialDequeue()
    {
        IQueue<int> queue = new LinkedQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);

        Assert.True(queue.TryDequeue(out var first));
        queue.Enqueue(3);

        Assert.Equal(1, first);
        Assert.Equal([2, 3], queue.Snapshot());
        Assert.Equal("[2, 3]", queue.Print());
    }

    [Fact]
    public void LinkedAndStandardQueues_ProduceSameResults()
    {
        var commands = "1,1 1,2 1,3 2 3 4 1,4 2 5";
        IQueue<string> linked = new LinkedQueue<string>();
        IQueue<string> standard = new StandardQueueAdapter<string>();

        var linkedResult = Run(linked, commands);
        var standardResult = Run(standard, commands);

        Assert.Equal(linkedResult, standardResult);
        Assert.Equal("[3, 4]", linkedResult.State);
    }

    [Fact]
    public void StandardQueueAdapter_WorksAsFifo()
    {
        IQueue<int> queue = new StandardQueueAdapter<int>();

        queue.Enqueue(10);
        queue.Enqueue(20);

        Assert.Equal("10", Peek(queue));
        Assert.Equal("10", Dequeue(queue));
        Assert.Equal(1, queue.Count);
        Assert.Equal("[20]", queue.Print());

        queue.Clear();
        Assert.True(queue.IsEmpty);
    }

    private static string Peek(IQueue<string> queue)
    {
        Assert.True(queue.TryPeek(out var value));
        return value;
    }

    private static string Dequeue(IQueue<string> queue)
    {
        Assert.True(queue.TryDequeue(out var value));
        return value;
    }

    private static string Peek(IQueue<int> queue)
    {
        Assert.True(queue.TryPeek(out var value));
        return value.ToString();
    }

    private static string Dequeue(IQueue<int> queue)
    {
        Assert.True(queue.TryDequeue(out var value));
        return value.ToString();
    }

    private static (string State, int Count) Run(IQueue<string> queue, string input)
    {
        foreach (var token in AlgorithmsLab3.Domain.Execution.CommandInput.Tokenize(input))
        {
            switch (token)
            {
                case "2":
                    queue.TryDequeue(out _);
                    break;
                case "3":
                    queue.TryPeek(out _);
                    break;
                default:
                    if (token.StartsWith("1,", StringComparison.Ordinal))
                    {
                        queue.Enqueue(token[2..]);
                    }

                    break;
            }
        }

        return (queue.Print(), queue.Count);
    }
}
