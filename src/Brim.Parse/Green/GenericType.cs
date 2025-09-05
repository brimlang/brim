namespace Brim.Parse.Green;

public sealed record GenericType(
  Identifier Name,
  GenericArgumentList Arguments) : GreenNode(SyntaxKind.GenericType, Name.Offset)
{
  public override int FullWidth => Arguments.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name; yield return Arguments;
  }

  public static GenericType ParseAfterName(Parser p, Identifier name)
  {
    GenericArgumentList args = ParseArgList(p);
    return new GenericType(name, args);
  }

  static GenericArgumentList ParseArgList(Parser p)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    bool empty = p.Match(RawTokenKind.RBracket);
    ImmutableArray<GreenNode>.Builder builder = ImmutableArray.CreateBuilder<GreenNode>();
    while (!empty && !p.Match(RawTokenKind.RBracket) && !p.Match(RawTokenKind.Eob))
    {
      Identifier head = Identifier.Parse(p);
      GreenNode typeNode = head;
      if (p.Match(RawTokenKind.LBracket) && !p.Match(RawTokenKind.LBracketLBracket))
      {
        // nested generic
        typeNode = ParseAfterName(p, head);
      }
      builder.Add(typeNode);
      if (p.Match(RawTokenKind.Comma))
      {
        _ = p.Expect(RawTokenKind.Comma);
        continue;
      }
      break;
    }
    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    if (empty)
    {
      p.AddDiagEmptyGenericArgList(open);
    }
    StructuralArray<GreenNode> arr = builder.ToImmutable();
    return new GenericArgumentList(open, arr, close);
  }
}
