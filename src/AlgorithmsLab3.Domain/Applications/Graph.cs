using System.Text.RegularExpressions;

namespace AlgorithmsLab3.Domain.Applications;

/// <summary>Неориентированный или ориентированный граф на списках смежности.</summary>
public sealed class Graph
{
    private readonly Dictionary<int, SortedSet<int>> _adjacency = [];
    private readonly List<(int From, int To)> _edges = [];

    public Graph(bool isDirected)
    {
        IsDirected = isDirected;
    }

    public bool IsDirected { get; }

    public IReadOnlyList<int> Vertices => _adjacency.Keys.Order().ToList();

    public IReadOnlyList<(int From, int To)> Edges => _edges;

    public int EdgeCount => _edges.Count;

    public bool ContainsVertex(int vertex) => _adjacency.ContainsKey(vertex);

    public void AddVertex(int vertex) => _adjacency.TryAdd(vertex, []);

    public void AddEdge(int from, int to)
    {
        AddVertex(from);
        AddVertex(to);
        if (_adjacency[from].Add(to) && !IsDirected)
        {
            _adjacency[to].Add(from);
        }

        _edges.Add((from, to));
    }

    public IReadOnlyList<int> Neighbors(int vertex) =>
        _adjacency.TryGetValue(vertex, out var set) ? set.ToList() : [];

    public static Graph CreateRandom(int vertexCount, int extraEdges, bool directed, int? seed = null)
    {
        vertexCount = Math.Clamp(vertexCount, 2, 60);
        var graph = new Graph(directed);
        for (var vertex = 1; vertex <= vertexCount; vertex++)
        {
            graph.AddVertex(vertex);
        }

        var random = seed is { } value ? new Random(value) : new Random();
        var edgeCount = Math.Clamp(extraEdges, 0, vertexCount * (vertexCount - 1) / 2 + vertexCount);
        var existing = new HashSet<(int, int)>();

        for (var i = 0; i < edgeCount; i++)
        {
            var from = random.Next(1, vertexCount + 1);
            var to = random.Next(1, vertexCount + 1);
            if (from == to)
            {
                i--;
                continue;
            }

            var key = from < to ? (from, to) : (to, from);
            if (!existing.Add(key))
            {
                i--;
                continue;
            }

            graph.AddEdge(from, to);
        }

        return graph;
    }

    /// <summary>
    /// Разбирает список рёбер. Поддерживаются строки вида "1 2", "1-2", "1,2", несколько рёбер через ';'.
    /// Одиночное число в строке добавляет изолированную вершину.
    /// </summary>
    public static Graph Parse(string text, bool directed)
    {
        ArgumentNullException.ThrowIfNull(text);
        var graph = new Graph(directed);
        var lineNumber = 0;

        foreach (var rawLine in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            lineNumber++;
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var matches = Regex.Matches(line, @"-?\d+");
            if (matches.Count == 0)
            {
                continue;
            }

            if (matches.Count == 1)
            {
                graph.AddVertex(int.Parse(matches[0].Value));
                continue;
            }

            if (matches.Count % 2 != 0)
            {
                throw new FormatException($"Строка {lineNumber}: нечётное количество вершин в описании рёбер.");
            }

            for (var i = 0; i < matches.Count; i += 2)
            {
                graph.AddEdge(int.Parse(matches[i].Value), int.Parse(matches[i + 1].Value));
            }
        }

        return graph;
    }

    public string Describe()
    {
        var kind = IsDirected ? "ориентированный" : "неориентированный";
        return $"{kind} граф: {Vertices.Count} вершин, {EdgeCount} рёбер";
    }
}
