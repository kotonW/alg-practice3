using System.Text;
using AlgorithmsLab3.Domain.Execution;

namespace AlgorithmsLab3.Domain.Benchmarks;

public sealed record BenchmarkScenario(string Name, string Group, string Description, string Input, int OperationCount);

public sealed class StackBenchmarkRow
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required int Operations { get; init; }

    public required double ReadMilliseconds { get; init; }

    public required double ExecuteMilliseconds { get; init; }

    public required double TotalMilliseconds { get; init; }

    public required double MicrosecondsPerOperation { get; init; }

    public required int FinalCount { get; init; }

    public required int Errors { get; init; }

    public required int PushCount { get; init; }

    public required int PopCount { get; init; }
}

public sealed class QueueBenchmarkRow
{
    public required string Name { get; init; }

    public required string Group { get; init; }

    public required string Description { get; init; }

    public required int Operations { get; init; }

    public required double LinkedTotalMilliseconds { get; init; }

    public required double StandardTotalMilliseconds { get; init; }

    public required double LinkedExecuteMilliseconds { get; init; }

    public required double StandardExecuteMilliseconds { get; init; }

    public required double LinkedReadMilliseconds { get; init; }

    public required double StandardReadMilliseconds { get; init; }

    public required int FinalCount { get; init; }

    public required int Errors { get; init; }

    public required bool ResultsMatch { get; init; }

    public double Ratio => StandardTotalMilliseconds <= 0
        ? 0
        : LinkedTotalMilliseconds / StandardTotalMilliseconds;

    public double DifferencePercent => StandardTotalMilliseconds <= 0
        ? 0
        : (LinkedTotalMilliseconds - StandardTotalMilliseconds) / StandardTotalMilliseconds * 100;

    public string MatchText => ResultsMatch ? "да" : "нет";
}

public static class BenchmarkScenarios
{
    public static IReadOnlyList<BenchmarkScenario> Stack()
    {
        var scenarios = new List<BenchmarkScenario>
        {
            Scenario("Пример из задания", "Пример", "9 команд: push/pop/top/isEmpty/print", "3 4 1,56 1,7 1,cat 2 5 4"),
            Scenario("1 000 push", "Разный размер", "Только вставки, стек растёт до 1 000", PushSequence(1000)),
            Scenario("1 000 push/pop", "Разный состав", "Вставка и удаление, стек остаётся маленьким", PushPopSequence(500)),
            Scenario("10 000 смешанных", "Разный размер", "Цикл push/push/pop/top/isEmpty", MixedStackSequence(10_000)),
            Scenario("50 000 push", "Разный размер", "Большой набор однотипных вставок", PushSequence(50_000)),
            Scenario("5 000 push + Print", "Разный состав", "Каждые 10 вставок — печать всего стека O(n)", PrintHeavyStackSequence(5_000)),
        };

        return scenarios;
    }

    public static IReadOnlyList<BenchmarkScenario> Queue()
    {
        return
        [
            Scenario("Пример из задания", "Пример", "9 команд: вставка/удаление/просмотр/isEmpty/print", "3 4 1,56 1,7 1,cat 2 5 4"),
            Scenario("1 000 вставок", "Разная длина", "Небольшая очередь", EnqueueSequence(1_000)),
            Scenario("5 000 вставок", "Разная длина", "Средняя очередь", EnqueueSequence(5_000)),
            Scenario("20 000 вставок", "Разная длина", "Большая очередь", EnqueueSequence(20_000)),
            Scenario("20 000: вставка/удаление", "Одинаковая длина", "Состав 50/50 — очередь почти пуста", AlternatingQueueSequence(20_000)),
            Scenario("20 000: вставки + Print", "Одинаковая длина", "Частые печати всего содержимого", PrintHeavyQueueSequence(20_000)),
        ];
    }

    private static BenchmarkScenario Scenario(string name, string group, string description, string input)
    {
        var operations = CommandInput.Tokenize(input).Count;
        return new BenchmarkScenario(name, group, description, input, operations);
    }

    private static string PushSequence(int count)
    {
        var builder = new StringBuilder(count * 8);
        for (var index = 1; index <= count; index++)
        {
            if (index > 1)
            {
                builder.Append(' ');
            }

            builder.Append("1,").Append(index);
        }

        return builder.ToString();
    }

    private static string PushPopSequence(int pairs)
    {
        var builder = new StringBuilder(pairs * 16);
        for (var index = 1; index <= pairs; index++)
        {
            if (index > 1)
            {
                builder.Append(' ');
            }

            builder.Append("1,").Append(index).Append(" 2");
        }

        return builder.ToString();
    }

    private static string MixedStackSequence(int count)
    {
        var builder = new StringBuilder(count * 12);
        for (var index = 1; index <= count; index++)
        {
            if (index > 1)
            {
                builder.Append(' ');
            }

            switch (index % 5)
            {
                case 1:
                case 2:
                    builder.Append("1,").Append(index);
                    break;
                case 3:
                    builder.Append('2');
                    break;
                case 4:
                    builder.Append('3');
                    break;
                default:
                    builder.Append('4');
                    break;
            }
        }

        return builder.ToString();
    }

    private static string PrintHeavyStackSequence(int count)
    {
        var builder = new StringBuilder(count * 12);
        for (var index = 1; index <= count; index++)
        {
            if (index > 1)
            {
                builder.Append(' ');
            }

            builder.Append("1,").Append(index);
            if (index % 10 == 0)
            {
                builder.Append(" 5");
            }
        }

        return builder.ToString();
    }

    private static string EnqueueSequence(int count)
    {
        var builder = new StringBuilder(count * 8);
        for (var index = 1; index <= count; index++)
        {
            if (index > 1)
            {
                builder.Append(' ');
            }

            builder.Append("1,").Append(index);
        }

        return builder.ToString();
    }

    private static string AlternatingQueueSequence(int count)
    {
        var builder = new StringBuilder(count * 8);
        var value = 1;
        for (var index = 0; index < count; index++)
        {
            if (index > 0)
            {
                builder.Append(' ');
            }

            if (index % 2 == 0)
            {
                builder.Append("1,").Append(value++);
            }
            else
            {
                builder.Append('2');
            }
        }

        return builder.ToString();
    }

    private static string PrintHeavyQueueSequence(int count)
    {
        var builder = new StringBuilder(count * 8);
        var value = 1;
        for (var index = 0; index < count; index++)
        {
            if (index > 0)
            {
                builder.Append(' ');
            }

            if (index % 20 == 19)
            {
                builder.Append('5');
            }
            else
            {
                builder.Append("1,").Append(value++);
            }
        }

        return builder.ToString();
    }
}

public static class BenchmarkRunner
{
    private static readonly string BenchmarkDirectory = Path.Combine(
        Path.GetTempPath(),
        "algorithms-lab3-benchmarks");

    public static IReadOnlyList<StackBenchmarkRow> RunStackBenchmarks(Action<string>? report = null)
    {
        Directory.CreateDirectory(BenchmarkDirectory);
        var rows = new List<StackBenchmarkRow>();

        foreach (var scenario in BenchmarkScenarios.Stack())
        {
            report?.Invoke($"Стек: {scenario.Name}");
            var path = WriteScenarioFile("stack", scenario);
            var readWatch = System.Diagnostics.Stopwatch.StartNew();
            var text = File.ReadAllText(path);
            readWatch.Stop();

            var result = StackCommandRunner.Run(text, captureLog: false);
            rows.Add(new StackBenchmarkRow
            {
                Name = scenario.Name,
                Description = scenario.Description,
                Operations = result.Operations,
                ReadMilliseconds = readWatch.Elapsed.TotalMilliseconds,
                ExecuteMilliseconds = result.Elapsed.TotalMilliseconds,
                TotalMilliseconds = readWatch.Elapsed.TotalMilliseconds + result.Elapsed.TotalMilliseconds,
                MicrosecondsPerOperation = result.Operations == 0
                    ? 0
                    : result.Elapsed.TotalMicroseconds / result.Operations,
                FinalCount = result.FinalCount,
                Errors = result.Errors,
                PushCount = result.PushCount,
                PopCount = result.PopCount,
            });
        }

        return rows;
    }

    public static IReadOnlyList<QueueBenchmarkRow> RunQueueBenchmarks(Action<string>? report = null)
    {
        Directory.CreateDirectory(BenchmarkDirectory);
        var rows = new List<QueueBenchmarkRow>();

        foreach (var scenario in BenchmarkScenarios.Queue())
        {
            report?.Invoke($"Очередь: {scenario.Name}");
            var path = WriteScenarioFile("queue", scenario);

            var linkedReadWatch = System.Diagnostics.Stopwatch.StartNew();
            var linkedText = File.ReadAllText(path);
            linkedReadWatch.Stop();
            var linked = QueueCommandRunner.Run(
                new Queues.LinkedQueue<string>(),
                linkedText,
                captureLog: false);

            var standardReadWatch = System.Diagnostics.Stopwatch.StartNew();
            var standardText = File.ReadAllText(path);
            standardReadWatch.Stop();
            var standard = QueueCommandRunner.Run(
                new Queues.StandardQueueAdapter<string>(),
                standardText,
                captureLog: false);

            var linkedTotal = linkedReadWatch.Elapsed.TotalMilliseconds + linked.Elapsed.TotalMilliseconds;
            var standardTotal = standardReadWatch.Elapsed.TotalMilliseconds + standard.Elapsed.TotalMilliseconds;

            rows.Add(new QueueBenchmarkRow
            {
                Name = scenario.Name,
                Group = scenario.Group,
                Description = scenario.Description,
                Operations = scenario.OperationCount,
                LinkedTotalMilliseconds = linkedTotal,
                StandardTotalMilliseconds = standardTotal,
                LinkedExecuteMilliseconds = linked.Elapsed.TotalMilliseconds,
                StandardExecuteMilliseconds = standard.Elapsed.TotalMilliseconds,
                LinkedReadMilliseconds = linkedReadWatch.Elapsed.TotalMilliseconds,
                StandardReadMilliseconds = standardReadWatch.Elapsed.TotalMilliseconds,
                FinalCount = linked.FinalCount,
                Errors = linked.Errors + standard.Errors,
                ResultsMatch = linked.Signature == standard.Signature,
            });
        }

        return rows;
    }

    private static string WriteScenarioFile(string prefix, BenchmarkScenario scenario)
    {
        var safeName = string.Concat(scenario.Name.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        var path = Path.Combine(BenchmarkDirectory, $"{prefix}-{safeName}.txt");
        File.WriteAllText(path, scenario.Input, Encoding.UTF8);
        return path;
    }
}
