using AlgorithmsLab3.Domain.Lists;

namespace AlgorithmsLab3.Tests;

public sealed class IntLinkedListTests
{
    private static IntLinkedList List(params int[] values) => IntLinkedList.FromValues(values);

    [Fact]
    public void FromValues_BuildsListAndKeepsEnds()
    {
        var list = List(5, 3, 8);

        Assert.Equal(3, list.Count);
        Assert.Equal([5, 3, 8], list.ToList());
        Assert.Equal(5, list.HeadValue);
        Assert.Equal(8, list.TailValue);
        Assert.False(list.IsEmpty);
    }

    [Fact]
    public void ReverseInPlace_ReversesOrderAndRepairsTail()
    {
        var list = List(5, 3, 8, 3, 1);

        Assert.True(list.ReverseInPlace());
        Assert.Equal([1, 3, 8, 3, 5], list.ToList());
        Assert.Equal(1, list.HeadValue);
        Assert.Equal(5, list.TailValue);
        Assert.Equal(5, list.Count);
    }

    [Fact]
    public void ReverseInPlace_OnShortLists_ReturnsFalse()
    {
        var empty = new IntLinkedList();
        var single = List(7);

        Assert.False(empty.ReverseInPlace());
        Assert.False(single.ReverseInPlace());
        Assert.Equal([7], single.ToList());
    }

    [Fact]
    public void MoveLastToFront_MovesTailToHead()
    {
        var list = List(1, 2, 3);

        Assert.True(list.MoveLastToFront());
        Assert.Equal([3, 1, 2], list.ToList());
        Assert.Equal(3, list.HeadValue);
        Assert.Equal(2, list.TailValue);
        Assert.False(List(1).MoveLastToFront());
    }

    [Fact]
    public void MoveFirstToEnd_MovesHeadToTail()
    {
        var list = List(1, 2, 3);

        Assert.True(list.MoveFirstToEnd());
        Assert.Equal([2, 3, 1], list.ToList());
        Assert.Equal(2, list.HeadValue);
        Assert.Equal(1, list.TailValue);
        Assert.False(List(1).MoveFirstToEnd());
    }

    [Fact]
    public void CountDistinct_CountsUniqueValues()
    {
        var list = List(5, 3, 8, 3, 1, 9, 5, 2);

        Assert.Equal(6, list.CountDistinct());
        Assert.Equal(0, new IntLinkedList().CountDistinct());
    }

    [Fact]
    public void RemoveNonUnique_KeepsOnlyValuesSeenOnce()
    {
        var list = List(5, 3, 8, 3, 1, 9, 5, 2);

        var removed = list.RemoveNonUnique();

        Assert.Equal(4, removed);
        Assert.Equal([8, 1, 9, 2], list.ToList());
        Assert.Equal(4, list.Count);
        Assert.Equal(2, list.TailValue);
    }

    [Fact]
    public void RemoveNonUnique_OnAllDuplicates_LeavesEmptyList()
    {
        var list = List(7, 7, 7);

        Assert.Equal(3, list.RemoveNonUnique());
        Assert.True(list.IsEmpty);
        Assert.Null(list.HeadValue);
        Assert.Null(list.TailValue);
        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void InsertSorted_KeepsAscendingOrder()
    {
        var list = new IntLinkedList();

        Assert.True(list.InsertSorted(5));
        list.InsertSorted(3);
        list.InsertSorted(7);
        list.InsertSorted(5);
        list.InsertSorted(1);

        Assert.Equal([1, 3, 5, 5, 7], list.ToList());
        Assert.Equal(1, list.HeadValue);
        Assert.Equal(7, list.TailValue);
    }

    [Fact]
    public void RemoveAll_RemovesEveryOccurrence()
    {
        var list = List(1, 2, 1, 3, 1);

        Assert.Equal(3, list.RemoveAll(1));
        Assert.Equal([2, 3], list.ToList());
        Assert.Equal(3, list.TailValue);
        Assert.Equal(0, list.RemoveAll(42));
    }

    [Fact]
    public void RemoveAll_CanEmptyTheList()
    {
        var list = List(7, 7);

        Assert.Equal(2, list.RemoveAll(7));
        Assert.True(list.IsEmpty);
        Assert.Null(list.TailValue);
    }

    [Fact]
    public void InsertBefore_FindsFirstOccurrence()
    {
        var list = List(1, 2, 3);

        Assert.True(list.InsertBefore(2, 9));
        Assert.Equal([1, 9, 2, 3], list.ToList());

        Assert.True(list.InsertBefore(1, 8));
        Assert.Equal([8, 1, 9, 2, 3], list.ToList());
        Assert.Equal(8, list.HeadValue);
        Assert.Equal(3, list.TailValue);

        Assert.False(list.InsertBefore(42, 0));
        Assert.False(new IntLinkedList().InsertBefore(1, 2));
    }

    [Fact]
    public void Append_AddsSecondListAndKeepsItUnchanged()
    {
        var first = List(1, 2);
        var second = List(3, 4);

        Assert.Equal(2, first.Append(second));

        Assert.Equal([1, 2, 3, 4], first.ToList());
        Assert.Equal([3, 4], second.ToList());
        Assert.Equal(4, first.TailValue);
    }

    [Fact]
    public void SplitByFirst_SplitsBeforeValue()
    {
        var list = List(1, 2, 3, 4);

        var (first, second) = list.SplitByFirst(3);

        Assert.Equal([1, 2], first.ToList());
        Assert.Equal([3, 4], second.ToList());
        Assert.Equal([1, 2, 3, 4], list.ToList());

        var (all, none) = list.SplitByFirst(42);
        Assert.Equal([1, 2, 3, 4], all.ToList());
        Assert.True(none.IsEmpty);

        var (empty, tail) = list.SplitByFirst(1);
        Assert.True(empty.IsEmpty);
        Assert.Equal([1, 2, 3, 4], tail.ToList());
    }

    [Fact]
    public void InsertSelfAfter_DuplicatesWholeListAfterAnchor()
    {
        var list = List(1, 2, 3);

        var added = list.InsertSelfAfter(2);

        Assert.Equal(3, added);
        Assert.Equal([1, 2, 1, 2, 3, 3], list.ToList());
        Assert.Equal(6, list.Count);
        Assert.Equal(3, list.TailValue);
    }

    [Fact]
    public void InsertSelfAfter_TailAnchor_AppendsCopyAtEnd()
    {
        var list = List(1, 2, 3);

        Assert.Equal(3, list.InsertSelfAfter(3));

        Assert.Equal([1, 2, 3, 1, 2, 3], list.ToList());
        Assert.Equal(3, list.TailValue);
        Assert.Equal(0, list.InsertSelfAfter(42));
    }

    [Fact]
    public void Double_AppendsCopyOfItself()
    {
        var list = List(1, 2, 3);

        Assert.True(list.Double());
        Assert.Equal([1, 2, 3, 1, 2, 3], list.ToList());
        Assert.Equal(3, list.TailValue);
        Assert.False(new IntLinkedList().Double());
    }

    [Fact]
    public void SwapPositions_SwapsNonAdjacentNodes()
    {
        var list = List(1, 2, 3, 4, 5);

        Assert.True(list.SwapPositions(1, 3));

        Assert.Equal([1, 4, 3, 2, 5], list.ToList());
        Assert.Equal(5, list.TailValue);
    }

    [Fact]
    public void SwapPositions_SwapsHeadAndTail()
    {
        var list = List(1, 2, 3, 4, 5);

        Assert.True(list.SwapPositions(0, 4));

        Assert.Equal([5, 2, 3, 4, 1], list.ToList());
        Assert.Equal(5, list.HeadValue);
        Assert.Equal(1, list.TailValue);
    }

    [Fact]
    public void SwapPositions_SwapsAdjacentNodes()
    {
        var list = List(1, 2, 3, 4, 5);

        Assert.True(list.SwapPositions(0, 1));
        Assert.Equal([2, 1, 3, 4, 5], list.ToList());

        Assert.True(list.SwapPositions(3, 4));
        Assert.Equal([2, 1, 3, 5, 4], list.ToList());
        Assert.Equal(2, list.HeadValue);
        Assert.Equal(4, list.TailValue);
    }

    [Fact]
    public void SwapPositions_RejectsInvalidIndexes()
    {
        var list = List(1, 2, 3);

        Assert.False(list.SwapPositions(0, 0));
        Assert.False(list.SwapPositions(-1, 1));
        Assert.False(list.SwapPositions(0, 3));
        Assert.Equal([1, 2, 3], list.ToList());
    }

    [Fact]
    public void Describe_TruncatesLongLists()
    {
        var list = List(1, 2, 3, 4, 5);

        Assert.Equal("[1, 2, 3, … (+2)]", list.Describe(3));
        Assert.Equal("[1, 2, 3, 4, 5]", list.Describe());
        Assert.Equal("[]", new IntLinkedList().Describe());
    }

    [Fact]
    public void Replace_ResetsContent()
    {
        var list = List(1, 2, 3);

        list.Replace([9, 8]);

        Assert.Equal([9, 8], list.ToList());
        Assert.Equal(2, list.Count);
        Assert.Equal(9, list.HeadValue);
        Assert.Equal(8, list.TailValue);
    }
}
