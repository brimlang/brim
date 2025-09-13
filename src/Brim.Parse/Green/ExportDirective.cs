namespace Brim.Parse.Green;

public sealed record ExportDirective(
  GreenToken ExportMarker,
  GreenToken Identifier,
  GreenToken Terminator) :
GreenNode(SyntaxKind.ExportDirective, ExportMarker.Offset),
IParsable<ExportDirective>
{
  public override int FullWidth => Identifier.EndOffset - ExportMarker.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ExportMarker;
    yield return Identifier;
    yield return Terminator;
  }

  public static ExportDirective Parse(Parser p) =>
    new(
      p.ExpectSyntax(SyntaxKind.ExportMarkerToken),
      p.ExpectSyntax(SyntaxKind.IdentifierToken),
      p.ExpectSyntax(SyntaxKind.TerminatorToken)
    );
}

