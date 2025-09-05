using System.Diagnostics;

namespace Brim.Parse;

/// <summary>
/// A token produced by the lexer.
/// </summary>
/// <param name="Kind">The kind of token.</param>
/// <param name="Offset">The offset of the token in the input buffer.</param>
/// <param name="Length">The length of the token in the input buffer.</param>
/// <param name="Line">The line number of the token (1-based).</param>
/// <param name="Column">The column number of the token (1-based).</param>
[DebuggerDisplay("{ToString(),nq}")]
public readonly record struct RawToken(
    RawTokenKind Kind,
    int Offset,
    int Length,
    int Line,
    int Column) : IToken
{
  /// <summary>
  /// Returns the token's text as a span from the given input buffer.
  /// </summary>
  /// <param name="input">The input buffer.</param>
  public ReadOnlySpan<char> Value(ReadOnlySpan<char> input) => input.Slice(Offset, Length);

  public override string ToString() => $"{Kind}@{Line}:{Column} [{Offset}..{Offset + Length})";
}

