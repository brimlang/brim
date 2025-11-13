using Brim.Parse.Green;

namespace Brim.Parse;

public sealed partial class Parser
{
  internal static GreenNode ParseIdentifierHead(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);

    if (p.Match(TokenKind.ColonEqual))
      return TypeDeclaration.ParseAfterName(p, name);

    if (p.Match(TokenKind.Colon))
    {
      // Lookahead to distinguish value decl from function decl
      // After name :, we need to peek into the parameter list
      if (IsFunctionDeclaration(p))
        return FunctionDeclaration.ParseAfterName(p, name);
      else
        return ValueDeclaration.ParseAfterName(p, mutator: null, name);
    }

    // Fall back: treat as unsupported module member
    CoreToken token = name.Identifier.CoreToken;
    p.AddDiagUnsupportedModuleMember(token);
    return SyntaxKind.ErrorToken.MakeGreen(token);
  }

  static bool IsFunctionDeclaration(Parser p)
  {
    // Look ahead after the colon to distinguish:
    // Value decl: name :(Type, ...) Ret = expr
    // Function decl: name :(param :Type, ...) Ret { body }

    if (!p.Match(TokenKind.LParen, 1))
      return false; // Not a function type at all

    // Empty parens: :()
    // Could be either form - need to see what's after the closing paren
    if (p.Match(TokenKind.RParen, 2))
    {
      // After :() we need to skip return type to find = or {
      // This is tricky - for now, be conservative and assume value decl
      // unless we can definitively see it's a function decl
      // Actually, empty params with = is value decl, anything else check further
      // But we can't look too far ahead. Default to value decl for :()
      return false; // Assume value declaration for empty params
    }

    // If first token in parens is identifier followed by colon, it's named param
    // Position: 0=current, 1=(, 2=first_in_parens, 3=after_first
    if (p.Match(TokenKind.Identifier, 2) && p.Match(TokenKind.Colon, 3))
      return true; // Function declaration (named parameter)

    // Otherwise it's a value declaration (type in parens, no param names)
    return false;
  }
}
