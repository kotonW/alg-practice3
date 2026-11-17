using System.Diagnostics;
using AlgorithmsLab3.Domain.Queues;

namespace AlgorithmsLab3.Domain.Execution;

public static class QueueCommandRunner
{
    public static QueueCommandResult Run(
        IQueue<string> queue,
        string? input,
        bool captureLog = true,
        int maxLogEntries = 5000)
    {
        ArgumentNullException.ThrowIfNull(queue);
        var stopwatch = Stopwatch.StartNew();
        var tokens = CommandInput.Tokenize(input);
        var log = new List<OperationLogEntry>();
        var signature = ResultSignature.Start();
        var operations = 0;
        var errors = 0;
        var enqueues = 0;
        var dequeues = 0;
        var peeks = 0;
        var emptyChecks = 0;
        var prints = 0;

        foreach (var command in tokens)
        {
            operations++;
            string result;
            var isError = false;

            if (command.StartsWith("1,", StringComparison.Ordinal))
            {
                var value = command[2..];
                if (value.Length == 0)
                {
                    result = "Enqueue: не указано значение";
                    isError = true;
                }
                else
                {
                    queue.Enqueue(value);
                    enqueues++;
                    result = $"Enqueue({value}): добавлено, размер {queue.Count}";
                }
            }
            else if (command == "1")
            {
                result = "Enqueue: значение должно быть указано через запятую";
                isError = true;
            }
            else if (command == "2")
            {
                if (queue.TryDequeue(out var value))
                {
                    dequeues++;
                    result = $"Dequeue(): {value}";
                }
                else
                {
                    isError = true;
                    result = "Dequeue(): очередь пуста";
                }
            }
            else if (command == "3")
            {
                if (queue.TryPeek(out var value))
                {
                    peeks++;
                    result = $"Peek(): {value}";
                }
                else
                {
                    isError = true;
                    result = "Peek(): очередь пуста";
                }
            }
            else if (command == "4")
            {
                emptyChecks++;
                result = $"isEmpty(): {(queue.IsEmpty ? "true" : "false")}";
            }
            else if (command == "5")
            {
                prints++;
                result = $"Print(): {queue.Print()}";
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
        var finalState = queue.Print();
        return new QueueCommandResult
        {
            Implementation = queue.DisplayName,
            Log = log,
            Operations = operations,
            Errors = errors,
            EnqueueCount = enqueues,
            DequeueCount = dequeues,
            PeekCount = peeks,
            IsEmptyCount = emptyChecks,
            PrintCount = prints,
            FinalCount = queue.Count,
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
