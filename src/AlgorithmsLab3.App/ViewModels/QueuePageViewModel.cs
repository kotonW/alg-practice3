using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using AlgorithmsLab3.Domain.Benchmarks;
using AlgorithmsLab3.Domain.Execution;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmsLab3.App.ViewModels;

public partial class QueuePageViewModel : ObservableObject
{
    private static readonly string[] Words = ["cat", "dog", "apple", "tree", "stack", "queue", "node", "data"];

    [ObservableProperty]
    private string _commandInput = "3 4 1,56 1,7 1,cat 2 5 4";

    [ObservableProperty]
    private string _status = "Команды выполняются параллельно для собственной очереди и стандартного Queue<T>.";

    [ObservableProperty]
    private string _linkedSummary = string.Empty;

    [ObservableProperty]
    private string _standardSummary = string.Empty;

    [ObservableProperty]
    private string _linkedFinal = "[]";

    [ObservableProperty]
    private string _standardFinal = "[]";

    [ObservableProperty]
    private string _matchText = "Результаты ещё не сравнивались.";

    [ObservableProperty]
    private string _benchmarkStatus = "Замеры ещё не выполнялись.";

    [ObservableProperty]
    private bool _isBusy;

    public QueuePageViewModel()
    {
        ApplyResult(QueueComparisonRunner.Run(CommandInput));
    }

    public ObservableCollection<QueueComparisonEntry> ComparisonLog { get; } = [];

    public ObservableCollection<QueueBenchmarkRow> BenchmarkRows { get; } = [];

    [RelayCommand]
    private async Task RunCommandsAsync()
    {
        var input = CommandInput;
        IsBusy = true;
        try
        {
            var result = await Task.Run(() => QueueComparisonRunner.Run(input));
            ApplyResult(result);
        }
        catch (Exception exception)
        {
            Status = "Ошибка выполнения: " + exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void LoadSample()
    {
        CommandInput = "3 4 1,56 1,7 1,cat 2 5 4";
        Status = "Загружен пример из задания.";
    }

    [RelayCommand]
    private void GenerateRandom()
    {
        var random = new Random();
        var builder = new StringBuilder("1,start 1,42 ");
        var count = random.Next(120, 220);

        for (var index = 0; index < count; index++)
        {
            var command = random.Next(1, 6);
            if (command == 1)
            {
                var value = random.Next(4) == 0
                    ? Words[random.Next(Words.Length)]
                    : random.Next(1, 1000).ToString(CultureInfo.InvariantCulture);
                builder.Append("1,").Append(value);
            }
            else
            {
                builder.Append(command.ToString(CultureInfo.InvariantCulture));
            }

            builder.Append(' ');
        }

        CommandInput = builder.ToString().TrimEnd();
        Status = "Сгенерирован случайный набор команд.";
    }

    [RelayCommand]
    private async Task RunBenchmarksAsync()
    {
        IsBusy = true;
        BenchmarkStatus = "Выполняются замеры для двух реализаций очереди…";
        try
        {
            var rows = await Task.Run(() => BenchmarkRunner.RunQueueBenchmarks());
            BenchmarkRows.Clear();
            foreach (var row in rows)
            {
                BenchmarkRows.Add(row);
            }

            BenchmarkStatus =
                $"Готово: {rows.Count} наборов. "
                + "Наборы 20 000 имеют одинаковую длину, но разный состав операций.";
        }
        catch (Exception exception)
        {
            BenchmarkStatus = "Ошибка замеров: " + exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void LoadCommandsFromFile(string text, string fileName)
    {
        CommandInput = text;
        Status = $"Загружен файл «{fileName}».";
        RunCommandsCommand.Execute(null);
    }

    public string BuildBenchmarkCsv()
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "Набор;Группа;Описание;Операций;Список: чтение, мс;Список: выполнение, мс;Список: итого, мс;"
            + "Queue: чтение, мс;Queue: выполнение, мс;Queue: итого, мс;Отношение;Разница, %;Совпадение");
        foreach (var row in BenchmarkRows)
        {
            builder.Append(Escape(row.Name)).Append(';')
                .Append(Escape(row.Group)).Append(';')
                .Append(Escape(row.Description)).Append(';')
                .Append(row.Operations).Append(';')
                .Append(row.LinkedReadMilliseconds.ToString("F6", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.LinkedExecuteMilliseconds.ToString("F6", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.LinkedTotalMilliseconds.ToString("F6", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.StandardReadMilliseconds.ToString("F6", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.StandardExecuteMilliseconds.ToString("F6", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.StandardTotalMilliseconds.ToString("F6", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.Ratio.ToString("F4", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.DifferencePercent.ToString("F2", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.ResultsMatch ? "да" : "нет").AppendLine();
        }

        return builder.ToString();
    }

    private void ApplyResult(QueueComparisonResult result)
    {
        ComparisonLog.Clear();
        foreach (var entry in result.Entries)
        {
            ComparisonLog.Add(entry);
        }

        LinkedSummary =
            $"{result.Linked.Operations:N0} операций · ошибок {result.Linked.Errors:N0} · "
            + $"{DisplayFormat.Milliseconds(result.Linked.Elapsed.TotalMilliseconds)}";
        StandardSummary =
            $"{result.Standard.Operations:N0} операций · ошибок {result.Standard.Errors:N0} · "
            + $"{DisplayFormat.Milliseconds(result.Standard.Elapsed.TotalMilliseconds)}";
        LinkedFinal = result.Linked.FinalState;
        StandardFinal = result.Standard.FinalState;
        MatchText = result.ResultsMatch
            ? "Результаты реализаций полностью совпадают."
            : "ВНИМАНИЕ: результаты реализаций различаются.";
        Status = result.ResultsMatch
            ? "Команды выполнены, очереди работают одинаково."
            : "Команды выполнены, но обнаружено расхождение.";
    }

    private static string Escape(string value) => value.Replace(';', ',');
}
