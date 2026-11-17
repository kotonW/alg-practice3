using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using AlgorithmsLab3.Domain.Benchmarks;
using AlgorithmsLab3.Domain.Execution;
using AlgorithmsLab3.Domain.Expressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmsLab3.App.ViewModels;

public partial class StackPageViewModel : ObservableObject
{
    private static readonly string[] Words = ["cat", "dog", "apple", "tree", "stack", "queue", "node", "data"];

    [ObservableProperty]
    private string _commandInput = "3 4 1,56 1,7 1,cat 2 5 4";

    [ObservableProperty]
    private string _runStatus = "Нажмите «Выполнить», чтобы прочитать команды и обработать стек.";

    [ObservableProperty]
    private string _metrics = string.Empty;

    [ObservableProperty]
    private string _finalState = "[]";

    [ObservableProperty]
    private string _postfixInput = "7 8 + 3 2 + /";

    [ObservableProperty]
    private string _postfixResult = "—";

    [ObservableProperty]
    private string _postfixTrace = "Здесь появится пошаговый протокол вычислений.";

    [ObservableProperty]
    private string _postfixMetrics = string.Empty;

    [ObservableProperty]
    private string _infixInput = "2 + 2 * (3 - 1) ^ 2 + sqrt(16)";

    [ObservableProperty]
    private string _infixPostfix = "—";

    [ObservableProperty]
    private string _infixResult = "—";

    [ObservableProperty]
    private string _infixTrace = "Введите инфиксное выражение и нажмите «Перевести и вычислить».";

    [ObservableProperty]
    private string _benchmarkStatus = "Замеры ещё не выполнялись. Будут созданы разные по размеру и составу наборы команд.";

    [ObservableProperty]
    private bool _isBusy;

    public StackPageViewModel()
    {
        ApplyResult(StackCommandRunner.Run(CommandInput));
        EvaluatePostfix();
        ConvertInfix();
    }

    public ObservableCollection<OperationLogEntry> Log { get; } = [];

    public ObservableCollection<StackBenchmarkRow> BenchmarkRows { get; } = [];

    public string ComplexityText =>
        "Постфиксная запись: время O(n), память O(n) в худшем случае. "
        + "Перевод инфикса: время O(n), память O(n). "
        + "Каждый токен обрабатывается один раз.";

    [RelayCommand]
    private async Task RunCommandsAsync()
    {
        var input = CommandInput;
        IsBusy = true;
        try
        {
            var result = await Task.Run(() => StackCommandRunner.Run(input));
            ApplyResult(result);
            RunStatus = result.Errors == 0
                ? "Все команды обработаны без ошибок."
                : $"Обработано команд: {result.Operations:N0}, ошибок: {result.Errors:N0}.";
        }
        catch (Exception exception)
        {
            RunStatus = "Ошибка выполнения: " + exception.Message;
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
        RunStatus = "Загружен пример из задания.";
    }

    [RelayCommand]
    private void GenerateRandom()
    {
        var random = new Random();
        var builder = new StringBuilder();
        var count = random.Next(120, 220);

        // Гарантируем, что Pop и Top не всегда попадают в пустой стек.
        builder.Append("1,start 1,42 ");

        for (var index = 0; index < count; index++)
        {
            var command = random.Next(1, 6);
            switch (command)
            {
                case 1:
                    var value = random.Next(4) == 0
                        ? Words[random.Next(Words.Length)]
                        : random.Next(1, 1000).ToString(CultureInfo.InvariantCulture);
                    builder.Append("1,").Append(value);
                    break;
                default:
                    builder.Append(command.ToString(CultureInfo.InvariantCulture));
                    break;
            }

            builder.Append(' ');
        }

        CommandInput = builder.ToString().TrimEnd();
        RunStatus = "Сгенерирован случайный набор команд.";
    }

    [RelayCommand]
    private void EvaluatePostfix()
    {
        try
        {
            var result = PostfixEvaluator.Evaluate(PostfixInput);
            PostfixResult = "= " + result.FormattedValue;
            PostfixTrace = string.Join(Environment.NewLine, result.Trace);
            PostfixMetrics =
                $"Токенов: {result.TokenCount:N0} · операций: {result.OperationCount:N0} · "
                + $"максимум в стеке: {result.MaxStackDepth:N0} · сложность O(n)";
        }
        catch (ExpressionException exception)
        {
            PostfixResult = "Ошибка";
            PostfixTrace = exception.Message;
            PostfixMetrics = "Проверьте запись: операнды и операции разделяются пробелами.";
        }
    }

    [RelayCommand]
    private void ConvertInfix()
    {
        try
        {
            var conversion = InfixToPostfixConverter.Convert(InfixInput);
            var evaluation = PostfixEvaluator.Evaluate(conversion.Tokens);
            InfixPostfix = conversion.Postfix;
            InfixResult = "= " + evaluation.FormattedValue;

            var trace = new StringBuilder();
            trace.AppendLine("Сортировочная станция:");
            foreach (var step in conversion.Steps)
            {
                trace.AppendLine("  " + step);
            }

            trace.AppendLine();
            trace.AppendLine("Вычисление постфиксной записи:");
            foreach (var step in evaluation.Trace)
            {
                trace.AppendLine("  " + step);
            }

            InfixTrace = trace.ToString().TrimEnd();
        }
        catch (ExpressionException exception)
        {
            InfixPostfix = "—";
            InfixResult = "Ошибка";
            InfixTrace = exception.Message;
        }
    }

    [RelayCommand]
    private async Task RunBenchmarksAsync()
    {
        IsBusy = true;
        BenchmarkStatus = "Выполняются замеры: создаются файлы, читаются и обрабатываются наборы команд…";
        try
        {
            var rows = await Task.Run(() => BenchmarkRunner.RunStackBenchmarks());
            BenchmarkRows.Clear();
            foreach (var row in rows)
            {
                BenchmarkRows.Add(row);
            }

            BenchmarkStatus =
                $"Готово: {rows.Count} наборов. "
                + "В таблице отдельно показаны чтение файла, выполнение и суммарное время.";
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
        RunStatus = $"Загружен файл «{fileName}».";
        RunCommandsCommand.Execute(null);
    }

    public string BuildBenchmarkCsv()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Набор;Описание;Операций;Чтение, мс;Выполнение, мс;Итого, мс;мкс/операция;В стеке;Ошибок");
        foreach (var row in BenchmarkRows)
        {
            builder.Append(Escape(row.Name)).Append(';')
                .Append(Escape(row.Description)).Append(';')
                .Append(row.Operations).Append(';')
                .Append(row.ReadMilliseconds.ToString("F6", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.ExecuteMilliseconds.ToString("F6", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.TotalMilliseconds.ToString("F6", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.MicrosecondsPerOperation.ToString("F3", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.FinalCount).Append(';')
                .Append(row.Errors).AppendLine();
        }

        return builder.ToString();
    }

    private static string Escape(string value) => value.Replace(';', ',');

    private void ApplyResult(StackCommandResult result)
    {
        Log.Clear();
        foreach (var entry in result.Log)
        {
            Log.Add(entry);
        }

        FinalState = result.FinalState;
        Metrics =
            $"Операций: {result.Operations:N0} · ошибок: {result.Errors:N0} · "
            + $"push: {result.PushCount:N0} · pop: {result.PopCount:N0} · "
            + $"top: {result.TopCount:N0} · print: {result.PrintCount:N0} · "
            + $"время: {DisplayFormat.Milliseconds(result.Elapsed.TotalMilliseconds)}";
    }
}
