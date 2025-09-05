using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Brim.Parse;

public static class StructuralArray
{
  public static StructuralArray<T> Empty<T>() => new([]);

#pragma warning disable IDE0028, IDE0306 // Collection expression can be simplified
  public static StructuralArray<T> Create<T>(params ReadOnlySpan<T> items) => new(items.ToArray());
#pragma warning restore IDE0028, IDE0306
}

[CollectionBuilder(typeof(StructuralArray), "Create")]
[DebuggerDisplay("Count = {Count}")]
/// <summary>
/// An immutable array that compares its contents for equality and hashing.
/// </summary>
public readonly struct StructuralArray<T> : IImmutableList<T>, IEquatable<StructuralArray<T>>
{
  private readonly ImmutableArray<T> _array;

  /// <summary>
  /// Initializes a new instance of <see cref="StructuralArray{T}"/> from an <see cref="ImmutableArray{T}"/>.
  /// </summary>
  /// <param name="array">The immutable array to wrap.</param>
  public StructuralArray(ImmutableArray<T> array) => _array = array;

  /// <summary>
  /// Initializes a new instance of <see cref="StructuralArray{T}"/> from an <see cref="IEnumerable{T}"/>.
  /// </summary>
  /// <param name="items">The items to include.</param>
  public StructuralArray(IEnumerable<T> items) => _array = [.. items];

#pragma warning disable IDE0028, IDE0306 // Collection expression can be simplified
  public static implicit operator StructuralArray<T>(T[] array) => new(array);
  public static implicit operator StructuralArray<T>(ImmutableArray<T> array) => new(array);
  public static implicit operator StructuralArray<T>(ImmutableArray<T>.Builder builder) => new(builder.ToImmutable());
#pragma warning restore IDE0028, IDE0306

  public int Count => _array.Length;
  public T this[int index] => _array[index];

  public static bool operator ==(StructuralArray<T> left, StructuralArray<T> right) => left.Equals(right);
  public static bool operator !=(StructuralArray<T> left, StructuralArray<T> right) => !(left == right);

  public bool Equals(StructuralArray<T> other)
  {
    if (Count != other.Count) return false;
    EqualityComparer<T> comparer = EqualityComparer<T>.Default;
    for (int i = 0; i < Count; i++)
    {
      if (!comparer.Equals(this[i], other[i]))
        return false;
    }
    return true;
  }

  public override bool Equals(object? obj)
    => obj is StructuralArray<T> other && Equals(other);

  public override int GetHashCode()
  {
    HashCode hash = new();
    foreach (T? item in _array)
      hash.Add(item);

    return hash.ToHashCode();
  }

  public StructuralArray<T> Add(T value) => [.. _array.Add(value)];
  public StructuralArray<T> AddRange(IEnumerable<T> items) => [.. _array.AddRange(items)];
  public StructuralArray<T> Insert(int index, T element) => [.. _array.Insert(index, element)];
  public StructuralArray<T> InsertRange(int index, IEnumerable<T> items) => [.. _array.InsertRange(index, items)];
  public StructuralArray<T> Remove(T value, IEqualityComparer<T>? equalityComparer = null) => [.. _array.Remove(value, equalityComparer)];
  public StructuralArray<T> RemoveAll(Predicate<T> match) => [.. _array.RemoveAll(match)];
  public StructuralArray<T> RemoveAt(int index) => [.. _array.RemoveAt(index)];
  public StructuralArray<T> SetItem(int index, T value) => [.. _array.SetItem(index, value)];
  public int IndexOf(T item, int startIndex, IEqualityComparer<T>? equalityComparer) => _array.IndexOf(item, startIndex, equalityComparer);
  public int IndexOf(T item, IEqualityComparer<T>? equalityComparer) => _array.IndexOf(item, equalityComparer);
  public int LastIndexOf(T item, IEqualityComparer<T>? equalityComparer) => _array.LastIndexOf(item, equalityComparer);
  public ImmutableArray<T>.Enumerator GetEnumerator() => _array.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_array).GetEnumerator();
  IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

  int IImmutableList<T>.IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) => _array.IndexOf(item, index, count, equalityComparer);
  int IImmutableList<T>.LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) => _array.LastIndexOf(item, index, count, equalityComparer);
  IImmutableList<T> IImmutableList<T>.Add(T value) => Add(value);
  IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items) => AddRange(items);
  IImmutableList<T> IImmutableList<T>.Clear() => new StructuralArray<T>([]);
  IImmutableList<T> IImmutableList<T>.Insert(int index, T element) => Insert(index, element);
  IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items) => InsertRange(index, items);
  IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T>? equalityComparer) => Remove(value, equalityComparer);
  IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match) => RemoveAll(match);
  IImmutableList<T> IImmutableList<T>.RemoveAt(int index) => RemoveAt(index);
  IImmutableList<T> IImmutableList<T>.SetItem(int index, T value) => SetItem(index, value);
  IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer) => ((IImmutableList<T>)_array).RemoveRange(items, equalityComparer);
  IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count) => ((IImmutableList<T>)_array).RemoveRange(index, count);
  IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer) => ((IImmutableList<T>)_array).Replace(oldValue, newValue, equalityComparer);
}
