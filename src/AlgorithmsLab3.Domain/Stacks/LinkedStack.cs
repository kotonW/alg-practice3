namespace AlgorithmsLab3.Domain.Stacks;

/// <summary>
/// Стек на собственном односвязном списке.
/// Все операции, кроме печати, работают за O(1).
/// </summary>
public sealed class LinkedStack<T>
{
    private sealed class Node
    {
        public Node(T value, Node? next)
        {
            Value = value;
            Next = next;
        }

        public T Value { get; }

        public Node? Next { get; }
    }

    private Node? _head;

    public int Count { get; private set; }

    public bool IsEmpty => _head is null;

    public void Push(T item)
    {
        _head = new Node(item, _head);
        Count++;
    }

    public T Pop()
    {
        if (!TryPop(out var item))
        {
            throw new InvalidOperationException("Стек пуст: операция Pop невозможна.");
        }

        return item;
    }

    public bool TryPop(out T item)
    {
        if (_head is null)
        {
            item = default!;
            return false;
        }

        item = _head.Value;
        _head = _head.Next;
        Count--;
        return true;
    }

    public T Top()
    {
        if (!TryTop(out var item))
        {
            throw new InvalidOperationException("Стек пуст: операция Top невозможна.");
        }

        return item;
    }

    public bool TryTop(out T item)
    {
        if (_head is null)
        {
            item = default!;
            return false;
        }

        item = _head.Value;
        return true;
    }

    /// <summary>Возвращает элементы от вершины к основанию.</summary>
    public IReadOnlyList<T> Snapshot()
    {
        var result = new List<T>(Count);
        for (var node = _head; node is not null; node = node.Next)
        {
            result.Add(node.Value);
        }

        return result;
    }

    /// <summary>Печать стека от вершины к основанию.</summary>
    public string Print(Func<T, string>? formatter = null)
    {
        formatter ??= static value => value?.ToString() ?? "null";
        return "[" + string.Join(", ", Snapshot().Select(formatter)) + "]";
    }

    public void Clear()
    {
        _head = null;
        Count = 0;
    }

    public override string ToString() => Print();
}
