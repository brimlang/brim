using System.Collections.ObjectModel;

namespace Brim.Parse.Collections;

/// <summary>
/// A builder for <see cref="StructuralArray{T}"/> and ImmutableArray{T},
/// similar to <see cref="ImmutableArray{T}.Builder"/> but with simpler syntax.
/// </summary>
[SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Preserve explicit constructor")]
public readonly ref struct ArrayBuilder<T> : IEnumerable<T>
{
  readonly ImmutableArray<T>.Builder _builder;

  public ArrayBuilder() => _builder = ImmutableArray.CreateBuilder<T>();
  public ArrayBuilder(int capacity) => _builder = ImmutableArray.CreateBuilder<T>(capacity);
  public static implicit operator StructuralArray<T>(ArrayBuilder<T> builder) => builder.ToStructuralArray();
  public static implicit operator ImmutableArray<T>(ArrayBuilder<T> builder) => builder.ToImmutable();

  public int Count => _builder.Count;
  public int Capacity {
    get => _builder.Capacity;
    set => _builder.Capacity = value;
  }

  public void Add(T item) => _builder.Add(item);
  public void AddRange(IEnumerable<T> items) => _builder.AddRange(items);
  public void Clear() => _builder.Clear();
  public bool Contains(T item, IEqualityComparer<T>? equalityComparer = null) => _builder.Contains(item, equalityComparer);
  public void CopyTo(T[] array, int arrayIndex) => _builder.CopyTo(array, arrayIndex);
  public void CopyTo(int sourceIndex, T[] array, int arrayIndex, int count) => _builder.CopyTo(sourceIndex, array, arrayIndex, count);

  public T[] ToArray() => _builder.ToArray();
  public StructuralArray<T> ToStructuralArray() => new(_builder.ToImmutable());
  public ImmutableArray<T> ToImmutable() => _builder.ToImmutable();
  public ImmutableList<T> ToImmutableList() => _builder.ToImmutableList();
  public ReadOnlyCollection<T> ToReadOnlyCollection() => _builder.AsReadOnly();

  IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_builder).GetEnumerator();
  IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_builder).GetEnumerator();
}
