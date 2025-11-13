using Brim.Core.Collections;

namespace Brim.Core.Tests;

public class StructuralArrayTests
{
  [Fact]
  public void Equals_ComparesContents()
  {
    StructuralArray<int> left = [.. new[] { 1, 2, 3 }];
    StructuralArray<int> right = [.. ImmutableArray.Create(1, 2, 3)];

    Assert.Equal(left, right);
    Assert.True(left.AsSpan.SequenceEqual(right.AsSpan));
    Assert.Equal(3, left.Count);
  }

  [Fact]
  public void DefaultAndEmptyHaveDifferentRepresentations()
  {
    StructuralArray<int> @default = default;
    StructuralArray<int> empty = [];

    Assert.True(@default.Equals(default));
    Assert.False(@default.Equals(empty));
    Assert.True(empty.IsEmpty);
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
    Assert.Equal(0, @default.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
    Assert.Empty(empty);
  }

  private static readonly int[] _items = [5];

  [Fact]
  public void ArrayBuilder_ConvertsToStructuralAndImmutable()
  {
    var builder = new ArrayBuilder<int>(capacity: 2)
    {
      4,
    };
    builder.AddRange(_items);

    StructuralArray<int> structural = builder;
    ImmutableArray<int> immutable = builder;

    Assert.True(structural.AsSpan.SequenceEqual([4, 5]));
    Assert.True(structural.AsImmutableArray.SequenceEqual(immutable));
  }
}
