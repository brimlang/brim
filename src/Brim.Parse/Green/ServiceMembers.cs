namespace Brim.Parse.Green;

public sealed record ServiceParam(
  GreenToken Name,
  GreenToken Colon,
  TypeExpr Type,
  GreenToken? TrailingComma)
  : GreenNode(SyntaxKind.FieldDeclaration, Name.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? Type.EndOffset) - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    foreach (GreenNode child in Type.GetChildren())
      yield return child;
    if (TrailingComma is not null) yield return TrailingComma;
  }
}

public sealed record ServiceCtorHeader(
  GreenToken Hat,
  GreenToken OpenParen,
  StructuralArray<ServiceParam> Parameters,
  GreenToken CloseParen,
  GreenToken BodyOpen)
  : GreenNode(SyntaxKind.FunctionDeclaration, Hat.Offset)
{
  public override int FullWidth => BodyOpen.EndOffset - Hat.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Hat;
    yield return OpenParen;
    foreach (ServiceParam p in Parameters) yield return p;
    yield return CloseParen;
    yield return BodyOpen;
  }
}

public sealed record ServiceMethodHeader(
  GreenToken Name,
  GreenToken OpenParen,
  StructuralArray<ServiceParam> Parameters,
  GreenToken CloseParen,
  TypeExpr ReturnType,
  GreenToken BodyOpen)
  : GreenNode(SyntaxKind.FunctionDeclaration, Name.Offset)
{
  public override int FullWidth => BodyOpen.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return OpenParen;
    foreach (ServiceParam p in Parameters) yield return p;
    yield return CloseParen;
    foreach (GreenNode child in ReturnType.GetChildren())
      yield return child;
    yield return BodyOpen;
  }
}

public sealed record ServiceDtorHeader(
  GreenToken Tilde,
  GreenToken OpenParen,
  GreenToken CloseParen,
  TypeExpr ReturnType,
  GreenToken BodyOpen)
  : GreenNode(SyntaxKind.FunctionDeclaration, Tilde.Offset)
{
  public override int FullWidth => BodyOpen.EndOffset - Tilde.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Tilde;
    yield return OpenParen;
    yield return CloseParen;
    foreach (GreenNode child in ReturnType.GetChildren())
      yield return child;
    yield return BodyOpen;
  }
}
