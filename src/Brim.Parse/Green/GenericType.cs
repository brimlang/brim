using Brim.Parse.Collections;

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
    StructuralArray<GenericArgument> arr =
      Delimited.ParseCommaSeparatedTypes(
        p,
        static p2 =>
        {
          GreenToken headTok2 = p2.ExpectSyntax(SyntaxKind.IdentifierToken);
          GreenNode typeNode2 = headTok2;
          if (p2.MatchRaw(RawKind.LBracket))
            typeNode2 = ParseAfterName(p2, headTok2);
          return typeNode2;
        },
        static (n, c) => new GenericArgument(n, c),
        RawKind.RBracket);

    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    if (empty)
      p.AddDiagEmptyGeneric(open);
    return new GenericArgumentList(open, arr, close);
  }
}
