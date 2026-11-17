using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AlgorithmsLab3.Domain.Applications;
using AlgorithmsLab3.App.Visualization;

namespace AlgorithmsLab3.App.Controls;

public sealed class TreeCanvas : Control
{
    public static readonly StyledProperty<BstSnapshot?> SnapshotProperty =
        AvaloniaProperty.Register<TreeCanvas, BstSnapshot?>(nameof(Snapshot));

    private static readonly IBrush NodeFill = new SolidColorBrush(Color.Parse("#22304A"));
    private static readonly IBrush NodeHighlight = new SolidColorBrush(Color.Parse("#6A4A1F"));
    private static readonly IBrush NodeText = new SolidColorBrush(Color.Parse("#F2F5FA"));
    private static readonly IBrush Edge = new SolidColorBrush(Color.Parse("#61718A"));
    private static readonly IBrush Accent = new SolidColorBrush(Color.Parse("#5B9CFF"));
    private static readonly IBrush Warn = new SolidColorBrush(Color.Parse("#F2B84B"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#8E98AA"));

    public BstSnapshot? Snapshot
    {
        get => GetValue(SnapshotProperty);
        set => SetValue(SnapshotProperty, value);
    }

    static TreeCanvas()
    {
        AffectsRender<TreeCanvas>(SnapshotProperty);
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

        var snapshot = Snapshot;
        if (snapshot?.Root is null || snapshot.Count == 0)
        {
            DrawCentered(context, "Дерево пусто — вставьте значение", width / 2, height / 2, Muted, 13);
            return;
        }

        var layout = new List<NodeLayout>();
        var nextLeaf = 0;
        Layout(snapshot.Root, 0, ref nextLeaf, layout);

        var count = layout.Count;
        var maxDepth = layout.Max(item => item.Depth) + 1;
        const double margin = 30;
        var usableWidth = Math.Max(10, width - (margin * 2));
        var span = Math.Max(1, count - 1);
        var step = Math.Min(84, usableWidth / span);
        var startX = (width - (step * span)) / 2;

        var rowGap = maxDepth <= 1 ? 0 : Math.Min(88, (height - 92) / (maxDepth - 1));
        rowGap = Math.Max(38, rowGap);
        var top = 52;
        var radius = Math.Clamp(Math.Min(step, rowGap) * 0.30, 9, 20);
        var positions = new Dictionary<BstNode, Point>();
        foreach (var item in layout)
        {
            positions[item.Node] = new Point(startX + (step * item.Index), top + (rowGap * item.Depth));
        }

        var edgePen = new Pen(Edge, 1.5);
        foreach (var item in layout)
        {
            if (item.Node.Left is not null && positions.TryGetValue(item.Node.Left, out var left))
            {
                DrawEdge(context, edgePen, positions[item.Node], left, radius);
            }

            if (item.Node.Right is not null && positions.TryGetValue(item.Node.Right, out var right))
            {
                DrawEdge(context, edgePen, positions[item.Node], right, radius);
            }
        }

        var fontSize = Math.Clamp(radius * 0.82, 8, 14);
        foreach (var item in layout)
        {
            var point = positions[item.Node];
            var highlighted = snapshot.HighlightValue == item.Node.Value;
            context.DrawEllipse(
                highlighted ? NodeHighlight : NodeFill,
                new Pen(highlighted ? Warn : Accent, highlighted ? 2.4 : 1.6),
                point,
                radius,
                radius);

            DrawCentered(
                context,
                item.Node.Value.ToString(CultureInfo.InvariantCulture),
                point.X,
                point.Y,
                NodeText,
                fontSize,
                bold: true);
        }

        DrawCentered(
            context,
            $"узлов: {snapshot.Count} · высота: {snapshot.Height} · обход in-order даёт возрастающий ряд",
            width / 2,
            20,
            Muted,
            11);
    }

    private static void Layout(BstNode node, int depth, ref int nextLeaf, List<NodeLayout> result)
    {
        if (node.Left is not null)
        {
            Layout(node.Left, depth + 1, ref nextLeaf, result);
        }

        result.Add(new NodeLayout(node, nextLeaf, depth));
        nextLeaf++;

        if (node.Right is not null)
        {
            Layout(node.Right, depth + 1, ref nextLeaf, result);
        }
    }

    private static void DrawEdge(DrawingContext context, Pen pen, Point parent, Point child, double radius)
    {
        var dx = child.X - parent.X;
        var dy = child.Y - parent.Y;
        var length = Math.Sqrt((dx * dx) + (dy * dy));
        if (length <= radius * 2)
        {
            return;
        }

        var ux = dx / length;
        var uy = dy / length;
        context.DrawLine(
            pen,
            new Point(parent.X + (ux * radius), parent.Y + (uy * radius)),
            new Point(child.X - (ux * radius), child.Y - (uy * radius)));
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

    private readonly record struct NodeLayout(BstNode Node, int Index, int Depth);
}
