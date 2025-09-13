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

    ImmutableArray<GreenNode>.Builder builder = ImmutableArray.CreateBuilder<GreenNode>();
    ImmutableArray<GreenToken>.Builder seps = ImmutableArray.CreateBuilder<GreenToken>();
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
        builder.Add(typeNode);
        if (p.MatchRaw(RawKind.Comma))
        {
          GreenToken commaTok = p.ExpectSyntax(SyntaxKind.CommaToken);
          seps.Add(commaTok);
          if (p.MatchRaw(RawKind.RBracket) || p.MatchRaw(RawKind.Eob))
            break; // trailing comma
          continue;
        }
        break;
      }
    }

    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    if (empty)
      p.AddDiagEmptyGeneric(open);

    StructuralArray<GreenNode> arr = builder.ToImmutable();
    StructuralArray<GreenToken> sepArr = [.. seps];
    return new GenericArgumentList(open, arr, sepArr, close);
  }
}
