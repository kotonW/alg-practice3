using AlgorithmsLab3.Domain.Queues;
using AlgorithmsLab3.Domain.Stacks;

namespace AlgorithmsLab3.Domain.Applications;

public enum TreeTraversal
{
    InOrder,
    PreOrder,
    PostOrder,
    LevelOrder,
}

public sealed class BstNode
{
    public BstNode(int value)
    {
        Value = value;
    }

    public int Value { get; internal set; }

    public BstNode? Left { get; internal set; }

    public BstNode? Right { get; internal set; }
}

/// <summary>
/// Двоичное дерево поиска как динамическое множество: дубликаты не хранятся,
/// вставка/поиск/удаление — O(h), где h — высота дерева (O(log n) для сбалансированного).
/// </summary>
public sealed class BinarySearchTree
{
    public BstNode? Root { get; private set; }

    public int Count { get; private set; }

    public bool IsEmpty => Root is null;

    public bool Insert(int value)
    {
        if (Root is null)
        {
            Root = new BstNode(value);
            Count++;
            return true;
        }

        var current = Root;
        while (true)
        {
            if (value == current.Value)
            {
                return false;
            }

            if (value < current.Value)
            {
                if (current.Left is null)
                {
                    current.Left = new BstNode(value);
                    Count++;
                    return true;
                }

                current = current.Left;
            }
            else
            {
                if (current.Right is null)
                {
                    current.Right = new BstNode(value);
                    Count++;
                    return true;
                }

                current = current.Right;
            }
        }
    }

    public int InsertRange(IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var inserted = 0;
        foreach (var value in values)
        {
            if (Insert(value))
            {
                inserted++;
            }
        }

        return inserted;
    }

    public bool Contains(int value)
    {
        var current = Root;
        while (current is not null)
        {
            if (value == current.Value)
            {
                return true;
            }

            current = value < current.Value ? current.Left : current.Right;
        }

        return false;
    }

    public bool Remove(int value)
    {
        var removed = false;
        Root = RemoveNode(Root, value, ref removed);
        if (removed)
        {
            Count--;
        }

        return removed;
    }

    public int? Min()
    {
        if (Root is null)
        {
            return null;
        }

        var node = Root;
        while (node.Left is not null)
        {
            node = node.Left;
        }

        return node.Value;
    }

    public int? Max()
    {
        if (Root is null)
        {
            return null;
        }

        var node = Root;
        while (node.Right is not null)
        {
            node = node.Right;
        }

        return node.Value;
    }

    public int Height()
    {
        if (Root is null)
        {
            return 0;
        }

        var maxDepth = 0;
        var stack = new Stack<(BstNode Node, int Depth)>();
        stack.Push((Root, 1));
        while (stack.Count > 0)
        {
            var (node, depth) = stack.Pop();
            maxDepth = Math.Max(maxDepth, depth);
            if (node.Left is not null)
            {
                stack.Push((node.Left, depth + 1));
            }

            if (node.Right is not null)
            {
                stack.Push((node.Right, depth + 1));
            }
        }

        return maxDepth;
    }

    public void Clear()
    {
        Root = null;
        Count = 0;
    }

    public IReadOnlyList<int> Traverse(TreeTraversal order)
    {
        var result = new List<int>(Count);
        switch (order)
        {
            case TreeTraversal.InOrder:
                TraverseInOrder(result);
                break;
            case TreeTraversal.PreOrder:
                TraversePreOrder(result);
                break;
            case TreeTraversal.PostOrder:
                TraversePostOrder(result);
                break;
            case TreeTraversal.LevelOrder:
                TraverseLevelOrder(result);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(order), order, null);
        }

        return result;
    }

    /// <summary>Проверка инварианта BST: строго возрастающий симметричный обход.</summary>
    public bool IsValid()
    {
        int? previous = null;
        foreach (var value in TraverseInOrderIterative())
        {
            if (previous is not null && value <= previous)
            {
                return false;
            }

            previous = value;
        }

        return true;
    }

    public string Describe()
    {
        if (Root is null)
        {
            return "Дерево пусто";
        }

        return $"Узлов: {Count}, высота: {Height()}, минимум: {Min()}, максимум: {Max()}, инвариант BST: {(IsValid() ? "выполняется" : "нарушен")}";
    }

    private static BstNode? RemoveNode(BstNode? node, int value, ref bool removed)
    {
        if (node is null)
        {
            return null;
        }

        if (value < node.Value)
        {
            node.Left = RemoveNode(node.Left, value, ref removed);
            return node;
        }

        if (value > node.Value)
        {
            node.Right = RemoveNode(node.Right, value, ref removed);
            return node;
        }

        removed = true;

        if (node.Left is null)
        {
            return node.Right;
        }

        if (node.Right is null)
        {
            return node.Left;
        }

        var successor = node.Right;
        while (successor.Left is not null)
        {
            successor = successor.Left;
        }

        node.Value = successor.Value;
        node.Right = RemoveMin(node.Right);
        return node;
    }

    private static BstNode? RemoveMin(BstNode node)
    {
        if (node.Left is null)
        {
            return node.Right;
        }

        node.Left = RemoveMin(node.Left);
        return node;
    }

    private void TraverseInOrder(List<int> result)
    {
        var stack = new LinkedStack<BstNode>();
        var current = Root;
        while (current is not null || !stack.IsEmpty)
        {
            while (current is not null)
            {
                stack.Push(current);
                current = current.Left;
            }

            current = stack.Pop();
            result.Add(current.Value);
            current = current.Right;
        }
    }

    private IEnumerable<int> TraverseInOrderIterative()
    {
        var result = new List<int>(Count);
        TraverseInOrder(result);
        return result;
    }

    private void TraversePreOrder(List<int> result)
    {
        if (Root is null)
        {
            return;
        }

        var stack = new LinkedStack<BstNode>();
        stack.Push(Root);
        while (!stack.IsEmpty)
        {
            var node = stack.Pop();
            result.Add(node.Value);
            if (node.Right is not null)
            {
                stack.Push(node.Right);
            }

            if (node.Left is not null)
            {
                stack.Push(node.Left);
            }
        }
    }

    private void TraversePostOrder(List<int> result)
    {
        if (Root is null)
        {
            return;
        }

        var stack = new LinkedStack<BstNode>();
        var output = new LinkedStack<int>();
        stack.Push(Root);
        while (!stack.IsEmpty)
        {
            var node = stack.Pop();
            output.Push(node.Value);
            if (node.Left is not null)
            {
                stack.Push(node.Left);
            }

            if (node.Right is not null)
            {
                stack.Push(node.Right);
            }
        }

        result.AddRange(output.Snapshot());
    }

    private void TraverseLevelOrder(List<int> result)
    {
        if (Root is null)
        {
            return;
        }

        var queue = new LinkedQueue<BstNode>();
        queue.Enqueue(Root);
        while (!queue.IsEmpty)
        {
            queue.TryDequeue(out var node);
            result.Add(node.Value);
            if (node.Left is not null)
            {
                queue.Enqueue(node.Left);
            }

            if (node.Right is not null)
            {
                queue.Enqueue(node.Right);
            }
        }
    }
}
