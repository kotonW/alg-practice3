using System.Text;

namespace AlgorithmsLab3.Domain.Lists;

/// <summary>
/// Собственный односвязный список целых чисел и набор алгоритмов части 4.
/// Список не содержит циклов: все операции работают с линейной структурой.
/// </summary>
public sealed class IntLinkedList
{
    private SinglyNode<int>? _head;
    private SinglyNode<int>? _tail;

    public int Count { get; private set; }

    public bool IsEmpty => _head is null;

    public int? HeadValue => _head?.Value;

    public int? TailValue => _tail?.Value;

    public SinglyNode<int>? Head => _head;

    public static IntLinkedList FromValues(IEnumerable<int> values)
    {
        var list = new IntLinkedList();
        list.AddRange(values);
        return list;
    }

    public void Add(int value)
    {
        var node = new SinglyNode<int>(value);
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

    public void AddRange(IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        foreach (var value in values)
        {
            Add(value);
        }
    }

    public void Replace(IEnumerable<int> values)
    {
        _head = null;
        _tail = null;
        Count = 0;
        AddRange(values);
    }

    public IReadOnlyList<int> ToList()
    {
        var result = new List<int>(Count);
        for (var node = _head; node is not null; node = node.Next)
        {
            result.Add(node.Value);
        }

        return result;
    }

    public IntLinkedList Clone() => FromValues(ToList());

    /// <summary>Переворачивает список, изменяя ссылки между узлами. O(n).</summary>
    public bool ReverseInPlace()
    {
        if (Count < 2)
        {
            return false;
        }

        SinglyNode<int>? previous = null;
        var current = _head;
        while (current is not null)
        {
            var next = current.Next;
            current.Next = previous;
            previous = current;
            current = next;
        }

        _tail = _head;
        _head = previous;
        return true;
    }

    /// <summary>Переносит последний элемент в начало непустого списка. O(n).</summary>
    public bool MoveLastToFront()
    {
        if (Count < 2)
        {
            return false;
        }

        var beforeTail = NodeAt(Count - 2);
        var tail = _tail!;
        beforeTail.Next = null;
        tail.Next = _head;
        _head = tail;
        _tail = beforeTail;
        return true;
    }

    /// <summary>Переносит первый элемент в конец непустого списка. O(n).</summary>
    public bool MoveFirstToEnd()
    {
        if (Count < 2)
        {
            return false;
        }

        var first = _head!;
        _head = first.Next;
        first.Next = null;
        _tail!.Next = first;
        _tail = first;
        return true;
    }

    /// <summary>Количество различных значений списка. O(n) в среднем.</summary>
    public int CountDistinct()
    {
        var set = new HashSet<int>();
        for (var node = _head; node is not null; node = node.Next)
        {
            set.Add(node.Value);
        }

        return set.Count;
    }

    /// <summary>Удаляет элементы, встречающиеся более одного раза. Возвращает число удалённых.</summary>
    public int RemoveNonUnique()
    {
        if (_head is null)
        {
            return 0;
        }

        var frequencies = new Dictionary<int, int>();
        for (var node = _head; node is not null; node = node.Next)
        {
            frequencies[node.Value] = frequencies.GetValueOrDefault(node.Value) + 1;
        }

        var kept = ToList().Where(value => frequencies[value] == 1).ToList();
        var removed = Count - kept.Count;
        Replace(kept);
        return removed;
    }

    /// <summary>
    /// Вставляет копию всего списка после первого вхождения <paramref name="value"/>.
    /// Возвращает количество добавленных узлов (0, если значение не найдено).
    /// </summary>
    public int InsertSelfAfter(int value)
    {
        var anchor = Find(value);
        if (anchor is null)
        {
            return 0;
        }

        var values = ToList();
        SinglyNode<int>? copyHead = null;
        SinglyNode<int>? copyTail = null;
        foreach (var item in values)
        {
            var copy = new SinglyNode<int>(item);
            if (copyHead is null)
            {
                copyHead = copy;
            }
            else
            {
                copyTail!.Next = copy;
            }

            copyTail = copy;
        }

        var oldNext = anchor.Next;
        anchor.Next = copyHead;
        copyTail!.Next = oldNext;
        if (ReferenceEquals(anchor, _tail))
        {
            _tail = copyTail;
        }

        Count += values.Count;
        return values.Count;
    }

    /// <summary>Вставка в отсортированный по неубыванию список с сохранением порядка. O(n).</summary>
    public bool InsertSorted(int value)
    {
        var node = new SinglyNode<int>(value);
        if (_head is null || value <= _head.Value)
        {
            node.Next = _head;
            _head = node;
            _tail ??= node;
            Count++;
            return true;
        }

        var current = _head;
        while (current.Next is not null && current.Next.Value <= value)
        {
            current = current.Next;
        }

        node.Next = current.Next;
        current.Next = node;
        if (ReferenceEquals(current, _tail))
        {
            _tail = node;
        }

        Count++;
        return true;
    }

    /// <summary>Удаляет все вхождения значения. Возвращает число удалённых узлов.</summary>
    public int RemoveAll(int value)
    {
        var removed = 0;
        SinglyNode<int>? previous = null;
        var current = _head;
        while (current is not null)
        {
            if (current.Value == value)
            {
                if (previous is null)
                {
                    _head = current.Next;
                }
                else
                {
                    previous.Next = current.Next;
                }

                if (ReferenceEquals(current, _tail))
                {
                    _tail = previous;
                }

                Count--;
                removed++;
            }
            else
            {
                previous = current;
            }

            current = current.Next;
        }

        return removed;
    }

    /// <summary>Вставляет <paramref name="value"/> перед первым вхождением <paramref name="existing"/>.</summary>
    public bool InsertBefore(int existing, int value)
    {
        if (_head is null)
        {
            return false;
        }

        if (_head.Value == existing)
        {
            var newHead = new SinglyNode<int>(value) { Next = _head };
            _head = newHead;
            Count++;
            return true;
        }

        var current = _head;
        while (current.Next is not null)
        {
            if (current.Next.Value == existing)
            {
                current.Next = new SinglyNode<int>(value) { Next = current.Next };
                Count++;
                return true;
            }

            current = current.Next;
        }

        return false;
    }

    /// <summary>Дописывает значения второго списка в конец текущего. O(m).</summary>
    public int Append(IntLinkedList other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var values = other.ToList();
        AddRange(values);
        return values.Count;
    }

    /// <summary>
    /// Делит список по первому вхождению значения: первый список — элементы до него,
    /// второй — начиная с него. Если значения нет, второй список пуст.
    /// </summary>
    public (IntLinkedList First, IntLinkedList Second) SplitByFirst(int value)
    {
        var values = ToList();
        var index = values.ToList().IndexOf(value);
        if (index < 0)
        {
            return (FromValues(values), new IntLinkedList());
        }

        return (FromValues(values.Take(index)), FromValues(values.Skip(index)));
    }

    /// <summary>Приписывает в конец копию самого списка. O(n).</summary>
    public bool Double()
    {
        if (_head is null)
        {
            return false;
        }

        AddRange(ToList());
        return true;
    }

    /// <summary>Меняет местами два узла по позициям (0-based), перестраивая ссылки. O(n).</summary>
    public bool SwapPositions(int first, int second)
    {
        if (first == second || first < 0 || second < 0 || first >= Count || second >= Count)
        {
            return false;
        }

        if (first > second)
        {
            (first, second) = (second, first);
        }

        var previousFirst = first == 0 ? null : NodeAt(first - 1);
        var firstNode = NodeAt(first);
        var previousSecond = second == first + 1 ? firstNode : NodeAt(second - 1);
        var secondNode = NodeAt(second);

        var firstWasTail = ReferenceEquals(firstNode, _tail);
        var secondWasTail = ReferenceEquals(secondNode, _tail);

        var afterFirst = firstNode.Next;
        var afterSecond = secondNode.Next;

        if (ReferenceEquals(firstNode.Next, secondNode))
        {
            // Соседние узлы: A -> B.
            firstNode.Next = afterSecond;
            secondNode.Next = firstNode;
        }
        else
        {
            // Между узлами есть как минимум один узел: X -> … -> C -> Y.
            firstNode.Next = afterSecond;
            secondNode.Next = afterFirst;
            previousSecond.Next = firstNode;
        }

        if (previousFirst is null)
        {
            _head = secondNode;
        }
        else
        {
            previousFirst.Next = secondNode;
        }

        if (secondWasTail)
        {
            _tail = firstNode;
        }

        if (firstWasTail)
        {
            _tail = secondNode;
        }

        return true;
    }

    public string Describe(int limit = 80)
    {
        var values = ToList();
        if (values.Count == 0)
        {
            return "[]";
        }

        var builder = new StringBuilder("[");
        var shown = Math.Min(values.Count, limit);
        for (var index = 0; index < shown; index++)
        {
            if (index > 0)
            {
                builder.Append(", ");
            }

            builder.Append(values[index]);
        }

        if (values.Count > shown)
        {
            builder.Append(", … (+").Append(values.Count - shown).Append(')');
        }

        return builder.Append(']').ToString();
    }

    public override string ToString() => Describe();

    private SinglyNode<int>? Find(int value)
    {
        for (var node = _head; node is not null; node = node.Next)
        {
            if (node.Value == value)
            {
                return node;
            }
        }

        return null;
    }

    private SinglyNode<int> NodeAt(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var node = _head!;
        for (var i = 0; i < index; i++)
        {
            node = node.Next!;
        }

        return node;
    }
}
