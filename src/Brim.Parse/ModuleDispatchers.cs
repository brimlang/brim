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
    {
      // Lookahead to distinguish value decl from function decl
      // After name :, we need to peek into the parameter list
      if (IsFunctionDeclaration(p))
        return FunctionDeclaration.ParseAfterName(p, name);
      else
        return ValueDeclaration.ParseAfterName(p, mutator: null, name);
    }

    // Fall back: treat as unsupported module member
    RawToken token = name.Identifier.Token;
    p.AddDiagUnsupportedModuleMember(token);
    return new GreenToken(SyntaxKind.ErrorToken, token);
  }

  static bool IsFunctionDeclaration(Parser p)
  {
    // Look ahead after the colon to distinguish:
    // Value decl: name :(Type, ...) Ret = expr
    // Function decl: name :(param :Type, ...) Ret { body }
    
    if (!p.MatchRaw(RawKind.LParen, 1))
      return false; // Not a function type at all
    
    // Empty parens: :() 
    // Could be either form - need to see what's after the closing paren
    if (p.MatchRaw(RawKind.RParen, 2))
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
    if (p.MatchRaw(RawKind.Identifier, 2) && p.MatchRaw(RawKind.Colon, 3))
      return true; // Function declaration (named parameter)
    
    // Otherwise it's a value declaration (type in parens, no param names)
    return false;
  }
}
