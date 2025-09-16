using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Brim.Parse.Collections;

/// <summary>
/// An immutable array that compares its contents for equality and hashing.
/// </summary>
public static class StructuralArray
{
  [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Prevent recursive call")]
  [SuppressMessage("Style", "IDE0301:Simplify collection initialization", Justification = "Prevent recursive call")]
  public static StructuralArray<T> Create<T>(params ReadOnlySpan<T> items)
  {
    return items.Length == 0
      ? StructuralArray<T>.Empty
      : new StructuralArray<T>(ImmutableArray.Create(items.ToArray()));
  }
}

[CollectionBuilder(typeof(StructuralArray), "Create")]
[DebuggerDisplay("Count = {Count}")]
/// <summary>
/// An immutable array that compares its contents for equality and hashing.
/// </summary>
public readonly struct StructuralArray<T> : IImmutableList<T>, IEquatable<StructuralArray<T>>
{
  readonly ImmutableArray<T> _array;

  /// <summary>
  /// An empty <see cref="StructuralArray{T}"/>.
  /// </summary>
  public static StructuralArray<T> Empty { get; } = ImmutableArray<T>.Empty;

  /// <summary>
  /// Gets the underlying <see cref="ImmutableArray{T}"/>.
  /// </summary>
  public ImmutableArray<T> AsImmutableArray => _array;

  /// <summary>
  /// Gets the underlying array as a <see cref="ReadOnlySpan{T}"/>.
  /// </summary>
  public ReadOnlySpan<T> AsSpan => _array.AsSpan();

  /// <summary>
  /// Gets the length of the underlying array.
  /// </summary>
  public int Length => _array.Length;

  /// <summary>
  /// Gets the number of elements in the collection.
  /// </summary>
  public int Count => _array.Length;

  /// <summary>
  /// Gets the element at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index of the element to get.</param>
  public T this[int index] => _array[index];

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

  /// <summary>
  /// Initializes a new instance of the <see cref="StructuralArray{T}"/> class with the specified items.
  /// </summary>
  /// <param name="items">The array of items to initialize the structural array with. If null, an empty array is used.</param>
  public StructuralArray(T[] items) => _array = items is null ? [] : ImmutableArray.Create(items);

#pragma warning disable IDE0028, IDE0306 // Collection expression can be simplified
  public static implicit operator StructuralArray<T>(T[] array) => new(array);
  public static implicit operator StructuralArray<T>(ImmutableArray<T> array) => new(array);
  public static implicit operator StructuralArray<T>(ImmutableArray<T>.Builder builder) => new(builder.ToImmutable());
#pragma warning restore IDE0028, IDE0306

  public static bool operator ==(StructuralArray<T> left, StructuralArray<T> right) => left.Equals(right);
  public static bool operator !=(StructuralArray<T> left, StructuralArray<T> right) => !(left == right);

  public bool Equals(StructuralArray<T> other)
  {
    int count = _array.Length;
    return count != other.Count
      ? false
      : _array.AsSpan().SequenceEqual(other._array.AsSpan());
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

  public StructuralArray<T> Add(T value) => _array.Add(value);
  public StructuralArray<T> AddRange(IEnumerable<T> items) => _array.AddRange(items);
  public StructuralArray<T> Clear() => Empty;
  public StructuralArray<T> Insert(int index, T element) => _array.Insert(index, element);
  public StructuralArray<T> InsertRange(int index, IEnumerable<T> items) => _array.InsertRange(index, items);
  public StructuralArray<T> Remove(T value, IEqualityComparer<T>? equalityComparer = null) => _array.Remove(value, equalityComparer);
  public StructuralArray<T> RemoveAll(Predicate<T> match) => _array.RemoveAll(match);
  public StructuralArray<T> RemoveAt(int index) => _array.RemoveAt(index);
  public StructuralArray<T> SetItem(int index, T value) => _array.SetItem(index, value);
  public StructuralArray<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer) => _array.RemoveRange(items, equalityComparer);
  public StructuralArray<T> RemoveRange(int index, int count) => _array.RemoveRange(index, count);
  public StructuralArray<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer) => _array.Replace(oldValue, newValue, equalityComparer);
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
  IImmutableList<T> IImmutableList<T>.Clear() => Clear();
  IImmutableList<T> IImmutableList<T>.Insert(int index, T element) => Insert(index, element);
  IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items) => InsertRange(index, items);
  IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T>? equalityComparer) => Remove(value, equalityComparer);
  IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match) => RemoveAll(match);
  IImmutableList<T> IImmutableList<T>.RemoveAt(int index) => RemoveAt(index);
  IImmutableList<T> IImmutableList<T>.SetItem(int index, T value) => SetItem(index, value);
  IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer) => RemoveRange(items, equalityComparer);
  IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count) => RemoveRange(index, count);
  IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer) => Replace(oldValue, newValue, equalityComparer);
}
