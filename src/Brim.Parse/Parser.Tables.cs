using Brim.Parse.Collections;
using Brim.Parse.Green;

namespace Brim.Parse;

public sealed partial class Parser
{
    // Member / declaration predictions.
    internal static readonly Prediction[] ModuleMemberPredictions =
    [
      new(ExportList.Parse, RawKind.LessLess),
    new(ImportDeclaration.Parse, (RawKind.Identifier, RawKind.ColonColonEqual, RawKind.Identifier)),

    // Mutable value declaration: '^' Ident ':' Type '=' Initializer Terminator
    new(ValueDeclaration.Parse, (RawKind.Hat, RawKind.Identifier, RawKind.Colon)),
    new(ServiceImpl.Parse, RawKind.Atmark),

    // Service lifecycle/protocol declarations: Ident [<...>]? [(receiver)]? '{'
    // We need LL(2) to distinguish from type/value declarations
    new(ServiceLifecycleDecl.Parse, (RawKind.Identifier, RawKind.LBrace)),
    new(ServiceProtocolDecl.Parse, (RawKind.Identifier, RawKind.Less)),
    new(ServiceProtocolDecl.Parse, (RawKind.Identifier, RawKind.LParen)),

    // Identifier-headed declarations (types, canonical values, etc.)
    new(ParseIdentifierHead, RawKind.Identifier),

    // Type shape declaration: Ident ':=' Shape
  ];

    internal static readonly PredictionTable ModuleMembersTable = PredictionTable.Build(ModuleMemberPredictions);

    internal static RawKind MapRawKind(SyntaxKind kind) => kind switch
    {
        SyntaxKind.TerminatorToken => RawKind.Terminator,
        SyntaxKind.QuestionToken => RawKind.Question,
        SyntaxKind.BangToken => RawKind.Bang,
        SyntaxKind.MatchGuardToken => RawKind.QuestionQuestion,
        SyntaxKind.ExportOpenToken => RawKind.LessLess,
        SyntaxKind.ModulePathOpenToken => RawKind.EqualLBracket,
        SyntaxKind.ModulePathCloseToken => RawKind.RBracketEqual,
        SyntaxKind.ModulePathSepToken => RawKind.ColonColon,
        SyntaxKind.ModuleBindToken => RawKind.ColonColonEqual,
        SyntaxKind.GenericOpenToken => RawKind.LBracket,
        SyntaxKind.GenericCloseToken => RawKind.RBracket,
        SyntaxKind.OpenParenToken => RawKind.LParen,
        SyntaxKind.CloseParenToken => RawKind.RParen,
        SyntaxKind.IdentifierToken => RawKind.Identifier,
        SyntaxKind.StopToken => RawKind.Stop,
        SyntaxKind.HatToken => RawKind.Hat,
        SyntaxKind.TildeToken => RawKind.Tilde,
        SyntaxKind.IntToken => RawKind.IntegerLiteral,
        SyntaxKind.DecimalToken => RawKind.DecimalLiteral,
        SyntaxKind.StrToken => RawKind.StringLiteral,
        SyntaxKind.RuneToken => RawKind.RuneLiteral,
        SyntaxKind.EqualToken => RawKind.Equal,
        SyntaxKind.AmpersandToken => RawKind.Ampersand,
        SyntaxKind.OpenBraceToken => RawKind.LBrace,
        SyntaxKind.CloseBlockToken => RawKind.RBrace,
        SyntaxKind.EobToken => RawKind.Eob,
        SyntaxKind.ColonToken => RawKind.Colon,
        SyntaxKind.LessToken => RawKind.Less,
        SyntaxKind.GreaterToken => RawKind.Greater,
        SyntaxKind.ExportEndToken => RawKind.GreaterGreater,
        SyntaxKind.PlusToken => RawKind.Plus,
        SyntaxKind.MinusToken => RawKind.Minus,
        SyntaxKind.LambdaOpenToken => RawKind.Pipe,
        SyntaxKind.LambdaCloseToken => RawKind.PipeGreater,
        SyntaxKind.StarToken => RawKind.Star,
        SyntaxKind.SlashToken => RawKind.Slash,
        SyntaxKind.PercentToken => RawKind.Percent,
        SyntaxKind.CommaToken => RawKind.Comma,
        SyntaxKind.ErrorToken => RawKind.Error,
        SyntaxKind.ServiceImplToken => RawKind.Atmark,
        SyntaxKind.TypeBindToken => RawKind.ColonEqual,
        SyntaxKind.ArrowToken => RawKind.EqualGreater,
        SyntaxKind.StructToken => RawKind.PercentLBrace,
        SyntaxKind.FlagsToken => RawKind.AmpersandLBrace,
        SyntaxKind.UnionToken => RawKind.PipeLBrace,
        SyntaxKind.ProtocolToken => RawKind.StopLBrace,
        SyntaxKind.ServiceToken => RawKind.AtmarkLBrace,
        SyntaxKind.NamedTupleToken => RawKind.HashLBrace,
        SyntaxKind.CastToken => RawKind.ColonGreater,
        SyntaxKind.AmpersandAmpersandToken => RawKind.AmpersandAmpersand,
        SyntaxKind.PipePipeToken => RawKind.PipePipe,
        SyntaxKind.EqualEqualToken => RawKind.EqualEqual,
        SyntaxKind.BangEqualToken => RawKind.BangEqual,
        SyntaxKind.LessEqualToken => RawKind.LessEqual,
        SyntaxKind.GreaterEqualToken => RawKind.GreaterEqual,
        _ => RawKind.Error
    };

    static SyntaxKind MapStandaloneSyntaxKind(RawKind kind) => kind switch
    {
        RawKind.CommentTrivia => SyntaxKind.Comment,
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
