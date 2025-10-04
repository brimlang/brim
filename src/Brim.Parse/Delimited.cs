using Brim.Parse.Collections;
using Brim.Parse.Green;

namespace Brim.Parse;

internal static class Delimited
{
  public static StructuralArray<TElement> ParseCommaSeparatedTypes<TElement>(
    Parser p,
    Func<Parser, GreenNode> parseType,
    Func<GreenNode, GreenToken?, TElement> makeElement,
    params RawKind[] closeKinds)
  {
    if (IsAtClose(p, closeKinds) || p.MatchRaw(RawKind.Eob))
      return [];

    ImmutableArray<TElement>.Builder builder = ImmutableArray.CreateBuilder<TElement>();
    while (true)
    {
      int before = p.Current.CoreToken.Offset;
      GreenNode node = parseType(p);
      GreenToken? trailing = null;
      if (p.MatchRaw(RawKind.Comma))
        trailing = p.ExpectSyntax(SyntaxKind.CommaToken);

      builder.Add(makeElement(node, trailing));
      if (trailing is null)
        break;

      if (IsAtClose(p, closeKinds) || p.MatchRaw(RawKind.Eob))
        break;

      if (p.Current.CoreToken.Offset == before)
        break;
    }

    return [.. builder];
  }

  // Generic helper for CommaList<T>. Elements are produced by parseElement and
  // may optionally consume and retain a trailing comma internally. Parsing
  // stops when no trailing comma was found, a close token, EOB, or lack of
  // forward progress is detected.
  public static StructuralArray<TElement> ParseCommaList<TElement>(
    Parser p,
    Func<Parser, (TElement element, bool hasTrailingComma)> parseElement,
    params RawKind[] closeKinds)
  {
    // CommaList requires at least one element; caller should ensure not at close.
    ImmutableArray<TElement>.Builder builder = ImmutableArray.CreateBuilder<TElement>();
    while (true)
    {
      int before = p.Current.CoreToken.Offset;
      (TElement element, bool hasTrailing) = parseElement(p);
      builder.Add(element);
      if (!hasTrailing) break;
      if (IsAtClose(p, closeKinds) || p.MatchRaw(RawKind.Eob)) break; // allow trailing comma
      if (p.Current.CoreToken.Offset == before) break; // safety stall guard
    }
    return [.. builder];
  }

  // Optional form CommaListOpt<T>. Returns empty when immediately at a close or EOB.
  public static StructuralArray<TElement> ParseCommaListOpt<TElement>(
    Parser p,
    Func<Parser, (TElement element, bool hasTrailingComma)> parseElement,
    params RawKind[] closeKinds)
  {
    if (IsAtClose(p, closeKinds) || p.MatchRaw(RawKind.Eob)) return [];
    return ParseCommaList(p, parseElement, closeKinds);
  }

  static bool IsAtClose(Parser p, ReadOnlySpan<RawKind> closeKinds)
  {
    foreach (RawKind k in closeKinds)
      if (p.MatchRaw(k)) return true;

    return false;
  }
}
