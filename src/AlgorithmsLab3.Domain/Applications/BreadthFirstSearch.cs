using AlgorithmsLab3.Domain.Queues;

namespace AlgorithmsLab3.Domain.Applications;

public sealed record BfsResult(
    int Start,
    IReadOnlyList<int> VisitOrder,
    IReadOnlyList<int> Distances,
    int EnqueueCount,
    int DequeueCount,
    int SkippedCount,
    int MaxQueueDepth,
    IReadOnlyList<string> Trace)
{
    public int ReachableCount => VisitOrder.Count;

    /// <summary>Группирует посещённые вершины по расстоянию от стартовой.</summary>
    public IReadOnlyList<(int Level, IReadOnlyList<int> Vertices)> Levels()
    {
        var levels = new SortedDictionary<int, List<int>>();
        for (var index = 0; index < VisitOrder.Count; index++)
        {
            var level = Distances[index];
            if (!levels.TryGetValue(level, out var bucket))
            {
                bucket = [];
                levels[level] = bucket;
            }

            bucket.Add(VisitOrder[index]);
        }

        return levels
            .Select(pair => (pair.Key, (IReadOnlyList<int>)pair.Value))
            .ToList();
    }
}

/// <summary>
/// Обход графа в ширину (BFS) с использованием собственной очереди.
/// Вершина помечается обнаруженной при постановке в очередь, поэтому каждая
/// вершина обрабатывается ровно один раз. Сложность O(V + E).
/// </summary>
public static class BreadthFirstSearch
{
    public static BfsResult Run(Graph graph, int start, int maxTraceEntries = 500)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (!graph.ContainsVertex(start))
        {
            throw new ArgumentException($"Вершины {start} нет в графе.", nameof(start));
        }

        var queue = new LinkedQueue<int>();
        var discovered = new HashSet<int> { start };
        var distance = new Dictionary<int, int> { [start] = 0 };
        var order = new List<int>();
        var distances = new List<int>();
        var trace = new List<string>();
        var enqueues = 0;
        var dequeues = 0;
        var skipped = 0;
        var maxDepth = 0;

        queue.Enqueue(start);
        enqueues++;
        maxDepth = Math.Max(maxDepth, queue.Count);
        AddTrace(trace, maxTraceEntries, $"enqueue {start} (уровень 0)");

        while (!queue.IsEmpty)
        {
            queue.TryDequeue(out var vertex);
            dequeues++;
            var vertexDistance = distance[vertex];
            order.Add(vertex);
            distances.Add(vertexDistance);
            AddTrace(trace, maxTraceEntries, $"dequeue {vertex} → посещение (уровень {vertexDistance})");

            foreach (var neighbor in graph.Neighbors(vertex))
            {
                if (!discovered.Add(neighbor))
                {
                    skipped++;
                    continue;
                }

                var neighborDistance = vertexDistance + 1;
                distance[neighbor] = neighborDistance;
                queue.Enqueue(neighbor);
                enqueues++;
                maxDepth = Math.Max(maxDepth, queue.Count);
                AddTrace(trace, maxTraceEntries, $"enqueue {neighbor} (уровень {neighborDistance})");
            }
        }

        if (order.Count < graph.Vertices.Count)
        {
            AddTrace(
                trace,
                maxTraceEntries,
                $"достижимо {order.Count} из {graph.Vertices.Count} вершин (запустите обход из другой компоненты)");
        }

        return new BfsResult(
            start,
            order,
            distances,
            enqueues,
            dequeues,
            skipped,
            maxDepth,
            trace);
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
