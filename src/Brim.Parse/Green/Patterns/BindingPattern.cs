namespace Brim.Parse.Green;

/// <summary>
/// Represents a binding pattern (identifier) that matches any value and binds it to a name.
/// </summary>
public sealed record BindingPattern(GreenToken Identifier)
  : PatternNode(SyntaxKind.BindingPattern, Identifier.Offset)
{
    public override int FullWidth => Identifier.FullWidth;

    public override IEnumerable<GreenNode> GetChildren()
    {
        yield return Identifier;
    }

    internal static new BindingPattern Parse(Parser parser)
    {
        GreenToken identifier = parser.Expect(RawKind.Identifier, SyntaxKind.IdentifierToken);
        return new BindingPattern(identifier);
    }

    internal static PatternNode ParseError(Parser parser)
    {
        GreenToken error = parser.Expect(RawKind.Error, SyntaxKind.ErrorToken);
        return new BindingPattern(error);
    }
}
