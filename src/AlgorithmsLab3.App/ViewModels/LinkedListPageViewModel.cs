using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using AlgorithmsLab3.Domain.Lists;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmsLab3.App.ViewModels;

public sealed record LinkedListLogEntry(int Number, string Operation, string Details, bool IsError);

public sealed record ListValueChip(int Value);

public partial class LinkedListPageViewModel : ObservableObject
{
    private IntLinkedList _list = IntLinkedList.FromValues([5, 3, 8, 3, 1, 9, 5, 2]);
    private int _logCounter;

    [ObservableProperty]
    private string _valuesText = "5 3 8 3 1 9 5 2";

    [ObservableProperty]
    private string _secondListText = "10 20 30";

    [ObservableProperty]
    private string _elementEText = "3";

    [ObservableProperty]
    private string _elementFText = "7";

    [ObservableProperty]
    private string _xText = "5";

    [ObservableProperty]
    private string _sortedValueText = "4";

    [ObservableProperty]
    private string _firstPositionText = "1";

    [ObservableProperty]
    private string _secondPositionText = "3";

    [ObservableProperty]
    private string _currentList = "[]";

    [ObservableProperty]
    private string _countText = "0";

    [ObservableProperty]
    private string _status = "Список загружен. Все операции изменяют ссылки собственного связного списка.";

    [ObservableProperty]
    private string _splitFirst = "—";

    [ObservableProperty]
    private string _splitSecond = "—";

    public LinkedListPageViewModel()
    {
        UpdateDisplay();
    }

    public ObservableCollection<ListValueChip> Items { get; } = [];

    public ObservableCollection<LinkedListLogEntry> Log { get; } = [];

    public string Hint =>
        "Все значения — целые числа. Позиции для Swap — с 1. "
        + "Split показывает два новых списка и не изменяет текущий.";

    [RelayCommand]
    private void ApplyValues()
    {
        try
        {
            var values = ParseIntList(ValuesText);
            if (values.Count == 0)
            {
                throw new FormatException("Введите хотя бы одно целое число.");
            }

            _list.Replace(values);
            UpdateDisplay();
            AddLog("Загрузить список", $"Создан список из {_list.Count} узлов.", false);
            Status = "Список создан заново из введённых значений.";
        }
        catch (Exception exception)
        {
            Status = "Ошибка: " + exception.Message;
            AddLog("Загрузить список", exception.Message, true);
        }
    }

    [RelayCommand]
    private void LoadSample()
    {
        ValuesText = "5 3 8 3 1 9 5 2";
        SecondListText = "10 20 30";
        ElementEText = "3";
        ElementFText = "7";
        XText = "5";
        SortedValueText = "4";
        FirstPositionText = "1";
        SecondPositionText = "3";
        _list = IntLinkedList.FromValues([5, 3, 8, 3, 1, 9, 5, 2]);
        SplitFirst = "—";
        SplitSecond = "—";
        UpdateDisplay();
        AddLog("Демо-список", "Загружен список [5, 3, 8, 3, 1, 9, 5, 2].", false);
        Status = "Демонстрационный список загружен.";
    }

    [RelayCommand]
    private void GenerateRandom()
    {
        var random = new Random();
        var count = random.Next(10, 17);
        var values = Enumerable.Range(0, count).Select(_ => random.Next(1, 10)).ToList();
        ValuesText = string.Join(' ', values);
        _list = IntLinkedList.FromValues(values);
        UpdateDisplay();
        AddLog("Случайный список", $"Создано {count} значений из диапазона 1…9 (есть повторы).", false);
        Status = "Сгенерирован случайный список с повторяющимися значениями.";
    }

    [RelayCommand]
    private void Reverse() => Perform("Перевернуть список", () =>
    {
        if (!_list.ReverseInPlace())
        {
            return "Недостаточно элементов: переворот не требуется.";
        }

        return "Ссылки перестроены в обратном порядке.";
    });

    [RelayCommand]
    private void MoveLastToFront() => Perform("Последний → начало", () =>
    {
        if (!_list.MoveLastToFront())
        {
            return "Недостаточно элементов: операция не изменила список.";
        }

        return "Последний узел перенесён в начало.";
    });

    [RelayCommand]
    private void MoveFirstToEnd() => Perform("Первый → конец", () =>
    {
        if (!_list.MoveFirstToEnd())
        {
            return "Недостаточно элементов: операция не изменила список.";
        }

        return "Первый узел перенесён в конец.";
    });

    [RelayCommand]
    private void CountDistinct() => Perform("Различные элементы", () =>
    {
        var distinct = _list.CountDistinct();
        return $"Различных значений: {distinct} из {_list.Count} узлов.";
    });

    [RelayCommand]
    private void RemoveNonUnique() => Perform("Удалить неуникальные", () =>
    {
        var removed = _list.RemoveNonUnique();
        return $"Удалено {removed} узлов; остались только значения, встречающиеся ровно один раз.";
    });

    [RelayCommand]
    private void InsertSelfAfterX() => Perform("Вставить список в себя", () =>
    {
        var x = ParseInt(XText, "x");
        var inserted = _list.InsertSelfAfter(x);
        return inserted == 0
            ? $"Значение {x} не найдено — вставка не выполнена."
            : $"После первого вхождения {x} вставлена копия списка: добавлено {inserted} узлов.";
    });

    [RelayCommand]
    private void InsertSorted() => Perform("Вставка по порядку", () =>
    {
        var value = ParseInt(SortedValueText, "значение E");
        var wasSorted = IsSortedNonDecreasing(_list.ToList());
        _list.InsertSorted(value);
        return wasSorted
            ? $"Значение {value} вставлено с сохранением неубывающего порядка."
            : $"Значение {value} вставлено по правилу поиска, но исходный список не был отсортирован.";
    });

    [RelayCommand]
    private void RemoveAllE() => Perform("Удалить все E", () =>
    {
        var e = ParseInt(ElementEText, "E");
        var removed = _list.RemoveAll(e);
        return removed == 0
            ? $"Значение {e} не встречалось."
            : $"Удалено вхождений {e}: {removed}.";
    });

    [RelayCommand]
    private void InsertBeforeE() => Perform("Вставить F перед E", () =>
    {
        var e = ParseInt(ElementEText, "E");
        var f = ParseInt(ElementFText, "F");
        var inserted = _list.InsertBefore(e, f);
        return inserted
            ? $"Значение {f} вставлено перед первым вхождением {e}."
            : $"Значение {e} не найдено — вставка не выполнена.";
    });

    [RelayCommand]
    private void AppendSecondList() => Perform("Дописать список E", () =>
    {
        var values = ParseIntList(SecondListText);
        if (values.Count == 0)
        {
            throw new FormatException("Второй список пуст.");
        }

        var other = IntLinkedList.FromValues(values);
        var appended = _list.Append(other);
        return $"Дописано {appended} узлов из второго списка.";
    });

    [RelayCommand]
    private void SplitByX() => Perform("Разбить по x", () =>
    {
        var x = ParseInt(XText, "x");
        var (first, second) = _list.SplitByFirst(x);
        SplitFirst = first.Describe(80);
        SplitSecond = second.Describe(80);
        return second.IsEmpty
            ? $"Значение {x} не найдено: второй список пуст, первый не изменился."
            : $"Первый список — элементы до {x}, второй — начиная с {x}.";
    });

    [RelayCommand]
    private void DoubleList() => Perform("Удвоить список", () =>
    {
        var oldCount = _list.Count;
        var doubled = _list.Double();
        return doubled
            ? $"В конец приписана копия: {oldCount} + {oldCount} = {_list.Count} узлов."
            : "Пустой список удваивать нечего.";
    });

    [RelayCommand]
    private void SwapPositions() => Perform("Поменять местами", () =>
    {
        var first = ParseInt(FirstPositionText, "первая позиция");
        var second = ParseInt(SecondPositionText, "вторая позиция");
        if (first < 1 || second < 1)
        {
            throw new FormatException("Позиции нумеруются с 1.");
        }

        var swapped = _list.SwapPositions(first - 1, second - 1);
        return swapped
            ? $"Узлы на позициях {first} и {second} поменялись местами."
            : "Позиции совпадают или выходят за границы списка.";
    });

    [RelayCommand]
    private void Clear()
    {
        _list = new IntLinkedList();
        SplitFirst = "—";
        SplitSecond = "—";
        UpdateDisplay();
        AddLog("Очистить", "Список очищен.", false);
        Status = "Список пуст.";
    }

    public void LoadValuesFromFile(string text, string fileName)
    {
        try
        {
            var values = ParseIntList(text);
            if (values.Count == 0)
            {
                throw new FormatException("В файле нет целых чисел.");
            }

            _list = IntLinkedList.FromValues(values);
            ValuesText = string.Join(' ', values);
            UpdateDisplay();
            AddLog("Файл", $"Загружено {values.Count} чисел из «{fileName}».", false);
            Status = $"Список загружен из файла «{fileName}».";
        }
        catch (Exception exception)
        {
            Status = "Ошибка чтения файла: " + exception.Message;
            AddLog("Файл", exception.Message, true);
        }
    }

    private void Perform(string operation, Func<string> action)
    {
        try
        {
            var details = action();
            UpdateDisplay();
            AddLog(operation, details, false);
            Status = details;
        }
        catch (Exception exception)
        {
            Status = "Ошибка: " + exception.Message;
            AddLog(operation, exception.Message, true);
        }
    }

    private void UpdateDisplay()
    {
        CurrentList = _list.Describe(120);
        CountText = _list.Count.ToString("N0", CultureInfo.CurrentCulture);
        Items.Clear();
        foreach (var value in _list.ToList().Take(200))
        {
            Items.Add(new ListValueChip(value));
        }
    }

    private void AddLog(string operation, string details, bool isError)
    {
        _logCounter++;
        Log.Insert(0, new LinkedListLogEntry(_logCounter, operation, details, isError));
        while (Log.Count > 150)
        {
            Log.RemoveAt(Log.Count - 1);
        }
    }

    private static int ParseInt(string text, string name)
    {
        if (!int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var value))
        {
            throw new FormatException($"Поле «{name}» должно содержать целое число.");
        }

        return value;
    }

    private static IReadOnlyList<int> ParseIntList(string text)
    {
        var matches = Regex.Matches(text, @"-?\d+");
        var values = new List<int>(matches.Count);
        foreach (Match match in matches)
        {
            values.Add(int.Parse(match.Value, CultureInfo.CurrentCulture));
        }

        return values;
    }

    private static bool IsSortedNonDecreasing(IReadOnlyList<int> values)
    {
        for (var index = 1; index < values.Count; index++)
        {
            if (values[index] < values[index - 1])
            {
                return false;
            }
        }

        return true;
    }
}
