using System.Diagnostics;
using System.Runtime.CompilerServices;
using Brim.Parse.Collections;

namespace Brim.Parse;

/// <summary>
/// A static table mapping ASCII characters to their corresponding single-character and multi-character raw kinds.
/// </summary>
public static class RawKindTable
{
  public const int MaxEntries = 128; // Basic ASCII range - No Unicode

  static readonly Entry[] _lookup = new Entry[MaxEntries];

  static RawKindTable()
  {
    _lookup[':'] = new(RawKind.Colon,
      [
        (":*", RawKind.ColonStar),
        (":=", RawKind.ColonEqual),
        ("::", RawKind.ColonColon)
      ]);
    _lookup['!'] = new(RawKind.Bang,
      [
        ("!!{", RawKind.BangBangLBrace),
        ("!{", RawKind.BangLBrace),
        ("!=", RawKind.BangEqual)
      ]);
    _lookup['<'] = new(RawKind.Less,
      [
        ("<<", RawKind.LessLess)
      ]);
    _lookup['?'] = new(RawKind.Question,
      [
        ("?(", RawKind.QuestionLParen),
        ("?{", RawKind.QuestionLBrace)
      ]);
    _lookup['='] = new(RawKind.Equal, [("=>", RawKind.EqualGreater)]);
    _lookup['*'] = new(RawKind.Star, [("*{", RawKind.StarLBrace)]);
    _lookup['~'] = new(RawKind.Tilde, [("~=", RawKind.TildeEqual)]);
    _lookup['|'] = new(RawKind.Pipe, [("|{", RawKind.PipeLBrace)]);
    _lookup['#'] = new(RawKind.Hash, [("#{", RawKind.HashLBrace), ("#(", RawKind.HashLParen)]);
    _lookup['%'] = new(RawKind.Percent, [("%{", RawKind.PercentLBrace)]);
    _lookup['['] = new(RawKind.LBracket, [("[[", RawKind.LBracketLBracket)]);
    _lookup[']'] = new(RawKind.RBracket, [("]]", RawKind.RBracketRBracket)]);
    _lookup['.'] = new(RawKind.Stop, [(".{", RawKind.StopLBrace)]);
    _lookup['@'] = new(RawKind.Atmark, []);
    _lookup['-'] = new(RawKind.Minus, []);
    _lookup['&'] = new(RawKind.Ampersand, []);
    _lookup['('] = new(RawKind.LParen, []);
    _lookup[')'] = new(RawKind.RParen, []);
    _lookup['{'] = new(RawKind.LBrace, []);
    _lookup['}'] = new(RawKind.RBrace, []);
    _lookup[','] = new(RawKind.Comma, []);
    _lookup['^'] = new(RawKind.Hat, []);
    _lookup['+'] = new(RawKind.Plus, []);
    _lookup['>'] = new(RawKind.Greater, []);
    _lookup['/'] = new(RawKind.Slash, []);
    _lookup['\\'] = new(RawKind.Backslash, []);

#if DEBUG
    for (int i = 0; i < _lookup.Length; i++)
    {
      ImmutableArray<Sequence> seqs = _lookup[i].Sequences;
      if (seqs.IsDefault)
        continue;

      for (int j = 1; j < seqs.Length; j++)
        Debug.Assert(seqs[j - 1].Seq.Length >= seqs[j].Seq.Length, "Sequences must be sorted by descending length.");
    }
#endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  /// <summary>
  /// Attempts to retrieve the table entry for the ASCII character <paramref name="c"/>.
  /// </summary>
  /// <param name="c">The character to look up.</param>
  /// <param name="entry">The resulting table entry when the lookup succeeds.</param>
  /// <returns><c>true</c> if the character is registered; otherwise <c>false</c>.</returns>
  public static bool TryGetEntry(char c, out Entry entry)
  {
    if (c < MaxEntries)
    {
      entry = _lookup[c];
      return entry.SingleKind != RawKind.Default;
    }

    entry = default;
    return false;
  }

  /// <summary>
  /// Gets the single-character raw kind for <paramref name="c"/> or <see cref="RawKind.Default"/> if none exists.
  /// </summary>
  /// <param name="c">The character to look up.</param>
  /// <returns>The associated single-character <see cref="RawKind"/> or <see cref="RawKind.Default"/>.</returns>
  public static RawKind GetSingleKind(char c) =>
    TryGetEntry(c, out Entry entry)
    ? entry.SingleKind
    : RawKind.Default;

  /// <summary>
  /// Determines whether <paramref name="c"/> corresponds to a single-character token with no multi-character continuations.
  /// </summary>
  /// <param name="c">The character to test.</param>
  /// <returns><c>true</c> if the character maps only to a single-character token; otherwise <c>false</c>.</returns>
  public static bool IsSingleKind(char c) =>
    TryGetEntry(c, out Entry entry)
    && !entry.IsMulti;

  /// <summary>
  /// Attempts to match the longest raw kind at the start of <paramref name="span"/>.
  /// </summary>
  /// <param name="span">The input character span.</param>
  /// <param name="kind">On success, the matched <see cref="RawKind"/>.</param>
  /// <param name="matchedLength">On success, the number of characters consumed.</param>
  /// <returns><c>true</c> if a kind was matched; otherwise <c>false</c>.</returns>
  public static bool TryMatch(ReadOnlySpan<char> span, out RawKind kind, out int matchedLength)
  {
    if (span.Length == 0 || !TryGetEntry(span[0], out Entry entry))
    {
      kind = RawKind.Default;
      matchedLength = 0;
      return false;
    }

    if (entry.IsMulti)
    {
      foreach (Sequence seq in entry.Sequences)
      {
        if (seq.Seq.PrefixMatch(span))
        {
          kind = seq.Kind;
          matchedLength = seq.Seq.Length;
          return true;
        }
      }
    }

    kind = entry.SingleKind;
    matchedLength = 1;
    return true;
  }

  internal static ReadOnlySpan<Entry> AsSpan() => _lookup;

  /// <summary>
  /// An entry in the raw kind table for a compound glyph sequence.
  /// </summary>
  /// <param name="Seq">The character sequence.</param>
  /// <param name="Kind">The corresponding raw kind.</param>
  public readonly record struct Sequence(
    CharSequence Seq,
    RawKind Kind)
  {
    public static implicit operator Sequence((string seq, RawKind kind) tuple) =>
      new(CharSequence.From(tuple.seq), tuple.kind);
  }

  /// <summary>
  /// An entry in the raw kind table.
  /// </summary>
  /// <param name="SingleKind">The single-character raw kind.</param>
  /// <param name="Sequences">The possible compound sequences starting with this character.</param>
  /// <remarks>
  /// <paramref name="Sequences"/> must be sorted in descending order of length.
  /// </remarks>
  public readonly record struct Entry(
    RawKind SingleKind,
    ImmutableArray<Sequence> Sequences)
  {
    /// <summary>
    /// Gets a value indicating whether this entry has multi-character sequences.
    /// </summary>
    public bool IsMulti => !Sequences.IsDefaultOrEmpty;
  }

}

