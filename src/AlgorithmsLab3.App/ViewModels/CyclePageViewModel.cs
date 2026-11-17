using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using AlgorithmsLab3.Domain.Applications;
using AlgorithmsLab3.App.Visualization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmsLab3.App.ViewModels;

public partial class CyclePageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _valuesText = "3 1 4 1 5 9 2 6";

    [ObservableProperty]
    private bool _createCycle = true;

    [ObservableProperty]
    private string _cycleIndexText = "3";

    [ObservableProperty]
    private string _resultTitle = "Построение списка";

    [ObservableProperty]
    private string _resultDetails = "Нажмите «Построить и найти цикл».";

    [ObservableProperty]
    private string _status = "Введите значения через пробел. Индекс узла для замыкания — с нуля.";

    [ObservableProperty]
    private string _listDescription = string.Empty;

    [ObservableProperty]
    private CycleVisualization? _visualization;

    public CyclePageViewModel()
    {
        BuildAndDetect();
    }

    public ObservableCollection<FloydStep> Trace { get; } = [];

    [RelayCommand]
    private void BuildAndDetect()
    {
        try
        {
            var values = ParseValues(ValuesText);
            if (values.Count == 0)
            {
                throw new FormatException("Нужно хотя бы одно целое число.");
            }

            if (values.Count > 64)
            {
                throw new FormatException("Для визуализации используйте не больше 64 узлов.");
            }

            int? cycleTo = null;
            if (CreateCycle)
            {
                if (!int.TryParse(CycleIndexText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var index))
                {
                    throw new FormatException("Индекс узла для цикла должен быть целым числом.");
                }

                if (index < 0 || index >= values.Count)
                {
                    throw new FormatException($"Индекс должен быть от 0 до {values.Count - 1}.");
                }

                cycleTo = index;
            }

            var list = new CycleLinkedList();
            list.Build(values, cycleTo);
            var result = list.Detect();

            Trace.Clear();
            foreach (var step in result.Trace)
            {
                Trace.Add(step);
            }

            Visualization = new CycleVisualization(
                values.Select((value, index) => new CycleNodeVisual(index, value)).ToList(),
                result.CycleStartIndex,
                result.MeetingIndex,
                result.HasCycle);

            ListDescription = $"Узлов: {values.Count}. Список: [{string.Join(", ", values)}]"
                + (cycleTo is { } cycle ? $". Хвост замкнут на узел #{cycle}." : ". Список линейный.");

            if (result.HasCycle)
            {
                ResultTitle = "Цикл обнаружен";
                ResultDetails =
                    $"Вход в цикл: узел #{result.CycleStartIndex} (значение {result.CycleStartValue})\n"
                    + $"Длина цикла: {result.CycleLength}\n"
                    + $"Место встречи: узел #{result.MeetingIndex} (значение {result.MeetingValue})\n"
                    + $"Итераций пары указателей: {result.Iterations}\n"
                    + "Сложность: O(n) времени, O(1) дополнительной памяти.";
                Status = "Черепаха и заяц встретились — цикл есть.";
            }
            else
            {
                ResultTitle = "Цикла нет";
                ResultDetails =
                    $"Заяц дошёл до конца за {result.Iterations} итераций.\n"
                    + $"Проверено узлов: {result.NodeCount}\n"
                    + "Сложность: O(n) времени, O(1) дополнительной памяти.";
                Status = "Заяц достиг null — список линейный.";
            }
        }
        catch (Exception exception)
        {
            ResultTitle = "Ошибка";
            ResultDetails = exception.Message;
            Status = "Исправьте входные данные.";
        }
    }

    [RelayCommand]
    private void RandomExample()
    {
        var random = new Random();
        var count = random.Next(8, 17);
        var values = Enumerable.Range(0, count).Select(_ => random.Next(1, 100)).ToList();
        ValuesText = string.Join(' ', values);
        CreateCycle = true;
        CycleIndexText = random.Next(0, count).ToString(CultureInfo.CurrentCulture);
        BuildAndDetect();
        Status = "Сгенерирован случайный список с циклом.";
    }

    [RelayCommand]
    private void LinearExample()
    {
        ValuesText = "10 20 30 40 50 60 70 80";
        CreateCycle = false;
        BuildAndDetect();
        Status = "Построен линейный список без цикла.";
    }

    private static IReadOnlyList<int> ParseValues(string text)
    {
        var matches = Regex.Matches(text, @"-?\d+");
        var values = new List<int>(matches.Count);
        foreach (Match match in matches)
        {
            if (!int.TryParse(match.Value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var value))
            {
                throw new FormatException($"Не удалось прочитать число «{match.Value}».");
            }

            values.Add(value);
        }

        return values;
    }
}
