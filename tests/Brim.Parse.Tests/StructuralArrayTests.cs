using Brim.Parse.Collections;

namespace Brim.Parse.Tests;

public class StructuralArrayTests
{
  [Fact]
  public void Create_Empty_ReturnsSingletonEmpty()
  {
    var a = StructuralArray.Create<int>();
    Assert.Empty(a);
    Assert.True(a.Equals([]));
  }

  private static readonly int[] _expected = [1, 2, 3];

  [Fact]
  public void Create_WithItems_HasSequence()
  {
    var a = StructuralArray.Create(1, 2, 3);
    Assert.Equal(3, a.Count);
    Assert.Equal(_expected, a.AsSpan.ToArray());
  }

  [Fact]
  public void Constructor_FromNullArray_IsEmpty()
  {
    int[]? source = null;
    var a = new StructuralArray<int>(source!);
    Assert.Empty(a);
  }

  [Fact]
  public void Implicit_FromArray_Works()
  {
    StructuralArray<int> a = new[] { 4, 5 };
    Assert.Equal(2, a.Count);
    Assert.Equal(5, a[1]);
  }

  [Fact]
  public void Implicit_FromImmutableArray_Works()
  {
    var imm = ImmutableArray.Create(7, 8, 9);
    StructuralArray<int> a = imm;
    Assert.Equal(3, a.Count);
    Assert.Equal(8, a[1]);
  }

  [Fact]
  public void Implicit_FromBuilder_Works()
  {
    var builder = ImmutableArray.CreateBuilder<int>();
    builder.Add(10);
    builder.Add(11);
    StructuralArray<int> a = builder; // implicit
    Assert.Equal(2, a.Count);
    Assert.Equal(11, a[1]);
  }

  [Fact]
  public void Equality_SameContents_IsTrue()
  {
    var a1 = StructuralArray.Create(1, 2, 3);
    var a2 = StructuralArray.Create(1, 2, 3);
    Assert.True(a1 == a2);
    Assert.True(a1.Equals(a2));
    Assert.Equal(a1.GetHashCode(), a2.GetHashCode());
  }

  [Fact]
  public void Equality_DifferentContents_IsFalse()
  {
    var a1 = StructuralArray.Create(1, 2, 3);
    var a2 = StructuralArray.Create(1, 2, 4);
    Assert.True(a1 != a2);
    Assert.False(a1.Equals(a2));
  }

  [Fact]
  public void HashSet_Deduplicates_BySequence()
  {
    var set = new HashSet<StructuralArray<int>>
    {
      StructuralArray.Create(1, 2),
      StructuralArray.Create(1, 2),
      StructuralArray.Create(2, 1)
    };
    Assert.Equal(2, set.Count); // (1,2) and (2,1)
  }

  private static readonly int[] _expected2 = [1, 2];

  [Fact]
  public void Add_DoesNotMutateOriginal()
  {
    var original = StructuralArray.Create(1, 2);
    var added = original.Add(3);
    Assert.Equal(2, original.Count);
    Assert.Equal(3, added.Count);
    Assert.Equal(_expected2, original.AsSpan.ToArray());
    Assert.Equal([1, 2, 3], added.AsSpan.ToArray());
  }

  private static readonly int[] _expected3 = [1, 3];

  [Fact]
  public void Remove_RemovesItem()
  {
    var original = StructuralArray.Create(1, 2, 3);
    var removed = original.Remove(2);
    Assert.Equal(_expected3, removed.AsSpan.ToArray());
    Assert.Equal([1, 2, 3], original.AsSpan.ToArray());
  }

  private static readonly int[] _expected4 = [1, 42, 3];

  [Fact]
  public void Replace_ReplacesItem()
  {
    var original = StructuralArray.Create(1, 2, 3);
    var replaced = original.Replace(2, 42, EqualityComparer<int>.Default);
    Assert.Equal(_expected4, replaced.AsSpan.ToArray());
  }

  private static readonly string[] _expected5 = ["a", "b", "c"];

  [Fact]
  public void Enumerator_IteratesInOrder()
  {
    var array = StructuralArray.Create("a", "b", "c");
    var list = new List<string>();
    foreach (var s in array)
      list.Add(s);
    Assert.Equal(_expected5, list);
  }

  [Fact]
  public void Indexer_ReturnsCorrectItem()
  {
    var array = StructuralArray.Create('x', 'y');
    Assert.Equal('y', array[1]);
  }
}
