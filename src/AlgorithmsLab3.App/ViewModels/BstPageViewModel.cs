using System.Globalization;
using AlgorithmsLab3.Domain.Applications;
using AlgorithmsLab3.App.Visualization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmsLab3.App.ViewModels;

public sealed record TraversalOption(TreeTraversal Value, string Title);

public partial class BstPageViewModel : ObservableObject
{
    private readonly BinarySearchTree _tree = new();

    [ObservableProperty]
    private string _valueText = string.Empty;

    [ObservableProperty]
    private string _status = "Введите целое число и выберите операцию.";

    [ObservableProperty]
    private string _stats = string.Empty;

    [ObservableProperty]
    private string _traversalText = string.Empty;

    [ObservableProperty]
    private TraversalOption _selectedTraversalOption;

    [ObservableProperty]
    private BstSnapshot? _snapshot;

    public BstPageViewModel()
    {
        TraversalOptions =
        [
            new TraversalOption(TreeTraversal.InOrder, "Симметричный (in-order)"),
            new TraversalOption(TreeTraversal.PreOrder, "Прямой (pre-order)"),
            new TraversalOption(TreeTraversal.PostOrder, "Обратный (post-order)"),
            new TraversalOption(TreeTraversal.LevelOrder, "По уровням (BFS)"),
        ];

        _selectedTraversalOption = TraversalOptions[0];
        _tree.InsertRange([50, 30, 70, 20, 40, 60, 80, 35, 65]);
        UpdateState();
        Status = "Загружено демонстрационное дерево. Дубликаты в множество не добавляются.";
    }

    public IReadOnlyList<TraversalOption> TraversalOptions { get; }

    [RelayCommand]
    private void Insert()
    {
        if (!TryParseValue(out var value))
        {
            return;
        }

        var inserted = _tree.Insert(value);
        UpdateState(inserted ? value : null);
        Status = inserted
            ? $"Вставлено значение {value}."
            : $"Значение {value} уже есть в множестве — дубликат не добавлен.";
    }

    [RelayCommand]
    private void Remove()
    {
        if (!TryParseValue(out var value))
        {
            return;
        }

        var removed = _tree.Remove(value);
        UpdateState();
        Status = removed
            ? $"Удалено значение {value}. При удалении узла с двумя детьми используется преемник."
            : $"Значение {value} не найдено.";
    }

    [RelayCommand]
    private void Contains()
    {
        if (!TryParseValue(out var value))
        {
            return;
        }

        var contains = _tree.Contains(value);
        UpdateState(contains ? value : null);
        Status = contains
            ? $"Значение {value} присутствует в множестве."
            : $"Значение {value} отсутствует.";
    }

    [RelayCommand]
    private void Min()
    {
        var value = _tree.Min();
        Status = value is null ? "Дерево пусто." : $"Минимальный элемент: {value} (самый левый узел).";
        UpdateState(value);
    }

    [RelayCommand]
    private void Max()
    {
        var value = _tree.Max();
        Status = value is null ? "Дерево пусто." : $"Максимальный элемент: {value} (самый правый узел).";
        UpdateState(value);
    }

    [RelayCommand]
    private void Clear()
    {
        _tree.Clear();
        UpdateState();
        Status = "Множество очищено.";
    }

    [RelayCommand]
    private void RandomTree()
    {
        var random = new Random();
        var values = new HashSet<int>();
        var target = random.Next(12, 20);
        while (values.Count < target)
        {
            values.Add(random.Next(1, 100));
        }

        _tree.Clear();
        _tree.InsertRange(values);
        UpdateState();
        Status = $"Построено случайное дерево из {values.Count} различных значений.";
    }

    [RelayCommand]
    private void RunTraversal() => UpdateTraversal();

    partial void OnSelectedTraversalOptionChanged(TraversalOption value) => UpdateTraversal();

    private bool TryParseValue(out int value)
    {
        if (!int.TryParse(ValueText.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
        {
            Status = "Введите целое число.";
            return false;
        }

        return true;
    }

    private void UpdateState(int? highlight = null)
    {
        Snapshot = new BstSnapshot(_tree.Root, _tree.Count, _tree.Height(), highlight);
        Stats = _tree.Count == 0
            ? "Дерево пусто."
            : $"Узлов: {_tree.Count} · высота: {_tree.Height()} · минимум: {_tree.Min()} · "
              + $"максимум: {_tree.Max()} · инвариант BST: {(_tree.IsValid() ? "выполняется" : "нарушен")}";
        UpdateTraversal();
    }

    private void UpdateTraversal()
    {
        TraversalText = _tree.Count == 0
            ? "—"
            : string.Join("  ", _tree.Traverse(SelectedTraversalOption.Value));
    }
}
