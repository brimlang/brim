namespace Brim.Parse.Green;

public sealed record ServiceParam(
  GreenToken Name,
  GreenToken Colon,
  GreenNode Type)
  : GreenNode(SyntaxKind.FieldDeclaration, Name.Offset)
{
  public override int FullWidth => Type.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren() { yield return Name; yield return Colon; yield return Type; }
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
  GreenNode ReturnType,
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
    yield return ReturnType;
    yield return BodyOpen;
  }
}

public sealed record ServiceDtorHeader(
  GreenToken OpenParen,
  GreenToken CloseParen,
  GreenToken BodyOpen)
  : GreenNode(SyntaxKind.FunctionDeclaration, OpenParen.Offset)
{
  public override int FullWidth => BodyOpen.EndOffset - OpenParen.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenParen; yield return CloseParen; yield return BodyOpen;
  }
}
