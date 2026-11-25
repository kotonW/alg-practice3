namespace AlgorithmsLab3.Domain.Applications;

/// <summary>Узел бинарного дерева с произвольной меткой.</summary>
public sealed class PostfixNode
{
    public PostfixNode(string label) => Label = label;

    public string Label { get; }

    public PostfixNode? Left { get; internal set; }

    public PostfixNode? Right { get; internal set; }
}

/// <summary>
/// Бинарное дерево, построенное по записи вида «5 3 1 * * 4 * * 8 * *»:
/// значения задают узлы, «*» отмечает отсутствие потомка.
/// Узел получает левое и правое поддерево по порядку, лист записывается как «значение * *».
/// </summary>
public sealed class PostfixTree
{
    private PostfixTree(PostfixNode root)
    {
        Root = root;
        Count = CountNodes(root);
        Height = HeightOf(root);
        Leaves = CountLeaves(root);
    }

    public PostfixNode Root { get; }

    public int Count { get; }

    public int Height { get; }

    public int Leaves { get; }

    public static PostfixTree Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            throw new FormatException("Введите запись дерева.");
        }

        var stack = new Stack<Frame>();
        PostfixNode? root = null;
        foreach (var token in tokens)
        {
            if (token == "*")
            {
                if (stack.Count == 0)
                {
                    throw new FormatException(
                        "Символ * не может стоять в начале записи или после завершённого дерева.");
                }

                stack.Peek().Add(null);
            }
            else
            {
                var node = new PostfixNode(token);
                if (stack.Count == 0)
                {
                    if (root is not null)
                    {
                        throw new FormatException($"Лишний узел «{token}»: дерево уже завершено.");
                    }

                    root = node;
                    stack.Push(new Frame(node));
                }
                else
                {
                    stack.Peek().Add(node);
                    stack.Push(new Frame(node));
                }
            }

            while (stack.Count > 0 && stack.Peek().IsFull)
            {
                stack.Pop();
            }
        }

        if (root is null)
        {
            throw new FormatException("Запись не содержит узлов.");
        }

        if (stack.Count > 0)
        {
            throw new FormatException(
                $"Запись не завершена: у узла «{stack.Peek().Node.Label}» не хватает потомков.");
        }

        return new PostfixTree(root);
    }

    private static int CountNodes(PostfixNode node) =>
        1
        + (node.Left is null ? 0 : CountNodes(node.Left))
        + (node.Right is null ? 0 : CountNodes(node.Right));

    private static int HeightOf(PostfixNode node)
    {
        var left = node.Left is null ? 0 : HeightOf(node.Left);
        var right = node.Right is null ? 0 : HeightOf(node.Right);
        return 1 + Math.Max(left, right);
    }

    private static int CountLeaves(PostfixNode node)
    {
        if (node.Left is null && node.Right is null)
        {
            return 1;
        }

        return (node.Left is null ? 0 : CountLeaves(node.Left))
               + (node.Right is null ? 0 : CountLeaves(node.Right));
    }

    private sealed class Frame
    {
        public Frame(PostfixNode node) => Node = node;

        public PostfixNode Node { get; }

        public int Filled { get; private set; }

        public bool IsFull => Filled >= 2;

        public void Add(PostfixNode? child)
        {
            if (Filled == 0)
            {
                Node.Left = child;
            }
            else if (Filled == 1)
            {
                Node.Right = child;
            }

            Filled++;
        }
    }
}
