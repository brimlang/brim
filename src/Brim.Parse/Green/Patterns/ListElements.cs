using Brim.Parse.Collections;

namespace Brim.Parse.Green;

/// <summary>
/// Represents the elements within a list pattern: Pattern [',' Pattern]* [',' RestPattern]? | RestPattern
/// </summary>
public sealed record ListElements(
  StructuralArray<GreenNode> Elements)
  : GreenNode(SyntaxKind.ListElements, Elements.Count > 0 ? Elements[0].Offset : 0)
{
    public override int FullWidth => Elements.Count > 0 ? Elements[^1].EndOffset - Offset : 0;

    public override IEnumerable<GreenNode> GetChildren()
    {
        foreach (GreenNode element in Elements)
        {
            yield return element;
        }
    }

    internal static ListElements Parse(Parser parser)
    {
        ArrayBuilder<GreenNode> elements = [];

        // First element - could be a pattern or rest pattern
        if (parser.Match(RawKind.StopStop))
        {
            // Rest pattern only: (..rest)
            elements.Add(RestPattern.Parse(parser));
            return new ListElements(elements);
        }

        // Parse first pattern
        elements.Add(PatternNode.Parse(parser));

        // Parse additional elements
        while (parser.Match(RawKind.Comma))
        {
            GreenToken comma = parser.Expect(RawKind.Comma, SyntaxKind.CommaToken);
            elements.Add(comma);

            // Check if next is rest pattern
            if (parser.Match(RawKind.StopStop))
            {
                elements.Add(RestPattern.Parse(parser));
                break; // Rest pattern must be last
            }
            else if (!parser.Match(RawKind.RParen))
            {
                // Regular pattern
                elements.Add(PatternNode.Parse(parser));
            }
            else
            {
                // Trailing comma before close paren
                break;
            }
        }

        return new ListElements(elements.ToArray());
    }
}
