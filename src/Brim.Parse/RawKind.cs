namespace Brim.Parse;

/// <summary>
/// Kinds of tokens in Brim source code.
/// </summary>
public enum RawKind : short
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
  Atmark, // @

  // Possible compound glyph runs
  Less, LessLess, // < <<
  Equal, EqualGreater, // = =>
  Minus, CommentTrivia, // - --
  Star, StarLBrace, // * *{
  Tilde, TildeEqual, // ~ ~=
  LBracket, LBracketLBracket, // [ [[
  RBracket, RBracketRBracket, // ] ]]
  Colon, ColonStar, ColonEqual, ColonColon, // : :* := ::
  ColonColonEqual, // ::= (module bind)
  Pipe, PipeLBrace, // | |{
  Hash, HashLParen, HashLBrace, // # #( #{
  Ampersand, // &
  Percent, PercentLBrace, // % %{
  Stop, StopLBrace, StopEqual, // . .{ .=
  Question, QuestionLParen, QuestionLBrace, QuestionQuestion, // ? ?( ?{ ??
  Bang, BangEqual, BangLBrace, BangBangLBrace,  // ! != !{ !!{

  // Literals
  IntegerLiteral,
  DecimalLiteral,
  StringLiteral,

  // Trivia
  WhitespaceTrivia,

  // Synthesized end-of-file (always emitted exactly once by new producers)
  Eob = short.MaxValue,
}
