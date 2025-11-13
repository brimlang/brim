using Brim.Core.Collections;

namespace Brim.Core.Tests;

public class ByteSequenceTests
{
  [Fact]
  public void From_TruncatesAfterMaxLength()
  {
    byte[] source = "ABCD"u8.ToArray();
    ByteSequence sequence = ByteSequence.From(source);

    Assert.Equal(ByteSequence.MaxLength, sequence.Length);
    Assert.Equal((byte)'A', sequence[0]);
    Assert.Equal((byte)'B', sequence[1]);
    Assert.Equal((byte)'C', sequence[2]);
  }

  [Fact]
  public void PrefixMatch_ValidatesInputLength()
  {
    ByteSequence sequence = new((byte)'+', (byte)'+', (byte)'>');

    Assert.False(sequence.PrefixMatch("+"u8));
    Assert.False(sequence.PrefixMatch("+->"u8));
    Assert.True(sequence.PrefixMatch("++>"u8));
    Assert.True(sequence.PrefixMatch("++>="u8));
  }

  [Fact]
  public void TryFormat_WritesAsciiRepresentation()
  {
    ByteSequence sequence = new((byte)'<', (byte)'=');

    Span<char> tiny = stackalloc char[1];
    Assert.False(sequence.TryFormat(tiny, out int writtenTiny));
    Assert.Equal(0, writtenTiny);

    Span<char> buffer = stackalloc char[ByteSequence.MaxLength];
    Assert.True(sequence.TryFormat(buffer, out int written));
    Assert.Equal(2, written);
    Assert.Equal("<=", new string(buffer[..written]));
    Assert.Equal("<=", sequence.ToString());
  }

  [Fact]
  public void Equals_RequiresMatchingLengthAndBytes()
  {
    ByteSequence sequence = new((byte)'|', (byte)'>');
    ByteSequence equal = new((byte)'|', (byte)'>');
    ByteSequence different = new((byte)'|', (byte)'>', (byte)'>');

    Assert.True(sequence.Equals(equal));
    Assert.True(sequence.Equals("|>"u8));
    Assert.False(sequence.Equals(different));
    Assert.False(sequence.Equals("|>>"u8));
  }
}
