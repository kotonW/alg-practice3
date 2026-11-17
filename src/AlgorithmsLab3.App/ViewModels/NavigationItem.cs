namespace AlgorithmsLab3.App.ViewModels;

public sealed class NavigationItem
{
    public required string Badge { get; init; }

    public required string Title { get; init; }

    public required string Subtitle { get; init; }

    public required object Content { get; init; }
}
