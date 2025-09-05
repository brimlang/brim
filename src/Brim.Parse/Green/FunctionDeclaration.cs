namespace Brim.Parse.Green;

public sealed record FunctionDeclaration(
  GreenToken Identifier,
  GreenToken Equal,
  ParameterList ParameterList,
  GreenNode ReturnType,
  Block Body)
: GreenNode(SyntaxKind.FunctionDeclaration, Identifier.Offset)
{
  public override int FullWidth => Body.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Equal;
    yield return ParameterList;
    yield return ReturnType;
    yield return Body;
  }
}

