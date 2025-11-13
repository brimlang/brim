namespace Brim.Parse.Green;

/// <summary>
/// Represents a protocol binding entry in a service pattern: IDENT ':' TypeRef
/// Used in patterns like @(auth :Auth, log :Logger)
/// </summary>
public sealed record ServicePatternEntry(
  GreenToken Name,
  GreenToken ColonToken,
  TypeRef Protocol)
  : GreenNode(SyntaxKind.ServicePatternEntry, Name.Offset)
{
  public override int FullWidth => Protocol.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return ColonToken;
    yield return Protocol;
  }

  internal static ServicePatternEntry Parse(Parser parser)
  {
    GreenToken name = parser.Expect(SyntaxKind.IdentifierToken);
    GreenToken colon = parser.Expect(SyntaxKind.ColonToken);
    TypeRef protocol = TypeRef.Parse(parser);

    return new ServicePatternEntry(name, colon, protocol);
  }
}
