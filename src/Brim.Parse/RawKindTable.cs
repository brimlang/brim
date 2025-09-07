using System.Runtime.CompilerServices;

namespace Brim.Parse;

public readonly record struct RawKindSequenceEntry(CharSequence Seq, RawKind Kind);

public readonly record struct RawKindTableEntry(RawKind SingleKind, ImmutableArray<RawKindSequenceEntry> Sequences)
{
  public bool IsMulti => Sequences.IsDefault ? false : Sequences.Length > 0;
}

public static class RawKindTable
{
  internal static int MaxEntries => 128; // ASCII range

#pragma warning disable IDE0032 // Use auto property
  static readonly RawKindTableEntry[] _lookup = new RawKindTableEntry[MaxEntries];
#pragma warning restore IDE0032

  static RawKindTable()
  {
    _lookup['@'] = new(RawKind.Atmark,
        [new("@{", RawKind.AtmarkLBrace),
        new("@}", RawKind.AtmarkRBrace),
        new("@>", RawKind.AtmarkGreater)]);
    _lookup[':'] = new(RawKind.Colon,
        [new(":*", RawKind.ColonStar),
        new(":=", RawKind.ColonEqual),
        new("::", RawKind.ColonColon)]);
    _lookup['<'] = new(RawKind.Less,
        [new("<<", RawKind.LessLess),
        new("<@", RawKind.LessAt)]);
    _lookup['='] = new(RawKind.Equal, [new("=>", RawKind.EqualGreater)]);
    _lookup['*'] = new(RawKind.Star, [new("*{", RawKind.StarLBrace)]);
    _lookup['~'] = new(RawKind.Tilde, [new("~=", RawKind.TildeEqual)]);
    _lookup['|'] = new(RawKind.Pipe, [new("|{", RawKind.PipeLBrace)]);
    _lookup['#'] = new(RawKind.Hash, [new("#(", RawKind.HashLParen)]);
    _lookup['%'] = new(RawKind.Percent, [new("%{", RawKind.PercentLBrace)]);
    _lookup['['] = new(RawKind.LBracket, [new("[[", RawKind.LBracketLBracket)]);
    _lookup[']'] = new(RawKind.RBracket, [new("]]", RawKind.RBracketRBracket)]);
    _lookup['?'] = new(RawKind.Question, [new("?(", RawKind.QuestionLParen)]);
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
    _lookup['.'] = new(RawKind.Stop, []);
    _lookup['\\'] = new(RawKind.Backslash, []);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryGetEntry(char c, out RawKindTableEntry entry)
  {
    if (c < MaxEntries)
    {
      entry = _lookup[c];
      return entry.SingleKind != RawKind.Default;
    }

    entry = default;
    return false;
  }

  public static bool TryMatch(ReadOnlySpan<char> span, out RawKind kind, out int matchedLength)
  {
    if (span.Length == 0 || !TryGetEntry(span[0], out RawKindTableEntry entry))
    {
      kind = RawKind.Default;
      matchedLength = 0;
      return false;
    }

    foreach (RawKindSequenceEntry seq in entry.Sequences)
    {
      if (seq.Seq.PrefixMatch(span))
      {
        kind = seq.Kind;
        matchedLength = seq.Seq.Length;
        return true;
      }
    }

    kind = entry.SingleKind;
    matchedLength = 1;
    return true;
  }

#pragma warning disable IDE0032 // Use auto property
  internal static RawKindTableEntry[] Lookup => _lookup;
#pragma warning restore IDE0032
}

