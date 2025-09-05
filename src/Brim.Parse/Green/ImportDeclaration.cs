using System.Collections.Immutable;

namespace Brim.Parse.Green;

public sealed record ImportDeclaration(
  Identifier Identifier,
  GreenToken Equal,
  ModuleHeader ModuleHeader,
  GreenToken Terminator)
: GreenNode(SyntaxKind.ImportDeclaration, Identifier.Offset)
, IParsable<ImportDeclaration>
{
  public override int FullWidth => ModuleHeader.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Equal;
    yield return ModuleHeader;
    yield return Terminator;
  }

  public static ImportDeclaration Parse(ref Parser p) => new(
    Identifier.Parse(ref p),
    p.ExpectSyntax(SyntaxKind.EqualToken),
    ModuleHeader.Parse(ref p),
    p.ExpectSyntax(SyntaxKind.TerminatorToken)
  );
}

public sealed record FieldDeclaration(
  Identifier Identifier,
  GreenToken Colon,
  Identifier TypeAnnotation) :
GreenNode(SyntaxKind.FieldDeclaration, Identifier.Offset),
IParsable<FieldDeclaration>
{
  public override int FullWidth => TypeAnnotation.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Colon;
    yield return TypeAnnotation;
  }

  public static FieldDeclaration Parse(ref Parser p) => new(
    Identifier.Parse(ref p),
    p.ExpectSyntax(SyntaxKind.ColonToken),
    Identifier.Parse(ref p)
  );
}

public sealed record StructDeclaration(
  Identifier Identifier,
  GreenToken Equal,
  GreenToken StructOpen,
  StructuralArray<FieldDeclaration> Fields,
  GreenToken Close,
  GreenToken Terminator) :
GreenNode(SyntaxKind.StructDeclaration, Identifier.Offset),
IParsable<StructDeclaration>
{
  public override int FullWidth => Terminator.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Equal;
    yield return StructOpen;
    foreach (FieldDeclaration field in Fields) yield return field;
    yield return Close;
    yield return Terminator;
  }

  public static StructDeclaration Parse(ref Parser p)
  {
    Identifier id = Identifier.Parse(ref p);
    GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.StructToken);

    ImmutableArray<FieldDeclaration>.Builder fields = ImmutableArray.CreateBuilder<FieldDeclaration>();
    while (!p.Match(RawTokenKind.RBrace) && !p.Match(RawTokenKind.Eof))
    {
      fields.Add(FieldDeclaration.Parse(ref p));
      if (p.Match(RawTokenKind.Comma))
        _ = p.Expect(RawTokenKind.Comma);
    }
    StructuralArray<FieldDeclaration> fieldArray = [.. fields];

    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    return new(
        id,
        eq,
        open,
        fieldArray,
        close,
        term);
  }
}
