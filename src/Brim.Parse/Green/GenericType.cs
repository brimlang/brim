namespace Brim.Parse.Green;

public sealed record GenericType(
  GreenToken Name,
  GenericArgumentList Arguments) :
GreenNode(SyntaxKind.GenericType, Name.Offset)
{
  public override int FullWidth => Arguments.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Arguments;
  }

  public static GenericType ParseAfterName(Parser p, GreenToken name)
  {
    GenericArgumentList args = ParseArgList(p);
    return new GenericType(name, args);
  }

  static GenericArgumentList ParseArgList(Parser p)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    bool empty = p.MatchRaw(RawKind.RBracket);

    ImmutableArray<GenericArgument>.Builder builder = ImmutableArray.CreateBuilder<GenericArgument>();
    if (!empty)
    {
      while (true)
      {
        GreenToken headTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
        GreenNode typeNode = headTok;
        if (p.MatchRaw(RawKind.LBracket))
        {
          // nested generic
          typeNode = ParseAfterName(p, headTok);
        }
        GreenToken? trailing = null;
        if (p.MatchRaw(RawKind.Comma))
          trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
        builder.Add(new GenericArgument(typeNode, trailing));
        if (trailing is null) break; // no comma => end
        if (p.MatchRaw(RawKind.RBracket) || p.MatchRaw(RawKind.Eob)) break; // trailing comma on last
        continue;
      }
    }

    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    if (empty)
      p.AddDiagEmptyGeneric(open);

    StructuralArray<GenericArgument> arr = [.. builder];
    return new GenericArgumentList(open, arr, close);
  }
}
