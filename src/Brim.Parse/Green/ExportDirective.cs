namespace Brim.Parse.Green;

public sealed record ExportDirective(
  GreenToken ExportMarker,
  GreenToken WhiteSpace,
  Identifier Identifier,
  GreenToken Terminator)
: GreenNode(SyntaxKind.ExportDeclaration, ExportMarker.Offset)
, IParsable<ExportDirective>
{
  public override int FullWidth => Identifier.EndOffset - ExportMarker.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ExportMarker;
    yield return WhiteSpace;
    yield return Identifier;
    yield return Terminator;
  }

  public static ExportDirective Parse(ref Parser p) =>
    new(
      p.ExpectSyntax(SyntaxKind.ExportMarkerToken),
      p.ExpectSyntax(SyntaxKind.WhiteSpaceToken),
      Identifier.Parse(ref p),
      p.ExpectSyntax(SyntaxKind.TerminatorToken)
    );
}

