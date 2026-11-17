using System.Diagnostics;
using AlgorithmsLab3.Domain.Stacks;

namespace AlgorithmsLab3.Domain.Execution;

public static class StackCommandRunner
{
    public static StackCommandResult Run(string? input, bool captureLog = true, int maxLogEntries = 5000)
    {
        var stopwatch = Stopwatch.StartNew();
        var tokens = CommandInput.Tokenize(input);
        var stack = new LinkedStack<string>();
        var log = new List<OperationLogEntry>();
        var signature = ResultSignature.Start();
        var operations = 0;
        var errors = 0;
        var pushes = 0;
        var pops = 0;
        var tops = 0;
        var emptyChecks = 0;
        var prints = 0;

        foreach (var token in tokens)
        {
            operations++;
            var command = token;
            string result;
            var isError = false;

            if (command.StartsWith("1,", StringComparison.Ordinal))
            {
                var value = command[2..];
                if (value.Length == 0)
                {
                    result = "Push: не указано значение";
                    isError = true;
                }
                else
                {
                    stack.Push(value);
                    pushes++;
                    result = $"Push({value}): добавлено, размер {stack.Count}";
                }
            }
            else if (command == "1")
            {
                result = "Push: значение должно быть указано через запятую";
                isError = true;
            }
            else if (command == "2")
            {
                if (stack.TryPop(out var value))
                {
                    pops++;
                    result = $"Pop(): {value}";
                }
                else
                {
                    isError = true;
                    result = "Pop(): стек пуст";
                }
            }
            else if (command == "3")
            {
                if (stack.TryTop(out var value))
                {
                    tops++;
                    result = $"Top(): {value}";
                }
                else
                {
                    isError = true;
                    result = "Top(): стек пуст";
                }
            }
            else if (command == "4")
            {
                emptyChecks++;
                result = $"isEmpty(): {(stack.IsEmpty ? "true" : "false")}";
            }
            else if (command == "5")
            {
                prints++;
                result = $"Print(): {stack.Print()}";
            }
            else
            {
                isError = true;
                result = $"Неизвестная команда «{command}»";
            }

            if (isError)
            {
                errors++;
            }

            AddLog(log, captureLog, maxLogEntries, operations, command, result, isError);
            signature = ResultSignature.Mix(signature, result);
        }

        stopwatch.Stop();
        var finalState = stack.Print();
        return new StackCommandResult
        {
            Log = log,
            Operations = operations,
            Errors = errors,
            PushCount = pushes,
            PopCount = pops,
            TopCount = tops,
            IsEmptyCount = emptyChecks,
            PrintCount = prints,
            FinalCount = stack.Count,
            FinalState = finalState,
            Elapsed = stopwatch.Elapsed,
            Signature = ResultSignature.Mix(signature, finalState),
        };
    }

    private static void AddLog(
        List<OperationLogEntry> log,
        bool capture,
        int maxEntries,
        int index,
        string command,
        string result,
        bool isError)
    {
        if (capture && log.Count < maxEntries)
        {
            log.Add(new OperationLogEntry(index, command, result, isError));
        }
    }
}
