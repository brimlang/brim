using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public static class CommaList
{
  public static CommaList<GreenToken> Parse(Parser p, SyntaxKind openKind, SyntaxKind closeKind, SyntaxKind elementKind) =>
    CommaList<GreenToken>.Parse(p, openKind, closeKind, p2 => p2.ExpectSyntax(elementKind));
}

public sealed record CommaList<T>(
  GreenToken openToken,
  GreenToken? leadingTerminator,
  StructuralArray<CommaList<T>.Element> elements,
  GreenToken? trailingComma,
  GreenToken? trailingTerminator,
  GreenToken closeToken
) : GreenNode(SyntaxKind.CommaList, openToken.Offset) where T : GreenNode
{
  public GreenToken OpenToken { get; } = openToken;
  public GreenToken? LeadingTerminator { get; } = leadingTerminator;
  public StructuralArray<Element> Elements { get; } = elements;
  public GreenToken? TrailingComma { get; } = trailingComma;
  public GreenToken? TrailingTerminator { get; } = trailingTerminator;
  public GreenToken CloseToken { get; } = closeToken;

  public override int FullWidth => CloseToken.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenToken;
    if (LeadingTerminator is not null) yield return LeadingTerminator;
    foreach (Element element in Elements) yield return element;
    if (TrailingComma is not null) yield return TrailingComma;
    if (TrailingTerminator is not null) yield return TrailingTerminator;
    yield return CloseToken;
  }

  public static CommaList<T> Parse(Parser p, SyntaxKind openKind, SyntaxKind closeKind, Func<Parser, T> parseElement)
  {
    GreenToken open = p.ExpectSyntax(openKind);

    GreenToken? leadingTerminator = null;
    if (p.MatchRaw(RawKind.Terminator))
      leadingTerminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    ImmutableArray<Element>.Builder elements = ImmutableArray.CreateBuilder<Element>();

    // Parse first element if not immediately at close
    if (!p.MatchSyntax(closeKind))
    {
      elements.Add(Element.Parse(p, parseElement));
      // Parse remaining elements
      while (true)
      {

        if (!p.MatchRaw(RawKind.Comma))
          break;

        if (p.MatchSyntax(closeKind, 1) || (p.MatchRaw(RawKind.Terminator, 1) && p.MatchSyntax(closeKind, 2)))
          break;

        Parser.StallGuard sg = p.GetStallGuard();
        elements.Add(Element.Parse(p, parseElement));
        if (sg.Stalled) break;
      }
    }

    GreenToken? trailingComma = null;
    if (p.MatchRaw(RawKind.Comma))
      trailingComma = p.ExpectSyntax(SyntaxKind.CommaToken);

    GreenToken? trailingTerminator = null;
    if (p.MatchRaw(RawKind.Terminator))
      trailingTerminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    GreenToken close = p.ExpectSyntax(closeKind);
    return new(
      open,
      leadingTerminator,
      elements,
      trailingComma,
      trailingTerminator,
      close
    );
  }

  public sealed record Element(
    GreenToken? LeadingComma,
    GreenToken? LeadingTerminator,
    T Node
  ) : GreenNode(SyntaxKind.ListElement, Node.Offset)
  {
    public override int FullWidth => (LeadingComma?.EndOffset ?? LeadingTerminator?.EndOffset ?? Node.EndOffset) - Offset;
    public override IEnumerable<GreenNode> GetChildren()
    {
      if (LeadingComma is not null) yield return LeadingComma;
      if (LeadingTerminator is not null) yield return LeadingTerminator;
      yield return Node;
    }

    internal static Element Parse(Parser p, Func<Parser, T> parseElement)
    {
      GreenToken? comma = null;
      if (p.MatchRaw(RawKind.Comma))
        comma = p.ExpectSyntax(SyntaxKind.CommaToken);

      GreenToken? terminator = null;
      if (p.MatchRaw(RawKind.Terminator))
        terminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);

      T element = parseElement(p);
      return new(comma, terminator, element);
    }
  }
}
