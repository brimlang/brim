using System.Collections.Generic;
using Brim.Core;
using Brim.Core.Collections;

namespace Brim.Lex.Tests;

public class CharacterTableTests
{
  public static IEnumerable<object[]> GlyphData()
  {
    CharacterTable.Entry[] entries = CharacterTable.AsSpan().ToArray();
    for (int i = 0; i < entries.Length; i++)
    {
      CharacterTable.Entry entry = entries[i];
      if (entry.SingleKind == TokenKind.Unitialized)
        continue;

      char singleChar = (char)i;
      yield return new object[] { singleChar.ToString(), entry.SingleKind, 1 };

      if (!entry.IsMulti)
        continue;

      foreach (CharacterTable.Sequence sequence in entry.Sequences)
        yield return new object[] { sequence.Seq.ToString(), sequence.Kind, sequence.Seq.Length };
    }
  }

  [Theory]
  [MemberData(nameof(GlyphData))]
  public void TryMatch_MatchesRegisteredGlyphs(string glyph, TokenKind expectedKind, int expectedLength)
  {
    bool matched = CharacterTable.TryMatch(glyph.AsSpan(), out TokenKind kind, out int length);

    Assert.True(matched);
    Assert.Equal(expectedKind, kind);
    Assert.Equal(expectedLength, length);
  }

  [Fact]
  public void TryMatch_ReturnsFalseForUnknownCharacter()
  {
    bool matched = CharacterTable.TryMatch("Ω".AsSpan(), out TokenKind kind, out int length);

    Assert.False(matched);
    Assert.Equal(TokenKind.Unitialized, kind);
    Assert.Equal(0, length);
  }

  [Fact]
  public void EntriesAreSortedByDescendingSequenceLength()
  {
    foreach (CharacterTable.Entry entry in CharacterTable.AsSpan())
    {
      if (!entry.IsMulti)
        continue;

      int lastLength = int.MaxValue;
      foreach (CharacterTable.Sequence sequence in entry.Sequences)
      {
        char lead = sequence.Seq.Length > 0 ? sequence.Seq[0] : '?';
        Assert.True(sequence.Seq.Length <= lastLength,
          $"Sequences for {lead} must be sorted by descending length.");
        lastLength = sequence.Seq.Length;
      }
    }
  }

  [Fact]
  public void SingleKindQueriesDistinguishSimpleGlyphs()
  {
    Assert.Equal(TokenKind.Unitialized, CharacterTable.GetSingleKind('Ω'));
    Assert.True(CharacterTable.IsSingleKind('['));
    Assert.False(CharacterTable.IsSingleKind('|'));
  }
}
