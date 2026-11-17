using AlgorithmsLab3.Domain.Lists;

namespace AlgorithmsLab3.Domain.Applications;

public sealed record FloydStep(int Step, int? TortoiseValue, int? HareValue, bool Met)
{
    public string TortoiseText => TortoiseValue?.ToString() ?? "—";

    public string HareText => HareValue?.ToString() ?? "—";

    public string Mark => Met ? "встреча" : string.Empty;
}

public sealed record FloydCycleResult(
    bool HasCycle,
    int? CycleStartIndex,
    int? CycleStartValue,
    int? CycleLength,
    int? MeetingIndex,
    int? MeetingValue,
    int Iterations,
    int NodeCount,
    IReadOnlyList<FloydStep> Trace)
{
    public string Summary => HasCycle
        ? $"Цикл есть: вход в узел #{CycleStartIndex} (значение {CycleStartValue}), длина цикла {CycleLength}."
        : "Цикла нет: заяц дошёл до конца списка.";
}

/// <summary>
/// Список для демонстрации алгоритма Флойда «черепаха и заяц».
/// Позволяет явно замкнуть хвост списка на произвольный узел.
/// </summary>
public sealed class CycleLinkedList
{
    private readonly List<SinglyNode<int>> _nodes = [];

    public SinglyNode<int>? Head { get; private set; }

    public int? CycleStartIndex { get; private set; }

    public IReadOnlyList<int> Values => _nodes.Select(node => node.Value).ToList();

    public int Count => _nodes.Count;

    public void Build(IReadOnlyList<int> values, int? cycleToIndex)
    {
        ArgumentNullException.ThrowIfNull(values);
        _nodes.Clear();
        Head = null;
        CycleStartIndex = null;

        if (values.Count == 0)
        {
            return;
        }

        SinglyNode<int>? previous = null;
        foreach (var value in values)
        {
            var node = new SinglyNode<int>(value);
            _nodes.Add(node);
            if (previous is null)
            {
                Head = node;
            }
            else
            {
                previous.Next = node;
            }

            previous = node;
        }

        if (cycleToIndex is { } target && target >= 0 && target < _nodes.Count)
        {
            CycleStartIndex = target;
            previous!.Next = _nodes[target];
        }
    }

    public FloydCycleResult Detect()
    {
        if (Head is null)
        {
            return new FloydCycleResult(false, null, null, null, null, null, 0, 0, []);
        }

        var trace = new List<FloydStep>();
        var tortoise = Head;
        var hare = Head;
        var iterations = 0;
        var met = false;

        while (hare?.Next is not null)
        {
            tortoise = tortoise!.Next;
            hare = hare.Next.Next;
            iterations++;
            var areEqual = ReferenceEquals(tortoise, hare);
            trace.Add(new FloydStep(iterations, tortoise?.Value, hare?.Value, areEqual));
            if (areEqual)
            {
                met = true;
                break;
            }
        }

        if (!met)
        {
            return new FloydCycleResult(
                false,
                null,
                null,
                null,
                null,
                null,
                iterations,
                _nodes.Count,
                trace);
        }

        var start = FindCycleStart(Head, hare!, out var startIndex);
        var cycleLength = FindCycleLength(start);

        return new FloydCycleResult(
            true,
            startIndex,
            start.Value,
            cycleLength,
            FindReferenceIndex(Head, hare!),
            hare!.Value,
            iterations,
            _nodes.Count,
            trace);
    }

    private static SinglyNode<int> FindCycleStart(SinglyNode<int> head, SinglyNode<int> meeting, out int index)
    {
        var first = head;
        var second = meeting;
        var position = 0;
        while (!ReferenceEquals(first, second))
        {
            first = first.Next!;
            second = second.Next!;
            position++;
        }

        index = position;
        return first;
    }

    private static int FindCycleLength(SinglyNode<int> start)
    {
        var length = 1;
        var current = start.Next!;
        while (!ReferenceEquals(current, start))
        {
            current = current.Next!;
            length++;
        }

        return length;
    }

    private static int FindReferenceIndex(SinglyNode<int> head, SinglyNode<int> target)
    {
        var index = 0;
        for (var node = head; node is not null; node = node.Next)
        {
            if (ReferenceEquals(node, target))
            {
                return index;
            }

            index++;
        }

        return -1;
    }
}
