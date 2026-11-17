using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AlgorithmsLab3.App.Visualization;

namespace AlgorithmsLab3.App.Controls;

public sealed class GraphCanvas : Control
{
    public static readonly StyledProperty<GraphVisualization?> VisualizationProperty =
        AvaloniaProperty.Register<GraphCanvas, GraphVisualization?>(nameof(Visualization));

    private static readonly IBrush NodeFill = new SolidColorBrush(Color.Parse("#22304A"));
    private static readonly IBrush NodeVisited = new SolidColorBrush(Color.Parse("#214C3A"));
    private static readonly IBrush NodeStart = new SolidColorBrush(Color.Parse("#3A4F7A"));
    private static readonly IBrush NodeText = new SolidColorBrush(Color.Parse("#F2F5FA"));
    private static readonly IBrush Edge = new SolidColorBrush(Color.Parse("#61718A"));
    private static readonly IBrush Accent = new SolidColorBrush(Color.Parse("#5B9CFF"));
    private static readonly IBrush Good = new SolidColorBrush(Color.Parse("#54D18C"));
    private static readonly IBrush Warn = new SolidColorBrush(Color.Parse("#F2B84B"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#8E98AA"));

    public GraphVisualization? Visualization
    {
        get => GetValue(VisualizationProperty);
        set => SetValue(VisualizationProperty, value);
    }

    static GraphCanvas()
    {
        AffectsRender<GraphCanvas>(VisualizationProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width < 80 || height < 80)
        {
            return;
        }

        var visualization = Visualization;
        if (visualization is null || visualization.Vertices.Count == 0)
        {
            DrawCentered(context, "Постройте граф и запустите DFS", width / 2, height / 2, Muted, 13);
            return;
        }

        var vertices = visualization.Vertices;
        var count = vertices.Count;
        var center = new Point(width / 2, (height / 2) + 6);
        var radius = Math.Max(48, (Math.Min(width, height) / 2) - 44);
        var nodeRadius = count <= 8 ? 21 : count <= 16 ? 18 : 14;
        var positions = new Dictionary<int, Point>();

        for (var index = 0; index < count; index++)
        {
            var angle = (-Math.PI / 2) + (2 * Math.PI * index / count);
            positions[vertices[index]] = new Point(
                center.X + (radius * Math.Cos(angle)),
                center.Y + (radius * Math.Sin(angle)));
        }

        var edgePen = new Pen(Edge, 1.6);
        foreach (var (from, to) in visualization.Edges)
        {
            if (!positions.TryGetValue(from, out var fromPoint) || !positions.TryGetValue(to, out var toPoint))
            {
                continue;
            }

            if (from == to)
            {
                context.DrawEllipse(null, edgePen, new Point(fromPoint.X + 24, fromPoint.Y - 10), 15, 13);
                continue;
            }

            DrawEdge(context, edgePen, fromPoint, toPoint, nodeRadius, visualization.Directed);
        }

        var order = visualization.VisitOrder
            .Select((vertex, index) => (vertex, index))
            .ToDictionary(pair => pair.vertex, pair => pair.index + 1);

        foreach (var vertex in vertices)
        {
            var point = positions[vertex];
            var visited = order.TryGetValue(vertex, out var visitNumber);
            var isStart = vertex == visualization.Start;
            var fill = isStart ? NodeStart : visited ? NodeVisited : NodeFill;
            var ring = isStart ? Warn : visited ? Good : Accent;

            context.DrawEllipse(fill, new Pen(ring, isStart ? 2.4 : 1.8), point, nodeRadius, nodeRadius);
            DrawCentered(
                context,
                vertex.ToString(CultureInfo.InvariantCulture),
                point.X,
                point.Y,
                NodeText,
                count <= 16 ? 12 : 10,
                bold: true);

            if (visited)
            {
                DrawCentered(
                    context,
                    "→" + visitNumber.ToString(CultureInfo.InvariantCulture),
                    point.X,
                    point.Y + nodeRadius + 12,
                    Good,
                    10);
            }
        }

        DrawCentered(
            context,
            visualization.Directed ? "ориентированный граф" : "неориентированный граф",
            width / 2,
            18,
            Muted,
            11);
    }

    private static void DrawEdge(
        DrawingContext context,
        Pen pen,
        Point from,
        Point to,
        double nodeRadius,
        bool directed)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var length = Math.Sqrt((dx * dx) + (dy * dy));
        if (length <= (nodeRadius * 2) + 2)
        {
            return;
        }

        var ux = dx / length;
        var uy = dy / length;
        var start = new Point(from.X + (ux * nodeRadius), from.Y + (uy * nodeRadius));
        var end = new Point(to.X - (ux * nodeRadius), to.Y - (uy * nodeRadius));
        if (directed)
        {
            end = new Point(to.X - (ux * (nodeRadius + 8)), to.Y - (uy * (nodeRadius + 8)));
        }

        context.DrawLine(pen, start, end);

        if (directed)
        {
            DrawArrowHead(context, Accent, end, start);
        }
    }

    private static void DrawArrowHead(DrawingContext context, IBrush brush, Point tip, Point from)
    {
        var dx = tip.X - from.X;
        var dy = tip.Y - from.Y;
        var length = Math.Sqrt((dx * dx) + (dy * dy));
        if (length < 0.001)
        {
            return;
        }

        var ux = dx / length;
        var uy = dy / length;
        const double size = 7;
        var left = new Point(tip.X - (ux * size) - (uy * size * 0.55), tip.Y - (uy * size) + (ux * size * 0.55));
        var right = new Point(tip.X - (ux * size) + (uy * size * 0.55), tip.Y - (uy * size) - (ux * size * 0.55));
        var geometry = new StreamGeometry();
        using (var geometryContext = geometry.Open())
        {
            geometryContext.BeginFigure(tip, true);
            geometryContext.LineTo(left);
            geometryContext.LineTo(right);
            geometryContext.EndFigure(true);
        }

        context.DrawGeometry(brush, null, geometry);
    }

    private static void DrawCentered(
        DrawingContext context,
        string text,
        double x,
        double y,
        IBrush brush,
        double size,
        bool bold = false)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI", FontStyle.Normal, bold ? FontWeight.SemiBold : FontWeight.Normal),
            size,
            brush);
        context.DrawText(formatted, new Point(x - (formatted.Width / 2), y - (formatted.Height / 2)));
    }
}
