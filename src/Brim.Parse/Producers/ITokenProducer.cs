namespace Brim.Parse.Producers;

/// <summary>
/// Simple pull-based token producer. Each call to <see cref="TryRead"/>
/// yields the next token. Must emit a single EOB token (RawKind.Eob)
/// exactly once, then return false thereafter.
/// </summary>
public interface ITokenProducer<T> where T : struct
{
  /// <summary>
  /// Attempts to read the next token.
  /// Returns true if <paramref name="item"/> contains a token (including EOB)
  /// not returned previously.
  /// </summary>
  bool TryRead(out T item);
}
