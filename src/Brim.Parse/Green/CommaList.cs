using Brim.Parse.Collections;

namespace Brim.Parse.Green;

/// <summary>
/// Provides factory methods for parsing comma-separated lists.
/// </summary>
public static class CommaList
{
  /// <summary>
  /// Parses a comma-separated list of tokens of the given kind, enclosed by the given open and close kinds.
  /// Backwards-compatible overload that uses SyntaxKind and maps to RawKind internally.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openKind">The syntax kind of the opening delimiter.</param>
  /// <param name="closeKind">The syntax kind of the closing delimiter.</param>
  /// <param name="elementKind">The syntax kind of list elements.</param>
  /// <returns>A comma list containing tokens of the specified kind.</returns>
  public static CommaList<GreenToken> Parse(
    Parser p,
    SyntaxKind openKind,
    SyntaxKind closeKind,
    SyntaxKind elementKind)
  {
    RawKind openRaw = Parser.MapRawKind(openKind);
    RawKind closeRaw = Parser.MapRawKind(closeKind);
    return Parse(p, openRaw, openKind, closeRaw, closeKind, elementKind);
  }

  /// <summary>
  /// Parses a comma-separated list of tokens of the given kind, enclosed by the given open and close kinds.
  /// Direct overload that uses RawKind for matching and SyntaxKind for token assignment.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openRaw">The raw kind of the opening delimiter.</param>
  /// <param name="openSyntax">The syntax kind to assign to the opening delimiter token.</param>
  /// <param name="closeRaw">The raw kind of the closing delimiter.</param>
  /// <param name="closeSyntax">The syntax kind to assign to the closing delimiter token.</param>
  /// <param name="elementKind">The syntax kind of list elements.</param>
  /// <returns>A comma list containing tokens of the specified kind.</returns>
  public static CommaList<GreenToken> Parse(
    Parser p,
    RawKind openRaw,
    SyntaxKind openSyntax,
    RawKind closeRaw,
    SyntaxKind closeSyntax,
    SyntaxKind elementKind) =>
    CommaList<GreenToken>.Parse(
      p,
      openRaw,
      openSyntax,
      closeRaw,
      closeSyntax,
      p2 => p2.ExpectSyntax(elementKind));
}

/// <summary>
/// Represents a comma-separated list in the green syntax tree.
/// Supports optional leading/trailing terminators and trailing commas.
/// </summary>
/// <typeparam name="T">The type of elements in the list, must be a <see cref="GreenNode"/>.</typeparam>
/// <param name="OpenToken">The opening delimiter token.</param>
/// <param name="LeadingTerminator">Optional terminator after the opening delimiter.</param>
/// <param name="Elements">The collection of list elements with their separators.</param>
/// <param name="TrailingComma">Optional comma after the last element.</param>
/// <param name="TrailingTerminator">Optional terminator before the closing delimiter.</param>
/// <param name="CloseToken">The closing delimiter token.</param>
public sealed record CommaList<T>(
  GreenToken OpenToken,
  GreenToken? LeadingTerminator,
  StructuralArray<CommaList<T>.Element> Elements,
  GreenToken? TrailingComma,
  GreenToken? TrailingTerminator,
  GreenToken CloseToken) :
GreenNode(SyntaxKind.CommaList, OpenToken.Offset) where T : GreenNode
{
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

  /// <summary>
  /// Parses a comma-separated list using a custom element parser.
  /// Backwards-compatible overload that uses SyntaxKind and maps to RawKind internally.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openKind">The syntax kind of the opening delimiter.</param>
  /// <param name="closeKind">The syntax kind of the closing delimiter.</param>
  /// <param name="parseElement">Function to parse individual list elements.</param>
  /// <returns>A parsed comma list.</returns>
  public static CommaList<T> Parse(Parser p, SyntaxKind openKind, SyntaxKind closeKind, Func<Parser, T> parseElement)
  {
    RawKind openRaw = Parser.MapRawKind(openKind);
    RawKind closeRaw = Parser.MapRawKind(closeKind);
    return Parse(p, openRaw, openKind, closeRaw, closeKind, parseElement);
  }

  /// <summary>
  /// Parses a comma-separated list using a custom element parser.
  /// Direct overload that uses RawKind for matching and SyntaxKind for token assignment.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openRaw">The raw kind of the opening delimiter.</param>
  /// <param name="openSyntax">The syntax kind to assign to the opening delimiter token.</param>
  /// <param name="closeRaw">The raw kind of the closing delimiter.</param>
  /// <param name="closeSyntax">The syntax kind to assign to the closing delimiter token.</param>
  /// <param name="parseElement">Function to parse individual list elements.</param>
  /// <returns>A parsed comma list.</returns>
  public static CommaList<T> Parse(Parser p, RawKind openRaw, SyntaxKind openSyntax, RawKind closeRaw, SyntaxKind closeSyntax, Func<Parser, T> parseElement)
  {
    GreenToken open = p.Expect(openRaw, openSyntax);

    GreenToken? leadingTerminator = null;
    if (p.MatchRaw(RawKind.Terminator))
      leadingTerminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    ArrayBuilder<Element> elements = [];

    // Parse first element if not immediately at close
    if (!p.MatchRaw(closeRaw))
    {
      elements.Add(Element.Parse(p, parseElement));
      // Parse remaining elements
      while (true)
      {
        if (!p.MatchRaw(RawKind.Comma))
          break;

        if (p.MatchRaw(closeRaw, 1) || (p.MatchRaw(RawKind.Terminator, 1) && p.MatchRaw(closeRaw, 2)))
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

    GreenToken close = p.Expect(closeRaw, closeSyntax);

    return new(
      open,
      leadingTerminator,
      elements,
      trailingComma,
      trailingTerminator,
      close
    );
  }

  /// <summary>
  /// Represents a single element in a comma list, including its leading separators.
  /// </summary>
  /// <param name="LeadingComma">Optional comma preceding this element.</param>
  /// <param name="LeadingTerminator">Optional terminator preceding this element.</param>
  /// <param name="Node">The element's content node.</param>
  public sealed record Element(
    GreenToken? LeadingComma,
    GreenToken? LeadingTerminator,
    T Node) :
  GreenNode(SyntaxKind.ListElement, Node.Offset)
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
