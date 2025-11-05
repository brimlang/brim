using Brim.Parse.Collections;

namespace Brim.Parse.Green;

/// <summary>
/// Provides factory methods for parsing terminator-separated lists.
/// </summary>
public static class TerminatorList
{
  /// <summary>
  /// Parses a terminator-separated list of tokens of the given kind, enclosed by the given open and close kinds.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openKind">The syntax kind of the opening delimiter.</param>
  /// <param name="closeKind">The syntax kind of the closing delimiter.</param>
  /// <param name="elementKind">The syntax kind of list elements.</param>
  /// <returns>A comma list containing tokens of the specified kind.</returns>
  public static TerminatorList<GreenToken> Parse(
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
  /// Parses a terminator-separated list of tokens of the given kind, enclosed by the given open and close kinds.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openRaw">The raw kind of the opening delimiter.</param>
  /// <param name="openSyntax">The syntax kind to assign to the opening delimiter token.</param>
  /// <param name="closeRaw">The raw kind of the closing delimiter.</param>
  /// <param name="closeSyntax">The syntax kind to assign to the closing delimiter token.</param>
  /// <param name="elementKind">The syntax kind of list elements.</param>
  /// <returns>A comma list containing tokens of the specified kind.</returns>
  public static TerminatorList<GreenToken> Parse(
    Parser p,
    RawKind openRaw,
    SyntaxKind openSyntax,
    RawKind closeRaw,
    SyntaxKind closeSyntax,
    SyntaxKind elementKind) =>
    TerminatorList<GreenToken>.Parse(
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
public sealed record TerminatorList<T>(
  GreenToken OpenToken,
  StructuralArray<GreenToken> LeadingTerminatorList,
  StructuralArray<TerminatorList<T>.Element> Elements,
  GreenToken CloseToken) :
GreenNode(SyntaxKind.TerminatorList, OpenToken.Offset) where T : GreenNode
{
  public override int FullWidth => CloseToken.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenToken;
    foreach (GreenToken leading in LeadingTerminatorList) yield return leading;
    foreach (Element element in Elements) yield return element;
    yield return CloseToken;
  }

  /// <summary>
  /// Parses a terminator-separated list using a custom element parser.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openKind">The syntax kind of the opening delimiter.</param>
  /// <param name="closeKind">The syntax kind of the closing delimiter.</param>
  /// <param name="parseElement">Function to parse individual list elements.</param>
  /// <returns>A parsed terminator list.</returns>
  public static TerminatorList<T> Parse(Parser p, SyntaxKind openKind, SyntaxKind closeKind, Func<Parser, T> parseElement)
  {
    RawKind openRaw = Parser.MapRawKind(openKind);
    RawKind closeRaw = Parser.MapRawKind(closeKind);
    return Parse(p, openRaw, openKind, closeRaw, closeKind, parseElement);
  }

  /// <summary>
  /// Parses a terminator-separated list using a custom element parser.
  /// </summary>
  /// <param name="p">The parser instance.</param>
  /// <param name="openRaw">The raw kind of the opening delimiter.</param>
  /// <param name="openSyntax">The syntax kind to assign to the opening delimiter token.</param>
  /// <param name="closeRaw">The raw kind of the closing delimiter.</param>
  /// <param name="closeSyntax">The syntax kind to assign to the closing delimiter token.</param>
  /// <param name="parseElement">Function to parse individual list elements.</param>
  /// <returns>A parsed terminator list.</returns>
  public static TerminatorList<T> Parse(
    Parser p,
    RawKind openRaw,
    SyntaxKind openSyntax,
    RawKind closeRaw,
    SyntaxKind closeSyntax,
    Func<Parser, T> parseElement)
  {
    GreenToken open = p.Expect(openRaw, openSyntax);
    StructuralArray<GreenToken> leadingTerminators = p.CollectSyntaxKind(SyntaxKind.TerminatorToken);
    ArrayBuilder<Element> elements = [];

    // Parse first element if not immediately at close
    while (!p.MatchRawNotEob(closeRaw))
    {
      Parser.StallGuard sg = p.GetStallGuard();
      elements.Add(Element.Parse(p, parseElement));
      if (sg.Stalled) break;
    }

    GreenToken close = p.Expect(closeRaw, closeSyntax);

    return new(
      open,
      leadingTerminators,
      elements,
      close
    );
  }

  /// <summary>
  /// Represents a single element in a terminator list, including its trailing separators.
  /// </summary>
  /// <param name="Node">The element's content node.</param>
  /// <param name="Terminator">The terminator token following this element.</param>
  public sealed record Element(
    T Node,
    StructuralArray<GreenToken> TerminatorList) :
  GreenNode(SyntaxKind.ListElement, Node.Offset)
  {
    public override int FullWidth => (TerminatorList.Count > 0 ? TerminatorList[^1].EndOffset : Node.EndOffset) - Offset;
    public override IEnumerable<GreenNode> GetChildren()
    {
      yield return Node;
      foreach (GreenToken term in TerminatorList) yield return term;
    }

    internal static Element Parse(Parser p, Func<Parser, T> parseElement)
    {
      T element = parseElement(p);
      StructuralArray<GreenToken> terminators = p.CollectSyntaxKind(SyntaxKind.TerminatorToken);

      return new(element, terminators);
    }
  }
}
