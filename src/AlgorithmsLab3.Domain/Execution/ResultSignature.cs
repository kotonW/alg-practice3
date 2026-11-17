namespace AlgorithmsLab3.Domain.Execution;

/// <summary>Стабильный FNV-1a хеш для сравнения вывода реализаций.</summary>
public static class ResultSignature
{
    private const ulong Offset = 14695981039346656037;
    private const ulong Prime = 1099511628211;

    public static ulong Start() => Offset;

    public static ulong Mix(ulong hash, string text)
    {
        foreach (var character in text)
        {
            hash ^= character;
            hash *= Prime;
        }

        hash ^= 0xFF;
        return hash * Prime;
    }
}
