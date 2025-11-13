using Brim.Core.Collections;

namespace Brim.Core.Tests;

public class CharSequenceTests
{
  [Fact]
  public void From_TruncatesAfterMaxLength()
  {
    CharSequence sequence = CharSequence.From("abcd");

    Assert.Equal(CharSequence.MaxLength, sequence.Length);
    Assert.Equal('a', sequence[0]);
    Assert.Equal('b', sequence[1]);
    Assert.Equal('c', sequence[2]);
  }

  [Fact]
  public void PrefixMatch_ValidatesInput()
  {
    CharSequence sequence = new('[', '[', '{');

    Assert.False(sequence.PrefixMatch("["));
    Assert.False(sequence.PrefixMatch("[]]"));
    Assert.True(sequence.PrefixMatch("[[{"));
    Assert.True(sequence.PrefixMatch("[[{rest"));
  }

  [Fact]
  public void EqualsSpan_RespectsLength()
  {
    CharSequence sequence = new('?', '?');

    Assert.True(sequence.Equals("??".AsSpan()));
    Assert.False(sequence.Equals("?".AsSpan()));
  }

  [Fact]
  public void TryFormat_WritesCharacters()
  {
    CharSequence sequence = new('|', '>');
    Span<char> buffer = stackalloc char[CharSequence.MaxLength];

    Assert.True(sequence.TryFormat(buffer, out int written));
    Assert.Equal(2, written);
    Assert.Equal("|>", new string(buffer[..written]));
    Assert.Equal("|>", sequence.ToString());
  }
}
