using System.Collections.ObjectModel;
using System.Globalization;
using AlgorithmsLab3.Domain.Applications;
using AlgorithmsLab3.App.Visualization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmsLab3.App.ViewModels;

public partial class DfsPageViewModel : ObservableObject
{
    private const string SampleEdges =
        "1 2\n1 3\n2 4\n2 5\n3 6\n4 7\n5 7\n6 8\n7 8";

    [ObservableProperty]
    private string _edgesText = SampleEdges;

    [ObservableProperty]
    private string _startText = "1";

    [ObservableProperty]
    private bool _directed;

    [ObservableProperty]
    private string _graphInfo = string.Empty;

    [ObservableProperty]
    private string _visitOrderText = string.Empty;

    [ObservableProperty]
    private string _stats = string.Empty;

    [ObservableProperty]
    private string _status = "Запустите обход: стек хранит вершины, рекурсия не используется.";

    [ObservableProperty]
    private GraphVisualization? _visualization;

    public DfsPageViewModel()
    {
        RunDfs();
    }

    public ObservableCollection<string> Trace { get; } = [];

    [RelayCommand]
    private void RunDfs()
    {
        try
        {
            var graph = Graph.Parse(EdgesText, Directed);
            if (graph.Vertices.Count == 0)
            {
                throw new FormatException("Добавьте хотя бы одно ребро или вершину.");
            }

            if (!int.TryParse(StartText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var start))
            {
                throw new FormatException("Стартовая вершина должна быть целым числом.");
            }

            if (!graph.ContainsVertex(start))
            {
                throw new FormatException($"Вершины {start} нет в графе.");
            }

            var result = IterativeDfs.Run(graph, start);
            GraphInfo = graph.Describe();
            VisitOrderText = result.VisitOrder.Count == 0
                ? "—"
                : string.Join(" → ", result.VisitOrder);

            var reachable = result.VisitOrder.Count;
            var total = graph.Vertices.Count;
            Stats =
                $"Достижимо вершин: {reachable} из {total} · "
                + $"push: {result.PushCount} · pop: {result.PopCount} · "
                + $"повторных извлечений: {result.SkippedCount} · "
                + $"максимальная глубина стека: {result.MaxStackDepth} · "
                + $"сложность O(V + E)";

            Trace.Clear();
            foreach (var line in result.Trace)
            {
                Trace.Add(line);
            }

            Visualization = new GraphVisualization(
                graph.Vertices,
                graph.Edges,
                result.VisitOrder,
                start,
                graph.IsDirected);

            Status = reachable == total
                ? "Обход завершён: достижимы все вершины."
                : $"Обход завершён: достижимо {reachable} из {total} вершин. Остальные компоненты не посещены.";
        }
        catch (Exception exception)
        {
            Status = "Ошибка: " + exception.Message;
            GraphInfo = "—";
            VisitOrderText = "—";
            Stats = string.Empty;
            Trace.Clear();
        }
    }

    [RelayCommand]
    private void RandomGraph()
    {
        var random = new Random();
        var vertexCount = random.Next(8, 15);
        var graph = Graph.CreateRandom(vertexCount, random.Next(vertexCount, vertexCount * 2 + 1), directed: false);
        EdgesText = string.Join(
            Environment.NewLine,
            graph.Edges.Select(edge => $"{edge.From} {edge.To}"));
        Directed = false;
        StartText = graph.Vertices[0].ToString(CultureInfo.CurrentCulture);
        RunDfs();
        Status = $"Сгенерирован случайный граф на {vertexCount} вершинах.";
    }

    [RelayCommand]
    private void LoadSample()
    {
        EdgesText = SampleEdges;
        StartText = "1";
        Directed = false;
        RunDfs();
        Status = "Загружен пример графа.";
    }
}
