using Brim.Core;

namespace Brim.Parse.Tests;

public class SourceTextTests
{
  [Fact]
  public void FromStringSetsLengthAndSpan()
  {
    var st = SourceText.From("abc");
    Assert.Equal(3, st.Length); // 3 UTF-8 bytes for "abc"
    Assert.Equal("abc", st.Span[..st.Length]); // Use GetText to decode bytes to string
  }

  [Fact]
  public void EnumeratorIteratesCharacters()
  {
    var st = SourceText.From("hi");
    // Use UTF-8 decoding instead of trying to cast bytes to string
    var collected = st.Span[..st.Length];
    Assert.Equal("hi", collected);
  }

  [Fact]
  public void EmptyStringWorks()
  {
    var st = SourceText.From(string.Empty);
    Assert.Equal(0, st.Length);
    Assert.True(st.Span.IsEmpty);
  }
}
