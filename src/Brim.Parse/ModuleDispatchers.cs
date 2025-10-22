using Brim.Parse.Green;

namespace Brim.Parse;

public sealed partial class Parser
{
  internal static GreenNode ParseIdentifierHead(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);

    if (p.MatchRaw(RawKind.ColonEqual))
      return TypeDeclaration.ParseAfterName(p, name);

    if (p.MatchRaw(RawKind.Colon))
      return ValueDeclaration.ParseAfterName(p, mutator: null, name);

    // Fall back: treat as unsupported module member
    RawToken token = name.Identifier.Token;
    p.AddDiagUnsupportedModuleMember(token);
    return new GreenToken(SyntaxKind.ErrorToken, token);
  }
}
