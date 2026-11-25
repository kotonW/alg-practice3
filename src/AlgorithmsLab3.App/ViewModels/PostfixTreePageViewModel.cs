using AlgorithmsLab3.Domain.Applications;
using AlgorithmsLab3.App.Visualization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmsLab3.App.ViewModels;

public partial class PostfixTreePageViewModel : ObservableObject
{
    private static readonly string[] Examples =
    [
        "5 3 1 * * 4 * * 8 * *",
        "K A * * B * *",
        "корень левое * * правое * *",
    ];

    private int _exampleIndex = 1;

    [ObservableProperty]
    private string _recordText = "5 3 1 * * 4 * * 8 * *";

    [ObservableProperty]
    private string _status = "Введите запись дерева и постройте дерево.";

    [ObservableProperty]
    private string _stats = "—";

    [ObservableProperty]
    private PostfixTreeSnapshot? _snapshot;

    public PostfixTreePageViewModel() => Build();

    [RelayCommand]
    private void Build()
    {
        try
        {
            var tree = PostfixTree.Parse(RecordText);
            Snapshot = new PostfixTreeSnapshot(tree.Root, tree.Count, tree.Height, tree.Leaves);
            Stats = $"Узлов: {tree.Count} · высота: {tree.Height} · листьев: {tree.Leaves} · "
                    + $"корень: {tree.Root.Label}";
            Status = "Дерево построено.";
        }
        catch (FormatException exception)
        {
            Snapshot = null;
            Stats = "—";
            Status = exception.Message;
        }
    }

    [RelayCommand]
    private void Example()
    {
        RecordText = Examples[_exampleIndex % Examples.Length];
        _exampleIndex++;
        Build();
    }

    [RelayCommand]
    private void Clear()
    {
        RecordText = string.Empty;
        Snapshot = null;
        Stats = "—";
        Status = "Поле очищено.";
    }
}
