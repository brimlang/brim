namespace Brim.Core.Tests;

public class SourceTextTests
{
  [Fact]
  public void From_SetsLengthAndSpan()
  {
    SourceText text = SourceText.From("let");

    Assert.Equal(3, text.Length);
    Assert.True(text.Span.SequenceEqual("let".AsSpan()));
    Assert.Equal(string.Empty, text.FilePath);
    Assert.Equal(0, text.Version);
  }

  private static readonly char[] _expected = ['a', 'b'];

  [Fact]
  public void Enumerator_YieldsCharacters()
  {
    SourceText text = SourceText.From("ab");
    List<char> chars = [.. text];

    Assert.Equal(_expected, chars);
  }

  [Fact]
  public void FromFile_ReadsUtf8AndCapturesFilePath()
  {
    string path = TestPaths.CreateTmpFile("SourceText", "λ brim");
    try
    {
      SourceText text = SourceText.FromFile(path);

      Assert.Equal(path, text.FilePath);
      Assert.Equal(6, text.Length);
      Assert.True(text.Span.SequenceEqual("λ brim".AsSpan()));
    }
    finally
    {
      TestPaths.DeleteIfExists(path);
    }
  }
}
