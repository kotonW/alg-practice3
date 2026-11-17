using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AlgorithmsLab3.App.Visualization;

namespace AlgorithmsLab3.App.Controls;

public sealed class CycleListCanvas : Control
{
    public static readonly StyledProperty<CycleVisualization?> VisualizationProperty =
        AvaloniaProperty.Register<CycleListCanvas, CycleVisualization?>(nameof(Visualization));

    private static readonly IBrush NodeFill = new SolidColorBrush(Color.Parse("#22304A"));
    private static readonly IBrush NodeFillCycle = new SolidColorBrush(Color.Parse("#254B45"));
    private static readonly IBrush NodeFillStart = new SolidColorBrush(Color.Parse("#2D5A3C"));
    private static readonly IBrush NodeFillMeeting = new SolidColorBrush(Color.Parse("#5A3F22"));
    private static readonly IBrush NodeText = new SolidColorBrush(Color.Parse("#F2F5FA"));
    private static readonly IBrush Edge = new SolidColorBrush(Color.Parse("#7486A0"));
    private static readonly IBrush Accent = new SolidColorBrush(Color.Parse("#5B9CFF"));
    private static readonly IBrush Good = new SolidColorBrush(Color.Parse("#54D18C"));
    private static readonly IBrush Warn = new SolidColorBrush(Color.Parse("#F2B84B"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#8E98AA"));

    public CycleVisualization? Visualization
    {
        get => GetValue(VisualizationProperty);
        set => SetValue(VisualizationProperty, value);
    }

    static CycleListCanvas()
    {
        AffectsRender<CycleListCanvas>(VisualizationProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width < 60 || height < 60)
        {
            return;
        }

        var visualization = Visualization;
        if (visualization is null || visualization.Nodes.Count == 0)
        {
            DrawCentered(context, "Постройте список, чтобы увидеть узлы", width / 2, height / 2, Muted, 13);
            return;
        }

        var nodes = visualization.Nodes;
        var count = nodes.Count;
        const double margin = 44;
        const double nodeRadius = 19;
        var top = Math.Max(54, height * 0.28);
        var usable = Math.Max(1, width - (margin * 2));
        var step = count <= 1 ? 0 : Math.Min(102, usable / (count - 1));
        var startX = count <= 1 ? width / 2 : margin;
        if (count > 1 && step * (count - 1) > usable)
        {
            step = usable / (count - 1);
        }

        var centers = new Point[count];
        for (var index = 0; index < count; index++)
        {
            centers[index] = new Point(startX + (step * index), top);
        }

        DrawCentered(context, "голова", centers[0].X, top - nodeRadius - 20, Muted, 11);

        var edgePen = new Pen(Edge, 1.8);

        // Ссылки между соседними узлами.
        for (var index = 0; index < count - 1; index++)
        {
            var from = centers[index];
            var to = centers[index + 1];
            DrawArrow(context, edgePen, from, to, nodeRadius, nodeRadius, Accent);
        }

        if (visualization.CycleStartIndex is { } cycleStart && cycleStart >= 0 && cycleStart < count)
        {
            var tail = centers[^1];
            var target = centers[cycleStart];
            var loopY = Math.Min(height - 38, top + Math.Max(86, nodeRadius * 2.8));
            var loopPen = new Pen(Good, 2);

            if (cycleStart == count - 1)
            {
                // Петля из последнего узла в себя.
                context.DrawEllipse(null, loopPen, new Point(tail.X + 26, tail.Y + 34), 24, 20);
                DrawArrowHead(context, Good, new Point(tail.X + 48, tail.Y + 18), new Point(tail.X + 19, tail.Y + 19));
            }
            else
            {
                var geometry = new StreamGeometry();
                using (var geometryContext = geometry.Open())
                {
                    geometryContext.BeginFigure(new Point(tail.X, tail.Y + nodeRadius), false);
                    geometryContext.LineTo(new Point(tail.X, loopY));
                    geometryContext.LineTo(new Point(target.X, loopY));
                    geometryContext.LineTo(new Point(target.X, target.Y + nodeRadius + 5));
                }

                context.DrawGeometry(null, loopPen, geometry);
                DrawArrowHead(
                    context,
                    Good,
                    new Point(target.X, target.Y + nodeRadius + 5),
                    new Point(target.X, target.Y + nodeRadius + 2));
            }

            DrawCentered(context, "цикл", (tail.X + target.X) / 2, loopY - 13, Good, 11);
        }
        else
        {
            var tail = centers[^1];
            var end = new Point(Math.Min(width - 26, tail.X + Math.Max(48, step)), tail.Y);
            DrawArrow(context, edgePen, tail, end, nodeRadius, 0, Muted);
            DrawCentered(context, "null", end.X + 12, end.Y, Muted, 11);
        }

        for (var index = 0; index < count; index++)
        {
            var center = centers[index];
            var inCycle = visualization.CycleStartIndex is { } start && index >= start;
            var fill = inCycle ? NodeFillCycle : NodeFill;
            var ring = inCycle ? Good : Accent;

            if (visualization.CycleStartIndex == index)
            {
                fill = NodeFillStart;
            }

            if (visualization.MeetingIndex == index)
            {
                fill = NodeFillMeeting;
            }

            context.DrawEllipse(fill, new Pen(ring, 2), center, nodeRadius, nodeRadius);
            DrawCentered(context, nodes[index].Value.ToString(CultureInfo.InvariantCulture), center.X, center.Y, NodeText, 13, bold: true);
            DrawCentered(context, "#" + index.ToString(CultureInfo.InvariantCulture), center.X, center.Y + nodeRadius + 13, Muted, 10);
        }

        if (visualization.MeetingIndex is { } meeting)
        {
            var center = centers[meeting];
            DrawCentered(context, "встреча", center.X, center.Y - nodeRadius - 15, Warn, 10);
        }

        if (visualization.CycleStartIndex is { } cycle)
        {
            var center = centers[cycle];
            DrawCentered(context, "вход в цикл", center.X, center.Y - nodeRadius - 15, Good, 10);
        }
    }

    private static void DrawArrow(
        DrawingContext context,
        Pen pen,
        Point from,
        Point to,
        double fromRadius,
        double toRadius,
        IBrush headBrush)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var length = Math.Sqrt((dx * dx) + (dy * dy));
        if (length <= fromRadius + toRadius + 4)
        {
            return;
        }

        var ux = dx / length;
        var uy = dy / length;
        var start = new Point(from.X + (ux * fromRadius), from.Y + (uy * fromRadius));
        var end = new Point(to.X - (ux * (toRadius + 7)), to.Y - (uy * (toRadius + 7)));
        context.DrawLine(pen, start, end);
        DrawArrowHead(context, headBrush, end, start);
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
        var size = 7.5;
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
