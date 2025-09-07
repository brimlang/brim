namespace Brim.Parse.Green;

public sealed record GenericType(
  Identifier Name,
  GenericArgumentList Arguments) :
GreenNode(SyntaxKind.GenericType, Name.Offset)
{
  public override int FullWidth => Arguments.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Arguments;
  }

  public static GenericType ParseAfterName(Parser p, Identifier name)
  {
    GenericArgumentList args = ParseArgList(p);
    return new GenericType(name, args);
  }

  static GenericArgumentList ParseArgList(Parser p)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    bool empty = p.MatchRaw(RawKind.RBracket);

    ImmutableArray<GreenNode>.Builder builder = ImmutableArray.CreateBuilder<GreenNode>();
    while (!empty && !p.MatchRaw(RawKind.RBracket) && !p.MatchRaw(RawKind.Eob))
    {
      Identifier head = Identifier.Parse(p);
      GreenNode typeNode = head;
      if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
      {
        // nested generic
        typeNode = ParseAfterName(p, head);
      }

      builder.Add(typeNode);
      if (p.MatchRaw(RawKind.Comma))
      {
        _ = p.ExpectRaw(RawKind.Comma);
        continue;
      }

      break;
    }

    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    if (empty)
      p.AddDiagEmptyGeneric(open);

    StructuralArray<GreenNode> arr = builder.ToImmutable();
    return new GenericArgumentList(open, arr, close);
  }
}
