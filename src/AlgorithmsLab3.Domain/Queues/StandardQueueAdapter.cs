namespace AlgorithmsLab3.Domain.Queues;

/// <summary>Адаптер над стандартным System.Collections.Generic.Queue&lt;T&gt;.</summary>
public sealed class StandardQueueAdapter<T> : IQueue<T>
{
    private readonly Queue<T> _queue = new();

    public string DisplayName => "Стандартный Queue<T>";

    public int Count => _queue.Count;

    public bool IsEmpty => _queue.Count == 0;

    public void Enqueue(T item) => _queue.Enqueue(item);

    public bool TryDequeue(out T item)
    {
        if (_queue.Count == 0)
        {
            item = default!;
            return false;
        }

        item = _queue.Dequeue();
        return true;
    }

    public bool TryPeek(out T item)
    {
        if (_queue.Count == 0)
        {
            item = default!;
            return false;
        }

        item = _queue.Peek();
        return true;
    }

    public IReadOnlyList<T> Snapshot() => _queue.ToArray();

    public string Print(Func<T, string>? formatter = null)
    {
        formatter ??= static value => value?.ToString() ?? "null";
        return "[" + string.Join(", ", _queue.Select(formatter)) + "]";
    }

    public void Clear() => _queue.Clear();
}
