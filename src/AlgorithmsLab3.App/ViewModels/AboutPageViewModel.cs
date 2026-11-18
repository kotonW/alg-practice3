using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgorithmsLab3.App.ViewModels;

public sealed class AboutPageViewModel : ObservableObject
{
    public string Title => "Практическое занятие №3";

    public string Subtitle => "Динамические структуры данных на C# и Avalonia";

    public IReadOnlyList<ComplexityRow> ComplexityRows { get; } =
    [
        new("Стек на связном списке", "Push / Pop / Top / isEmpty", "O(1)", "O(n)"),
        new("Print стека", "Обход всех узлов сверху вниз", "O(n)", "O(1)"),
        new("Постфиксное выражение", "Один проход по токенам", "O(n)", "O(n)"),
        new("Инфикс → постфикс", "Сортировочная станция Дейкстры", "O(n)", "O(n)"),
        new("Очередь на списке", "Вставка в хвост / удаление из головы", "O(1)", "O(1)"),
        new("Очередь Queue<T>", "Стандартная реализация .NET", "O(1)*", "O(n)"),
        new("Флойд: цикл в списке", "Черепаха и заяц, два указателя", "O(n)", "O(1)"),
        new("DFS на стеке", "Итеративный обход графа", "O(V + E)", "O(V)"),
        new("BFS на очереди", "Обход в ширину, кратчайшие пути", "O(V + E)", "O(V)"),
        new("BST-множество", "Вставка / поиск / удаление", "O(h), в среднем O(log n)", "O(n)"),
        new("Переворот списка", "Изменение ссылок на месте", "O(n)", "O(1)"),
        new("Удаление неуникальных", "Подсчёт частот + перестройка", "O(n)", "O(n)"),
        new("Разбиение списка", "По первому вхождению числа", "O(n)", "O(n)"),
    ];
}

public sealed record ComplexityRow(string Structure, string Operation, string Time, string Memory);
