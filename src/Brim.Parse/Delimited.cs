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

  static bool IsAtClose(Parser p, ReadOnlySpan<RawKind> closeKinds)
  {
    foreach (RawKind k in closeKinds)
      if (p.MatchRaw(k)) return true;

    return false;
  }
}
