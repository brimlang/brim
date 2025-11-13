using System.Runtime.CompilerServices;

namespace Brim.Lex;

/// <summary>
/// A static table mapping ASCII characters to their corresponding single-character and multi-character raw kinds.
/// </summary>
public static class CharacterTable
{
  public const int MaxEntries = 128; // Basic ASCII range - No Unicode

  static readonly Entry[] _lookup = new Entry[MaxEntries];

  static CharacterTable()
  {
    _lookup[':'] = new(TokenKind.Colon,
      [
        ("::=", TokenKind.ColonColonEqual),
        ("::", TokenKind.ColonColon),
        (":>", TokenKind.ColonGreater),
        (":=", TokenKind.ColonEqual)
      ]);
    _lookup['!'] = new(TokenKind.Bang,
      [
        ("!!{", TokenKind.BangBangLBrace),
        ("!{", TokenKind.BangLBrace),
        ("!=", TokenKind.BangEqual)
      ]);
    _lookup['|'] = new(TokenKind.Pipe,
      [
        ("||>", TokenKind.PipePipeGreater),
        ("|>", TokenKind.PipeGreater),
        ("||", TokenKind.PipePipe),
        ("|{", TokenKind.PipeLBrace),
        ("|(", TokenKind.PipeLParen)
      ]);
    _lookup['&'] = new(TokenKind.Ampersand,
      [
        ("&&", TokenKind.AmpersandAmpersand),
        ("&{", TokenKind.AmpersandLBrace),
        ("&(", TokenKind.AmpersandLParen)
      ]);
    _lookup['?'] = new(TokenKind.Question,
      [
        ("??", TokenKind.QuestionQuestion),
        ("?{", TokenKind.QuestionLBrace)
      ]);
    _lookup['#'] = new(TokenKind.Hash,
      [
        ("#{", TokenKind.HashLBrace),
        ("#(", TokenKind.HashLParen)
      ]);
    _lookup['<'] = new(TokenKind.Less,
      [
        ("<=", TokenKind.LessEqual),
        ("<<", TokenKind.LessLess)
      ]);
    _lookup['.'] = new(TokenKind.Stop,
      [
        (".{", TokenKind.StopLBrace),
        ("..", TokenKind.StopStop)
      ]);
    _lookup['@'] = new(TokenKind.Atmark,
      [
        ("@{", TokenKind.AtmarkLBrace),
        ("@(", TokenKind.AtmarkLParen)
      ]);
    _lookup['='] = new(TokenKind.Equal,
      [
        ("=[", TokenKind.EqualLBracket),
        ("==", TokenKind.EqualEqual),
        ("=>", TokenKind.EqualGreater)
      ]);
    _lookup['%'] = new(TokenKind.Percent,
      [
        ("%{", TokenKind.PercentLBrace),
        ("%(", TokenKind.PercentLParen)
      ]);
    _lookup['>'] = new(TokenKind.Greater,
      [
        (">>", TokenKind.GreaterGreater),
        (">=", TokenKind.GreaterEqual)
      ]);
    _lookup['*'] = new(TokenKind.Star,
      [
        ("*{", TokenKind.StarLBrace)
      ]);
    _lookup['~'] = new(TokenKind.Tilde,
      [
        ("~=", TokenKind.TildeEqual)
      ]);
    _lookup['['] = new(TokenKind.LBracket, []);
    _lookup[']'] = new(TokenKind.RBracket,
      [
        ("]=", TokenKind.RBracketEqual)
      ]);
    _lookup['-'] = new(TokenKind.Minus,
      [
        ("->", TokenKind.MinusGreater)
      ]);
    _lookup['('] = new(TokenKind.LParen, []);
    _lookup[')'] = new(TokenKind.RParen, []);
    _lookup['{'] = new(TokenKind.LBrace, []);
    _lookup['}'] = new(TokenKind.RBrace, []);
    _lookup[','] = new(TokenKind.Comma, []);
    _lookup['^'] = new(TokenKind.Hat, []);
    _lookup['+'] = new(TokenKind.Plus, []);
    _lookup['/'] = new(TokenKind.Slash, []);
    _lookup['\\'] = new(TokenKind.Backslash, []);

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

  /// <summary>
  /// Attempts to retrieve the table entry for the ASCII character <paramref name="c"/>.
  /// </summary>
  /// <param name="c">The character to look up.</param>
  /// <param name="entry">The resulting table entry when the lookup succeeds.</param>
  /// <returns><c>true</c> if the character is registered; otherwise <c>false</c>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryGetEntry(char c, out Entry entry)
  {
    if (c < MaxEntries)
    {
      entry = _lookup[c];
      return entry.SingleKind != TokenKind.Unitialized;
    }

    entry = default;
    return false;
  }

  /// <summary>
  /// Gets the single-character raw kind for <paramref name="c"/> or <see cref="TokenKind.Default"/> if none exists.
  /// </summary>
  /// <param name="c">The character to look up.</param>
  /// <returns>The associated single-character <see cref="TokenKind"/> or <see cref="TokenKind.Default"/>.</returns>
  public static TokenKind GetSingleKind(char c) =>
    TryGetEntry(c, out Entry entry)
    ? entry.SingleKind
    : TokenKind.Unitialized;

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
  /// <param name="kind">On success, the matched <see cref="TokenKind"/>.</param>
  /// <param name="matchedLength">On success, the number of characters consumed.</param>
  /// <returns><c>true</c> if a kind was matched; otherwise <c>false</c>.</returns>
  public static bool TryMatch(ReadOnlySpan<char> span, out TokenKind kind, out int matchedLength)
  {
    if (span.Length == 0 || !TryGetEntry(span[0], out Entry entry))
    {
      kind = TokenKind.Unitialized;
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
    TokenKind Kind)
  {
    public static implicit operator Sequence((string seq, TokenKind kind) tuple) =>
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
    TokenKind SingleKind,
    ImmutableArray<Sequence> Sequences)
  {
    /// <summary>
    /// Gets a value indicating whether this entry has multi-character sequences.
    /// </summary>
    public bool IsMulti => !Sequences.IsDefaultOrEmpty;
  }
}
