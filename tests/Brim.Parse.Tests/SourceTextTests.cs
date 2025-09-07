namespace Brim.Parse.Tests;

public class SourceTextTests
{
  [Fact]
  public void FromStringSetsLengthAndSpan()
  {
    var st = SourceText.From("abc");
    Assert.Equal(3, st.Length);
    Assert.Equal("abc", st.Span.ToString());
  }

  [Fact]
  public void EnumeratorIteratesCharacters()
  {
    var st = SourceText.From("hi");
    var collected = string.Concat(st.Span.ToArray());
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
