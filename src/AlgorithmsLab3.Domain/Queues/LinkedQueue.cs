namespace AlgorithmsLab3.Domain.Queues;

/// <summary>
/// Очередь на собственном односвязном списке.
/// Вставка в хвост, удаление из головы, просмотр начала и проверка пустоты — O(1).
/// </summary>
public sealed class LinkedQueue<T> : IQueue<T>
{
    private sealed class Node
    {
        public Node(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public Node? Next { get; set; }
    }

    private Node? _head;
    private Node? _tail;

    public string DisplayName => "Собственный связный список";

    public int Count { get; private set; }

    public bool IsEmpty => _head is null;

    public void Enqueue(T item)
    {
        var node = new Node(item);
        if (_tail is null)
        {
            _head = node;
            _tail = node;
        }
        else
        {
            _tail.Next = node;
            _tail = node;
        }

        Count++;
    }

    public bool TryDequeue(out T item)
    {
        if (_head is null)
        {
            item = default!;
            return false;
        }

        item = _head.Value;
        _head = _head.Next;
        if (_head is null)
        {
            _tail = null;
        }

        Count--;
        return true;
    }

    public bool TryPeek(out T item)
    {
        if (_head is null)
        {
            item = default!;
            return false;
        }

        item = _head.Value;
        return true;
    }

    public IReadOnlyList<T> Snapshot()
    {
        var result = new List<T>(Count);
        for (var node = _head; node is not null; node = node.Next)
        {
            result.Add(node.Value);
        }

        return result;
    }

    public string Print(Func<T, string>? formatter = null)
    {
        formatter ??= static value => value?.ToString() ?? "null";
        return "[" + string.Join(", ", Snapshot().Select(formatter)) + "]";
    }

    public void Clear()
    {
        _head = null;
        _tail = null;
        Count = 0;
    }
}
