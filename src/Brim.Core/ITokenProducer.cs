namespace Brim.Core;

/// <summary>
/// Simple pull-based token producer. Each call to <see cref="TryRead"/>
/// yields the next token. Must emit a single end-of-source token
/// exactly once, then return false thereafter.
/// </summary>
public interface ITokenSource<T> where T : struct
{
  /// <summary>
  /// Returns true if the given item is the last item in the source.
  /// </summary>
  /// <param name="item">The item to check.</param>
  /// <returns><see langword="true" /> if the item is the last item; <see langword="false" /> otherwise.</returns>
  bool IsEndOfSource(in T item);

  /// <summary>
  /// Attempts to read the next item from the source.
  /// </summary>
  /// <param name="item">The next item, if available.</param>
  /// <returns><see langword="true" /> if an item was read; <see langword="false"> if the source is exhausted.</returns>
  bool TryRead(out T item);
}

public interface ITokenSink<T> where T : struct
{
  bool Consume(in ITokenSource<T> source);
}
