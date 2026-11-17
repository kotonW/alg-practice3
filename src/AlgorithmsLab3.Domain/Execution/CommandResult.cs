using System.Text;

namespace AlgorithmsLab3.Domain.Execution;

public sealed class StackCommandResult
{
    public required IReadOnlyList<OperationLogEntry> Log { get; init; }

    public required int Operations { get; init; }

    public required int Errors { get; init; }

    public required int PushCount { get; init; }

    public required int PopCount { get; init; }

    public required int TopCount { get; init; }

    public required int IsEmptyCount { get; init; }

    public required int PrintCount { get; init; }

    public required int FinalCount { get; init; }

    public required string FinalState { get; init; }

    public required TimeSpan Elapsed { get; init; }

    public required ulong Signature { get; init; }

    public string Summary =>
        $"операций: {Operations:N0}, ошибок: {Errors:N0}, в стеке: {FinalCount:N0}, время: {Elapsed.TotalMilliseconds:F3} мс";
}

public sealed class QueueCommandResult
{
    public required string Implementation { get; init; }

    public required IReadOnlyList<OperationLogEntry> Log { get; init; }

    public required int Operations { get; init; }

    public required int Errors { get; init; }

    public required int EnqueueCount { get; init; }

    public required int DequeueCount { get; init; }

    public required int PeekCount { get; init; }

    public required int IsEmptyCount { get; init; }

    public required int PrintCount { get; init; }

    public required int FinalCount { get; init; }

    public required string FinalState { get; init; }

    public required TimeSpan Elapsed { get; init; }

    public required ulong Signature { get; init; }

    public string Summary =>
        $"{Implementation}: операций {Operations:N0}, в очереди {FinalCount:N0}, время {Elapsed.TotalMilliseconds:F3} мс";
}

public static class CommandInput
{
    /// <summary>Разбивает входной файл на команды, поддерживая запись «1, 56».</summary>
    public static IReadOnlyList<string> Tokenize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        var normalized = new StringBuilder(input.Length);
        for (var index = 0; index < input.Length; index++)
        {
            var current = input[index];
            if (current == ',')
            {
                normalized.Append(',');
                while (index + 1 < input.Length && char.IsWhiteSpace(input[index + 1]))
                {
                    index++;
                }

                continue;
            }

            normalized.Append(current);
        }

        return normalized
            .ToString()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
