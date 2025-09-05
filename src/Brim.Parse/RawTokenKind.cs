namespace Brim.Parse;

/// <summary>
/// Kinds of tokens in Brim source code.
/// </summary>
public enum RawTokenKind
{
  // Special
  Any = -1, // Wildcard for parsing
  Default = 0, // Uninitialized
  Identifier,
  Terminator,
  Error,

  // Single glyphs -- no compound forms
  LParen, // (
  RParen, // )
  LBrace, // {
  RBrace, // }
  Comma, // ,
  Hat, //^
  Plus, // +
  Greater, // >
  Slash, // /
  Backslash, // \
  Stop, // .

  // Possible compound glyph runs
  Less, LessLess, LessAt, // < << <@
  Equal, EqualGreater, // = =>
  Minus, CommentTrivia, // - --
  Star, StarLBrace, // * *{
  Tilde, TildeEqual, // ~ ~=
  LBracket, LBracketLBracket, // [ [[
  RBracket, RBracketRBracket, // ] ]]
  Atmark, AtmarkLBrace, AtmarkRBrace, AtmarkGreater, // @ @{ @} @>
  Colon, ColonStar, ColonEqual, ColonColon, // : :* := ::
  Pipe, PipeLBrace, // | |{
  Hash, HashLParen, // # #(
  Ampersand, // &
  Percent, PercentLBrace, // % %{
  Question, QuestionLParen, // ? ?(

  // Literals
  NumberLiteral,
  StringLiteral,

  // Trivia
  WhitespaceTrivia,

  // Synthesized end-of-file (always emitted exactly once by new producers)
  Eob = short.MaxValue,
}

