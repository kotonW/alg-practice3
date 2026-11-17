using AlgorithmsLab3.Domain.Stacks;

namespace AlgorithmsLab3.Domain.Applications;

public sealed record DfsResult(
    int Start,
    IReadOnlyList<int> VisitOrder,
    int PushCount,
    int PopCount,
    int SkippedCount,
    int MaxStackDepth,
    IReadOnlyList<string> Trace);

/// <summary>
/// Итеративный обход графа в глубину с использованием собственного стека.
/// Рекурсия не используется.
/// </summary>
public static class IterativeDfs
{
    public static DfsResult Run(Graph graph, int start, int maxTraceEntries = 500)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (!graph.ContainsVertex(start))
        {
            throw new ArgumentException($"Вершины {start} нет в графе.", nameof(start));
        }

        var visited = new HashSet<int>();
        var order = new List<int>();
        var trace = new List<string>();
        var stack = new LinkedStack<int>();
        var pushCount = 0;
        var popCount = 0;
        var skipped = 0;
        var maxDepth = 0;

        stack.Push(start);
        pushCount++;
        AddTrace(trace, maxTraceEntries, $"push {start}");

        while (!stack.IsEmpty)
        {
            var vertex = stack.Pop();
            popCount++;
            maxDepth = Math.Max(maxDepth, stack.Count);
            AddTrace(trace, maxTraceEntries, $"pop {vertex}");

            if (!visited.Add(vertex))
            {
                skipped++;
                AddTrace(trace, maxTraceEntries, $"вершина {vertex} уже посещена — пропуск");
                continue;
            }

            order.Add(vertex);
            AddTrace(trace, maxTraceEntries, $"visit {vertex}");

            var neighbors = graph.Neighbors(vertex)
                .Where(neighbor => !visited.Contains(neighbor))
                .ToList();

            if (neighbors.Count == 0)
            {
                continue;
            }

            // Кладём в обратном порядке, чтобы меньший сосед оказался на вершине стека.
            for (var index = neighbors.Count - 1; index >= 0; index--)
            {
                stack.Push(neighbors[index]);
                pushCount++;
            }

            AddTrace(trace, maxTraceEntries, $"push [{string.Join(", ", neighbors)}]");
        }

        if (order.Count < graph.Vertices.Count)
        {
            AddTrace(
                trace,
                maxTraceEntries,
                $"достижимо {order.Count} из {graph.Vertices.Count} вершин (запустите обход из другой компоненты)");
        }

        return new DfsResult(start, order, pushCount, popCount, skipped, maxDepth, trace);
    }

    private static void AddTrace(List<string> trace, int maxEntries, string message)
    {
        if (trace.Count < maxEntries)
        {
            trace.Add(message);
        }
        else if (trace.Count == maxEntries)
        {
            trace.Add("… журнал обрезан");
        }
    }
}
