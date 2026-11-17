namespace AlgorithmsLab3.Domain.Lists;

/// <summary>Узел собственного односвязного списка.</summary>
public sealed class SinglyNode<T>
{
    public SinglyNode(T value)
    {
        Value = value;
    }

    public T Value { get; set; }

    public SinglyNode<T>? Next { get; set; }
}
