namespace AlgorithmsLab3.Domain.Queues;

/// <summary>Общий интерфейс очереди для сравнения собственной и стандартной реализаций.</summary>
public interface IQueue<T>
{
    string DisplayName { get; }

    int Count { get; }

    bool IsEmpty { get; }

    void Enqueue(T item);

    bool TryDequeue(out T item);

    bool TryPeek(out T item);

    /// <summary>Элементы в порядке очереди: от начала к концу.</summary>
    IReadOnlyList<T> Snapshot();

    string Print(Func<T, string>? formatter = null);

    void Clear();
}
