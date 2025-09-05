namespace Brim.Parse;

/// <summary>
/// Simple pull-based token producer. Each call to <see cref="TryRead"/> yields the next token.
/// Must emit a single EOF token (RawTokenKind.Eof) exactly once, then return false thereafter.
/// </summary>
public interface ITokenProducer<T> where T : struct
{
  /// <summary>Attempts to read the next token. Returns true if <paramref name="item"/> contains a token (including EOF) not returned previously.</summary>
  bool TryRead(out T item);
}
