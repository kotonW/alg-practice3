using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AlgorithmsLab3.Domain.Applications;
using AlgorithmsLab3.App.Visualization;

namespace AlgorithmsLab3.App.Controls;

public sealed class PostfixTreeCanvas : Control
{
    public static readonly StyledProperty<PostfixTreeSnapshot?> SnapshotProperty =
        AvaloniaProperty.Register<PostfixTreeCanvas, PostfixTreeSnapshot?>(nameof(Snapshot));

    private static readonly IBrush NodeFill = new SolidColorBrush(Color.Parse("#22304A"));
    private static readonly IBrush NodeBorder = new SolidColorBrush(Color.Parse("#5B9CFF"));
    private static readonly IBrush NodeText = new SolidColorBrush(Color.Parse("#F2F5FA"));
    private static readonly IBrush Edge = new SolidColorBrush(Color.Parse("#61718A"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#8E98AA"));

    public PostfixTreeSnapshot? Snapshot
    {
        get => GetValue(SnapshotProperty);
        set => SetValue(SnapshotProperty, value);
    }

    static PostfixTreeCanvas()
    {
        AffectsRender<PostfixTreeCanvas>(SnapshotProperty);
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
            DrawCentered(context, "Введите запись дерева и постройте дерево", width / 2, height / 2, Muted, 13);
            return;
        }

        var layout = new List<NodeLayout>();
        var nextLeaf = 0;
        Layout(snapshot.Root, 0, ref nextLeaf, layout);

        var labels = new Dictionary<PostfixNode, NodeLabel>();
        foreach (var item in layout)
        {
            labels[item.Node] = FitLabel(item.Node.Label);
        }

        var count = layout.Count;
        var maxBoxWidth = labels.Values.Max(label => label.Width);
        const double margin = 26;
        var usableWidth = Math.Max(10, width - (margin * 2));
        var span = Math.Max(1, count - 1);
        var step = Math.Min(Math.Max(66, maxBoxWidth + 18), usableWidth / span);
        var startX = (width - (step * (count - 1))) / 2;

        var maxDepth = layout.Max(item => item.Depth) + 1;
        var rowGap = maxDepth <= 1 ? 0 : Math.Min(86, (height - 96) / (maxDepth - 1));
        rowGap = Math.Max(46, rowGap);
        const double top = 52;
        var positions = new Dictionary<PostfixNode, Point>();
        foreach (var item in layout)
        {
            positions[item.Node] = new Point(startX + (step * item.Index), top + (rowGap * item.Depth));
        }

        var edgePen = new Pen(Edge, 1.5);
        foreach (var item in layout)
        {
            if (item.Node.Left is not null && positions.TryGetValue(item.Node.Left, out var left))
            {
                context.DrawLine(edgePen, positions[item.Node], left);
            }

            if (item.Node.Right is not null && positions.TryGetValue(item.Node.Right, out var right))
            {
                context.DrawLine(edgePen, positions[item.Node], right);
            }
        }

        const double boxHeight = 32;
        foreach (var item in layout)
        {
            var label = labels[item.Node];
            var point = positions[item.Node];
            var rect = new Rect(
                point.X - (label.Width / 2),
                point.Y - (boxHeight / 2),
                label.Width,
                boxHeight);
            context.DrawRectangle(NodeFill, new Pen(NodeBorder, 1.6), rect, 9, 9);
            DrawCentered(context, label.Text, point.X, point.Y, NodeText, label.FontSize, bold: true);
        }

        DrawCentered(
            context,
            $"узлов: {snapshot.Count} · высота: {snapshot.Height} · листьев: {snapshot.Leaves}",
            width / 2,
            20,
            Muted,
            11);
    }

    private static void Layout(PostfixNode node, int depth, ref int nextLeaf, List<NodeLayout> result)
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

    private static NodeLabel FitLabel(string label)
    {
        const double maxWidth = 190;
        const double baseSize = 12.5;
        var text = label;
        var size = baseSize;
        var formatted = Measure(text, size);
        if (formatted.Width > maxWidth)
        {
            size = Math.Max(8, baseSize * maxWidth / formatted.Width);
            formatted = Measure(text, size);
        }

        while (formatted.Width > maxWidth && text.Length > 1)
        {
            text = text[..^1];
            formatted = Measure(text + "…", size);
        }

        if (!string.Equals(text, label, StringComparison.Ordinal))
        {
            text += "…";
            formatted = Measure(text, size);
        }

        return new NodeLabel(text, size, Math.Max(34, formatted.Width + 18));
    }

    private static FormattedText Measure(string text, double size) =>
        new(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI", FontStyle.Normal, FontWeight.SemiBold),
            size,
            Brushes.White);

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

    private readonly record struct NodeLayout(PostfixNode Node, int Index, int Depth);

    private readonly record struct NodeLabel(string Text, double FontSize, double Width);
}
