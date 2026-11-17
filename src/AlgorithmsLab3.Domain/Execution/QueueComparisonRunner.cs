using AlgorithmsLab3.Domain.Queues;

namespace AlgorithmsLab3.Domain.Execution;

public sealed record QueueComparisonEntry(
    int Index,
    string Command,
    string LinkedResult,
    string StandardResult,
    bool Match)
{
    public string Mark => Match ? "✓" : "≠";
}

public sealed class QueueComparisonResult
{
    public required QueueCommandResult Linked { get; init; }

    public required QueueCommandResult Standard { get; init; }

    public required IReadOnlyList<QueueComparisonEntry> Entries { get; init; }

    public required bool ResultsMatch { get; init; }

    public TimeSpan TotalElapsed => Linked.Elapsed + Standard.Elapsed;
}

public static class QueueComparisonRunner
{
    public static QueueComparisonResult Run(string? input, bool captureLog = true, int maxLogEntries = 5000)
    {
        var linked = QueueCommandRunner.Run(new LinkedQueue<string>(), input, captureLog, maxLogEntries);
        var standard = QueueCommandRunner.Run(new StandardQueueAdapter<string>(), input, captureLog, maxLogEntries);

        var entries = new List<QueueComparisonEntry>();
        var count = Math.Min(linked.Log.Count, standard.Log.Count);
        for (var index = 0; index < count; index++)
        {
            var left = linked.Log[index];
            var right = standard.Log[index];
            entries.Add(new QueueComparisonEntry(
                left.Index,
                left.Command,
                left.Result,
                right.Result,
                string.Equals(left.Result, right.Result, StringComparison.Ordinal)));
        }

        var match = linked.Signature == standard.Signature
            && linked.Errors == standard.Errors
            && linked.FinalCount == standard.FinalCount
            && linked.Operations == standard.Operations;

        return new QueueComparisonResult
        {
            Linked = linked,
            Standard = standard,
            Entries = entries,
            ResultsMatch = match,
        };
    }
}
