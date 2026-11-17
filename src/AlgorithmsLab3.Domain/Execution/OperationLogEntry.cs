namespace AlgorithmsLab3.Domain.Execution;

public sealed record OperationLogEntry(int Index, string Command, string Result, bool IsError)
{
    public string Status => IsError ? "ошибка" : "ok";
}
