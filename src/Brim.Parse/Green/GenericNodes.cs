using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record GenericParameterList(
  GreenToken Open,
  ImmutableArray<GenericParameter> Parameters,
  GreenToken Close)
: GreenNode(SyntaxKind.GenericParameterList, Open.Offset)
{
  public override int FullWidth => Close.EndOffset - Open.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    foreach (GenericParameter p in Parameters) yield return p;
    yield return Close;
  }
}

public sealed record GenericParameter(
  GreenToken Name,
  ConstraintList? Constraints,
  GreenToken? TrailingComma) : GreenNode(SyntaxKind.GenericParameter, Name.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? Constraints?.EndOffset ?? Name.EndOffset) - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    if (Constraints is not null) yield return Constraints;
    if (TrailingComma is not null) yield return TrailingComma;
  }
}

public sealed record ConstraintList(
  GreenToken Colon,
  StructuralArray<ConstraintRef> Constraints) : GreenNode(SyntaxKind.ConstraintList, Colon.Offset)
{
  public override int FullWidth => Constraints.Count == 0 ? Colon.FullWidth : Constraints[^1].EndOffset - Colon.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Colon;
    foreach (ConstraintRef c in Constraints) yield return c;
  }
}

public sealed record GenericArgument(
  GreenNode TypeNode
) : GreenNode(SyntaxKind.GenericArgument, TypeNode.Offset)
{
  public override int FullWidth => TypeNode.FullWidth;
  public override IEnumerable<GreenNode> GetChildren() { yield return TypeNode; }
}

public sealed record GenericArgumentList(
  CommaList<GenericArgument> ArgumentList
) : GreenNode(SyntaxKind.GenericArgumentList, ArgumentList.Offset)
{
  public override int FullWidth => ArgumentList.FullWidth;
  public override IEnumerable<GreenNode> GetChildren() => ArgumentList.GetChildren();

  public static GenericArgumentList Parse(Parser p)
  {
    CommaList<GenericArgument> list = CommaList<GenericArgument>.Parse(
      p,
      SyntaxKind.GenericOpenToken,
      SyntaxKind.GenericCloseToken,
      static p2 =>
      {
        GreenToken headTok2 = p2.ExpectSyntax(SyntaxKind.IdentifierToken);
        GreenNode typeNode2 = headTok2;
        if (p2.MatchRaw(RawKind.LBracket))
          typeNode2 = GenericType.ParseAfterName(p2, headTok2);
        return new GenericArgument(typeNode2);
      });

    return new GenericArgumentList(list);
  }
}
