namespace Brim.Parse.Green;

/// <summary>
/// Provides factory methods for parsing comma-separated lists.
/// </summary>
public static class CommaList
{
  /// <summary>
  /// Parses a comma-separated list of tokens of the given kind, enclosed by the given open and close kinds.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openSyntax">The syntax kind to assign to the opening delimiter token.</param>
  /// <param name="closeSyntax">The syntax kind to assign to the closing delimiter token.</param>
  /// <param name="elementKind">The syntax kind of list elements.</param>
  /// <returns>A comma list containing tokens of the specified kind.</returns>
  public static CommaList<GreenToken> Parse(
    Parser p,
    SyntaxKind openSyntax,
    SyntaxKind closeSyntax,
    SyntaxKind elementKind) =>
    CommaList<GreenToken>.Parse(
      p,
      openSyntax,
      closeSyntax,
      p2 => p2.Expect(elementKind));
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
  /// Direct overload that uses RawKind for matching and SyntaxKind for token assignment.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openRaw">The raw kind of the opening delimiter.</param>
  /// <param name="openSyntax">The syntax kind to assign to the opening delimiter token.</param>
  /// <param name="closeRaw">The raw kind of the closing delimiter.</param>
  /// <param name="closeSyntax">The syntax kind to assign to the closing delimiter token.</param>
  /// <param name="parseElement">Function to parse individual list elements.</param>
  /// <returns>A parsed comma list.</returns>
  public static CommaList<T> Parse(Parser p, SyntaxKind openSyntax, SyntaxKind closeSyntax, Func<Parser, T> parseElement)
  {
    GreenToken open = p.Expect(openSyntax);

    GreenToken? leadingTerminator = null;
    if (p.Match(TokenKind.Terminator))
      leadingTerminator = p.Expect(SyntaxKind.TerminatorToken);

    ArrayBuilder<Element> elements = [];

    // Parse first element if not immediately at close
    if (!p.Match(Parser.MapTokenKind(closeSyntax)))
    {
      elements.Add(Element.Parse(p, parseElement));
      // Parse remaining elements
      while (true)
      {
        if (!p.Match(TokenKind.Comma))
          break;

        if (p.Match(Parser.MapTokenKind(closeSyntax), 1) || (p.Match(TokenKind.Terminator, 1) && p.Match(Parser.MapTokenKind(closeSyntax), 2)))
          break;

        Parser.StallGuard sg = p.GetStallGuard();
        elements.Add(Element.Parse(p, parseElement));
        if (sg.Stalled) break;
      }
    }

    GreenToken? trailingComma = null;
    if (p.Match(TokenKind.Comma))
      trailingComma = p.Expect(SyntaxKind.CommaToken);

    GreenToken? trailingTerminator = null;
    if (p.Match(TokenKind.Terminator))
      trailingTerminator = p.Expect(SyntaxKind.TerminatorToken);

    GreenToken close = p.Expect(closeSyntax);

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
      if (p.Match(TokenKind.Comma))
        comma = p.Expect(SyntaxKind.CommaToken);

      GreenToken? terminator = null;
      if (p.Match(TokenKind.Terminator))
        terminator = p.Expect(SyntaxKind.TerminatorToken);

      T element = parseElement(p);
      return new(comma, terminator, element);
    }
  }
}
