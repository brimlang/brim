using Brim.Parse.Green;

namespace Brim.Parse;

public sealed partial class Parser
{
  // Member / declaration predictions.
  internal static readonly Prediction[] ModuleMemberPredictions =
  [
    new(ExportDirective.Parse, RawKind.LessLess),
    new(ImportDeclaration.Parse, (RawKind.Identifier, RawKind.Equal, RawKind.LBracketLBracket)),
    new(GenericDeclaration.Parse, (RawKind.Identifier, RawKind.LBracket, RawKind.Identifier, RawKind.Comma)),
    new(GenericDeclaration.Parse, (RawKind.Identifier, RawKind.LBracket, RawKind.Identifier, RawKind.Identifier)),
    new(GenericDeclaration.Parse, (RawKind.Identifier, RawKind.LBracket, RawKind.Identifier, RawKind.RBracket)),
    new(StructDeclaration.Parse, (RawKind.Identifier, RawKind.Equal, RawKind.PercentLBrace)),
    new(UnionDeclaration.Parse, (RawKind.Identifier, RawKind.Equal, RawKind.PipeLBrace)),
    new(FlagsDeclaration.Parse, (RawKind.Identifier, RawKind.Equal, RawKind.Ampersand)),
    new(GenericDeclaration.Parse, (RawKind.Identifier, RawKind.LBracket, RawKind.RBracket)), // empty generic param list
  ];

  internal static PredictionTable ModuleMembersTable => PredictionTable.Build(ModuleMemberPredictions);

  internal static RawKind MapRawKind(SyntaxKind kind) => kind switch
  {
    SyntaxKind.TerminatorToken => RawKind.Terminator,
    SyntaxKind.ExportMarkerToken => RawKind.LessLess,
    SyntaxKind.ModulePathOpenToken => RawKind.LBracketLBracket,
    SyntaxKind.ModulePathCloseToken => RawKind.RBracketRBracket,
    SyntaxKind.ModulePathSepToken => RawKind.ColonColon,
    SyntaxKind.GenericOpenToken => RawKind.LBracket,
    SyntaxKind.GenericCloseToken => RawKind.RBracket,
    SyntaxKind.IdentifierToken => RawKind.Identifier,
    SyntaxKind.NumberToken => RawKind.NumberLiteral,
    SyntaxKind.StrToken => RawKind.StringLiteral,
    SyntaxKind.EqualToken => RawKind.Equal,
    SyntaxKind.StructToken => RawKind.PercentLBrace,
    SyntaxKind.UnionToken => RawKind.PipeLBrace,
    SyntaxKind.AmpersandToken => RawKind.Ampersand,
    SyntaxKind.OpenBraceToken => RawKind.LBrace,
    SyntaxKind.CloseBraceToken => RawKind.RBrace,
    SyntaxKind.EobToken => RawKind.Eob,
    SyntaxKind.ColonToken => RawKind.Colon,
    SyntaxKind.ErrorToken => RawKind.Error,
    _ => RawKind.Error
  };

  static SyntaxKind MapStandaloneSyntaxKind(RawKind kind) => kind switch
  {
    RawKind.Terminator => SyntaxKind.TerminatorToken,
    _ => SyntaxKind.ErrorToken
  };

  static Parser()
  {
#if DEBUG
    static void Validate(ReadOnlySpan<Prediction> preds, string name)
    {
      HashSet<string> seen = [];
      foreach (Prediction e in preds)
      {
        TokenSequence ls = e.Sequence;
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < ls.Length; i++)
        {
          if (i > 0) sb.Append(',');
          sb.Append((int)ls[i]);
        }
        string key = sb.ToString();
        if (!seen.Add(key))
          throw new InvalidOperationException($"Duplicate TokenSequence in prediction table '{name}': {key}");
      }
    }

    Validate(ModuleMemberPredictions, nameof(ModuleMemberPredictions));
#endif
  }
}
