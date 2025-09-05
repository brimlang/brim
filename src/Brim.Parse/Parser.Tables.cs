using Brim.Parse.Green;

namespace Brim.Parse;

delegate GreenNode ParseAction(ref Parser parser);

/// <summary>
/// Ordered prediction entry. The <see cref="LookAhead"/> is matched against the token stream; first matching entry wins.
/// Linear scan is intentional: grammar surface is small and this keeps things transparent.
/// </summary>
readonly record struct PredictEntry(LookAhead LookAhead, ParseAction Action);

readonly record struct LookAhead(
  RawTokenKind K1,
  RawTokenKind K2 = RawTokenKind.Any,
  RawTokenKind K3 = RawTokenKind.Any,
  RawTokenKind K4 = RawTokenKind.Any)
{
  public static implicit operator LookAhead(RawTokenKind one) => new(one);
  public static implicit operator LookAhead((RawTokenKind one, RawTokenKind two) t) => new(t.one, t.two);
  public static implicit operator LookAhead((RawTokenKind one, RawTokenKind two, RawTokenKind three) t) => new(t.one, t.two, t.three);
  public static implicit operator LookAhead((RawTokenKind one, RawTokenKind two, RawTokenKind three, RawTokenKind four) t) => new(t.one, t.two, t.three, t.four);
}

public partial struct Parser
{
  static readonly Dictionary<SyntaxKind, RawTokenKind> _tokenMap = new()
  {
    { SyntaxKind.WhiteSpaceToken, RawTokenKind.WhitespaceTrivia },
    { SyntaxKind.TerminatorToken, RawTokenKind.Terminator },
    { SyntaxKind.ExportMarkerToken, RawTokenKind.LessLess },
    { SyntaxKind.ModulePathOpenToken, RawTokenKind.LBracketLBracket},
    { SyntaxKind.ModulePathCloseToken, RawTokenKind.RBracketRBracket},
    { SyntaxKind.ModulePathSepToken, RawTokenKind.ColonColon},
    { SyntaxKind.IdentifierToken, RawTokenKind.Identifier },
    { SyntaxKind.NumberToken, RawTokenKind.NumberLiteral },
    { SyntaxKind.StrToken, RawTokenKind.StringLiteral },
    { SyntaxKind.EqualToken, RawTokenKind.Equal },
    { SyntaxKind.CloseBraceToken, RawTokenKind.RBrace },
    { SyntaxKind.CommentToken, RawTokenKind.CommentTrivia },
    { SyntaxKind.EofToken, RawTokenKind.Eof },
    { SyntaxKind.ColonToken, RawTokenKind.Colon },
    { SyntaxKind.StructToken, RawTokenKind.PercentLBrace },
    { SyntaxKind.ErrorToken, RawTokenKind.Error },
  };

  /// <summary>
  /// Ordered module prediction rules. Keep list short; duplicates guarded in DEBUG.
  /// </summary>
  static readonly PredictEntry[] _moduleTable =
  [
    new(RawTokenKind.LessLess, ExportDirective.Parse),
    new((RawTokenKind.LBracketLBracket, RawTokenKind.Identifier), ModuleHeader.Parse),
    new((RawTokenKind.Identifier, RawTokenKind.Equal, RawTokenKind.LBracketLBracket), ImportDeclaration.Parse),
    new((RawTokenKind.Identifier, RawTokenKind.Equal, RawTokenKind.PercentLBrace), StructDeclaration.Parse),
  ];

  static Parser()
  {
#if DEBUG
    HashSet<LookAhead> seen = [];
    foreach (PredictEntry e in _moduleTable)
    {
      if (!seen.Add(e.LookAhead))
      {
        throw new InvalidOperationException($"Duplicate LookAhead in prediction table: {e.LookAhead}");
      }
    }
#endif
  }

  internal static ParseAction MakeTrace(string name, ParseAction action) => (ref p) =>
  {
    Console.WriteLine($"Entering {name} at token {p.Current} (offset {p.Current.Offset})");
    GreenNode result = action(ref p);
    Console.WriteLine($"Exiting {name} at token {p.Current} (offset {p.Current.Offset})");
    return result;
  };
}
