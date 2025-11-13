using Brim.Parse.Collections;
using Brim.Parse.Green;

namespace Brim.Parse;

public sealed partial class Parser
{
  // Member / declaration predictions.
  internal static readonly Prediction[] ModuleMemberPredictions =
  [
    new(ExportList.Parse, TokenKind.LessLess),
    new(ImportDeclaration.Parse, (TokenKind.Identifier, TokenKind.ColonColonEqual, TokenKind.Identifier)),

    // Mutable value declaration: '^' Ident ':' Type '=' Initializer Terminator
    new(ValueDeclaration.Parse, (TokenKind.Hat, TokenKind.Identifier, TokenKind.Colon)),
    new(ServiceImpl.Parse, TokenKind.Atmark),

    // Service lifecycle/protocol declarations: Ident [<...>]? [(receiver)]? '{'
    // We need LL(2) to distinguish from type/value declarations
    new(ServiceLifecycleDecl.Parse, (TokenKind.Identifier, TokenKind.LBrace)),
    new(ServiceProtocolDecl.Parse, (TokenKind.Identifier, TokenKind.Less)),
    new(ServiceProtocolDecl.Parse, (TokenKind.Identifier, TokenKind.LParen)),

    // Identifier-headed declarations (types, canonical values, etc.)
    new(ParseIdentifierHead, TokenKind.Identifier),

    // Type shape declaration: Ident ':=' Shape
  ];

  internal static readonly PredictionTable ModuleMembersTable = PredictionTable.Build(ModuleMemberPredictions);

  internal static TokenKind MapTokenKind(SyntaxKind kind) => kind switch
  {
    SyntaxKind.TerminatorToken => TokenKind.Terminator,
    SyntaxKind.QuestionToken => TokenKind.Question,
    SyntaxKind.BangToken => TokenKind.Bang,
    SyntaxKind.MatchGuardToken => TokenKind.QuestionQuestion,
    SyntaxKind.ExportOpenToken => TokenKind.LessLess,
    SyntaxKind.ModulePathOpenToken => TokenKind.EqualLBracket,
    SyntaxKind.ModulePathCloseToken => TokenKind.RBracketEqual,
    SyntaxKind.ModulePathSepToken => TokenKind.ColonColon,
    SyntaxKind.ModuleBindToken => TokenKind.ColonColonEqual,
    SyntaxKind.GenericOpenToken => TokenKind.LBracket,
    SyntaxKind.GenericCloseToken => TokenKind.RBracket,
    SyntaxKind.OpenParenToken => TokenKind.LParen,
    SyntaxKind.CloseParenToken => TokenKind.RParen,
    SyntaxKind.IdentifierToken => TokenKind.Identifier,
    SyntaxKind.StopToken => TokenKind.Stop,
    SyntaxKind.MutableToken => TokenKind.Hat,
    SyntaxKind.TildeToken => TokenKind.Tilde,
    SyntaxKind.IntToken => TokenKind.IntegerLiteral,
    SyntaxKind.DecimalToken => TokenKind.DecimalLiteral,
    SyntaxKind.StrToken => TokenKind.StringLiteral,
    SyntaxKind.RuneToken => TokenKind.RuneLiteral,
    SyntaxKind.EqualToken => TokenKind.Equal,
    SyntaxKind.AmpersandToken => TokenKind.Ampersand,
    SyntaxKind.OpenBlockToken => TokenKind.LBrace,
    SyntaxKind.CloseBlockToken => TokenKind.RBrace,
    SyntaxKind.EobToken => TokenKind.Eob,
    SyntaxKind.ColonToken => TokenKind.Colon,
    SyntaxKind.LessToken => TokenKind.Less,
    SyntaxKind.GreaterToken => TokenKind.Greater,
    SyntaxKind.ExportEndToken => TokenKind.GreaterGreater,
    SyntaxKind.PlusToken => TokenKind.Plus,
    SyntaxKind.MinusToken => TokenKind.Minus,
    SyntaxKind.LambdaOpenToken => TokenKind.Pipe,
    SyntaxKind.LambdaCloseToken => TokenKind.PipeGreater,
    SyntaxKind.EmptyLambaToken => TokenKind.PipePipeGreater,
    SyntaxKind.StarToken => TokenKind.Star,
    SyntaxKind.SlashToken => TokenKind.Slash,
    SyntaxKind.PercentToken => TokenKind.Percent,
    SyntaxKind.CommaToken => TokenKind.Comma,
    SyntaxKind.ErrorToken => TokenKind.Error,
    SyntaxKind.ServiceImplToken => TokenKind.Atmark,
    SyntaxKind.TypeBindToken => TokenKind.ColonEqual,
    SyntaxKind.ArrowToken => TokenKind.EqualGreater,
    SyntaxKind.StructToken => TokenKind.PercentLBrace,
    SyntaxKind.FlagsToken => TokenKind.AmpersandLBrace,
    SyntaxKind.FlagsPatternToken => TokenKind.AmpersandLParen,
    SyntaxKind.UnionToken => TokenKind.PipeLBrace,
    SyntaxKind.ProtocolToken => TokenKind.StopLBrace,
    SyntaxKind.ServiceToken => TokenKind.AtmarkLBrace,
    SyntaxKind.NamedTupleToken => TokenKind.HashLBrace,
    SyntaxKind.CastToken => TokenKind.ColonGreater,
    SyntaxKind.AmpersandAmpersandToken => TokenKind.AmpersandAmpersand,
    SyntaxKind.PipePipeToken => TokenKind.PipePipe,
    SyntaxKind.EqualEqualToken => TokenKind.EqualEqual,
    SyntaxKind.BangEqualToken => TokenKind.BangEqual,
    SyntaxKind.LessEqualToken => TokenKind.LessEqual,
    SyntaxKind.GreaterEqualToken => TokenKind.GreaterEqual,
    SyntaxKind.ServiceHandleToken => TokenKind.Atmark,
    SyntaxKind.OptionConstruct => TokenKind.QuestionLBrace,
    SyntaxKind.ResultConstruct => TokenKind.BangLBrace,
    SyntaxKind.ErrorConstruct => TokenKind.BangBangLBrace,
    _ => throw new ArgumentOutOfRangeException(nameof(kind), $"No mapping for SyntaxKind '{kind}'. Add mapping in Parser.MapRawKind."),
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
