using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record CommaList(
  GreenToken openToken,
  GreenToken? leadingTerminator,
  StructuralArray<CommaList.Element> elements,
  GreenToken? trailingComma,
  GreenToken? trailingTerminator,
  GreenToken closeToken
)
{
  public GreenToken OpenToken { get; } = openToken;
  public GreenToken? LeadingTerminator { get; } = leadingTerminator;
  public StructuralArray<Element> Elements { get; } = elements;
  public GreenToken? TrailingComma { get; } = trailingComma;
  public GreenToken? TrailingTerminator { get; } = trailingTerminator;
  public GreenToken CloseToken { get; } = closeToken;

  public static CommaList Parse(Parser p, SyntaxKind openKind, SyntaxKind closeKind, SyntaxKind elementKind)
  {
    GreenToken open = p.ExpectSyntax(openKind);

    GreenToken? leadingTerminator = null;
    if (p.MatchRaw(RawKind.Terminator))
      leadingTerminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    ImmutableArray<Element>.Builder elements = ImmutableArray.CreateBuilder<Element>();

    // Parse first element if not immediately at close
    if (!p.MatchSyntax(closeKind))
    {
      elements.Add(Element.Parse(p, elementKind));
      // Parse remaining elements
      while (true)
      {
        if (!p.MatchRaw(RawKind.Comma))
          break;

        if (TrailingCommaAhead())
          break;

        elements.Add(Element.Parse(p, elementKind));
      }
    }

    GreenToken? trailingComma = null;
    if (p.MatchRaw(RawKind.Comma))
      trailingComma = p.ExpectSyntax(SyntaxKind.CommaToken);

    GreenToken? trailingTerminator = null;
    if (p.MatchRaw(RawKind.Terminator))
      trailingTerminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    GreenToken close = p.ExpectSyntax(closeKind);
    return new CommaList(
      open,
      leadingTerminator,
      elements,
      trailingComma,
      trailingTerminator,
      close
    );

    // local functions

    // Helper to detect a trailing-comma scenario: close follows either
    // immediately or after a single Terminator. Placed at end of parent
    // function per style preference (local functions may be referenced
    // before their declaration).
    bool TrailingCommaAhead() => p.MatchSyntax(closeKind, 1)
      ? true
      : p.MatchRaw(RawKind.Terminator, 1) && p.MatchSyntax(closeKind, 2);
  }

  public sealed record Element(
    GreenToken? LeadingComma,
    GreenToken? LeadingTerminator,
    GreenNode Node
  ) : GreenNode(SyntaxKind.ListElement, Node.Offset)
  {
    public override int FullWidth => (LeadingComma?.EndOffset ?? LeadingTerminator?.EndOffset ?? Node.EndOffset) - Offset;
    public override IEnumerable<GreenNode> GetChildren()
    {
      if (LeadingComma is not null) yield return LeadingComma;
      if (LeadingTerminator is not null) yield return LeadingTerminator;
      yield return Node;
    }

    public static Element Parse(Parser p, SyntaxKind elementKind)
    {
      GreenToken? comma = null;
      if (p.MatchRaw(RawKind.Comma))
        comma = p.ExpectSyntax(SyntaxKind.CommaToken);

      GreenToken? terminator = null;
      if (p.MatchRaw(RawKind.Terminator))
        terminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);

      GreenNode element = p.ExpectSyntax(elementKind);
      return new(comma, terminator, element);
    }
  }
}
