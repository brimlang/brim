namespace Brim.Parse.Green;

interface IParsable<out T> where T : GreenNode
{
  static abstract T Parse(ref Parser p);
}

public abstract record GreenNode(SyntaxKind Kind, int Offset)
{
  public int EndOffset => Offset + FullWidth;
  public StructuralArray<Diagnostic> Diagnostics { get; init; } = [];

  public ReadOnlySpan<char> GetChars(ReadOnlySpan<char> source) => source.Slice(Offset, FullWidth);
  public string GetText(ReadOnlySpan<char> source) => GetChars(source).ToString();

  public abstract int FullWidth { get; }
  public abstract IEnumerable<GreenNode> GetChildren();
}
