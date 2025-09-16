namespace Brim.Parse.Green;

delegate T ParseFunc<out T>(Parser p) where T : GreenNode;
delegate GreenNode ParseFunc(Parser p);

interface IParsable<out T> where T : GreenNode
{
  static abstract T Parse(Parser p);
}

public abstract record GreenNode(SyntaxKind Kind, int Offset)
{
  public int EndOffset => Offset + FullWidth;

  public ReadOnlySpan<char> GetChars(ReadOnlySpan<char> source) => source.Slice(Offset, FullWidth);
  public string GetText(ReadOnlySpan<char> source) => GetChars(source).ToString();

  public abstract int FullWidth { get; }
  public abstract IEnumerable<GreenNode> GetChildren();
}
