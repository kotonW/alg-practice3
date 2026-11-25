using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgorithmsLab3.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private NavigationItem _selectedItem;

    [ObservableProperty]
    private bool _isDarkTheme = true;

    public MainWindowViewModel()
    {
        var stack = new StackPageViewModel();
        var queue = new QueuePageViewModel();
        var cycle = new CyclePageViewModel();
        var dfs = new DfsPageViewModel();
        var bfs = new BfsPageViewModel();
        var bst = new BstPageViewModel();
        var lists = new LinkedListPageViewModel();
        var postfixTree = new PostfixTreePageViewModel();

        Items =
        [
            new NavigationItem
            {
                Badge = "1",
                Title = "Стек",
                Subtitle = "Свой связный список · команды · постфикс",
                Content = stack,
            },
            new NavigationItem
            {
                Badge = "2",
                Title = "Очередь",
                Subtitle = "Свой список против Queue<T> · замеры",
                Content = queue,
            },
            new NavigationItem
            {
                Badge = "3",
                Title = "Цикл в списке",
                Subtitle = "Флойд: черепаха и заяц",
                Content = cycle,
            },
            new NavigationItem
            {
                Badge = "3",
                Title = "DFS на стеке",
                Subtitle = "Итеративный обход графа",
                Content = dfs,
            },
            new NavigationItem
            {
                Badge = "3",
                Title = "BFS на очереди",
                Subtitle = "Обход графа в ширину",
                Content = bfs,
            },
            new NavigationItem
            {
                Badge = "3",
                Title = "BST-множество",
                Subtitle = "Двоичное дерево поиска",
                Content = bst,
            },
            new NavigationItem
            {
                Badge = "4",
                Title = "Операции со списком",
                Subtitle = "12 алгоритмов над связным списком",
                Content = lists,
            },
            new NavigationItem
            {
                Badge = "5",
                Title = "Бинарное дерево",
                Subtitle = "Построение из постфиксной записи",
                Content = postfixTree,
            },
        ];

        _selectedItem = Items[0];
    }

    public IReadOnlyList<NavigationItem> Items { get; }

    partial void OnIsDarkThemeChanged(bool value)
    {
        if (Application.Current is { } application)
        {
            application.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }
}
