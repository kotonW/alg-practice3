using AlgorithmsLab3.Domain.Applications;

namespace AlgorithmsLab3.App.Visualization;

public sealed record CycleNodeVisual(int Index, int Value);

public sealed record CycleVisualization(
    IReadOnlyList<CycleNodeVisual> Nodes,
    int? CycleStartIndex,
    int? MeetingIndex,
    bool HasCycle);

public sealed record GraphVisualization(
    IReadOnlyList<int> Vertices,
    IReadOnlyList<(int From, int To)> Edges,
    IReadOnlyList<int> VisitOrder,
    int Start,
    bool Directed);

public sealed record BstSnapshot(BstNode? Root, int Count, int Height, int? HighlightValue);

public sealed record PostfixTreeSnapshot(PostfixNode? Root, int Count, int Height, int Leaves);
