using Brim.Parse.Collections;
using Brim.Parse.Green;

namespace Brim.Parse;

public sealed partial class Parser
{
  // Member / declaration predictions.
  internal static readonly Prediction[] ModuleMemberPredictions =
  [
    new(ExportDirective.Parse, RawKind.LessLess),
    new(ImportDeclaration.Parse, (RawKind.Identifier, RawKind.ColonColonEqual, RawKind.Identifier)),
    // Protocol and Service declarations (headers only for now)
    new(ProtocolDeclaration.Parse, (RawKind.Identifier, RawKind.Colon, RawKind.Stop)),
    new(ServiceDeclaration.Parse, (RawKind.Identifier, RawKind.Colon, RawKind.Hat)),
    // Type declarations (generic heads and specific body openers)
    new(TypeDeclaration.Parse, (RawKind.Identifier, RawKind.LBracket, RawKind.Identifier, RawKind.Any)),
    // Alias form: Name := TypeName (Identifier ...)
    new(TypeDeclaration.Parse, (RawKind.Identifier, RawKind.ColonEqual, RawKind.Identifier)),
    new(TypeDeclaration.Parse, (RawKind.Identifier, RawKind.ColonEqual, RawKind.PercentLBrace)),
    new(TypeDeclaration.Parse, (RawKind.Identifier, RawKind.ColonEqual, RawKind.PipeLBrace)),
    new(TypeDeclaration.Parse, (RawKind.Identifier, RawKind.ColonEqual, RawKind.Ampersand)),
    new(TypeDeclaration.Parse, (RawKind.Identifier, RawKind.ColonEqual, RawKind.HashLBrace)),
    new(TypeDeclaration.Parse, (RawKind.Identifier, RawKind.LBracket, RawKind.RBracket)), // empty generic param list
  ];

  internal static PredictionTable ModuleMembersTable => PredictionTable.Build(ModuleMemberPredictions);

  internal static RawKind MapRawKind(SyntaxKind kind) => kind switch
  {
    SyntaxKind.TerminatorToken => RawKind.Terminator,
    SyntaxKind.QuestionToken => RawKind.Question,
    SyntaxKind.BangToken => RawKind.Bang,
    SyntaxKind.ExportMarkerToken => RawKind.LessLess,
    SyntaxKind.ModulePathOpenToken => RawKind.LBracketLBracket,
    SyntaxKind.ModulePathCloseToken => RawKind.RBracketRBracket,
    SyntaxKind.ModulePathSepToken => RawKind.ColonColon,
    SyntaxKind.ModuleBindToken => RawKind.ColonColonEqual,
    SyntaxKind.GenericOpenToken => RawKind.LBracket,
    SyntaxKind.GenericCloseToken => RawKind.RBracket,
    SyntaxKind.OpenParenToken => RawKind.LParen,
    SyntaxKind.CloseParenToken => RawKind.RParen,
    SyntaxKind.IdentifierToken => RawKind.Identifier,
    SyntaxKind.StopToken => RawKind.Stop,
    SyntaxKind.HatToken => RawKind.Hat,
    SyntaxKind.IntToken => RawKind.IntegerLiteral,
    SyntaxKind.DecimalToken => RawKind.DecimalLiteral,
    SyntaxKind.StrToken => RawKind.StringLiteral,
    SyntaxKind.EqualToken => RawKind.Equal,
    SyntaxKind.StructToken => RawKind.PercentLBrace,
    SyntaxKind.UnionToken => RawKind.PipeLBrace,
    SyntaxKind.AmpersandToken => RawKind.Ampersand,
    SyntaxKind.OpenBraceToken => RawKind.LBrace,
    SyntaxKind.CloseBraceToken => RawKind.RBrace,
    SyntaxKind.EobToken => RawKind.Eob,
    SyntaxKind.ColonToken => RawKind.Colon,
    SyntaxKind.TypeBindToken => RawKind.ColonEqual,
    SyntaxKind.CommaToken => RawKind.Comma,
    SyntaxKind.ErrorToken => RawKind.Error,
    SyntaxKind.NamedTupleToken => RawKind.HashLBrace,
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
