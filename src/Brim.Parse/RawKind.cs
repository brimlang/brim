namespace Brim.Parse;

/// <summary>
/// Kinds of tokens in Brim source code.
/// </summary>
public enum RawKind : sbyte
{
  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of special kinds that are not real tokens.
  /// </summary>
  _SentinelSpecial = sbyte.MinValue,
  Any = -1, // Wildcard for parsing

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of "normal" token kinds. Doubles as uninitialized.
  /// </summary>
  _SentinelDefault = 0,

  Error,
  Identifier,
  Terminator,

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of glyph token kinds.
  /// </summary>
  _SentinelGlyphs = 5,

  // Single glyphs -- no compound forms
  LParen, // (
  RParen, // )
  LBrace, // {
  RBrace, // }
  Comma, // ,
  Hat, //^
  Plus, // +
  Minus, // -
  Greater, // >
  Slash, // /
  Backslash, // \
  Ampersand, // &   --TODO &[u8]{read, write}

  // Possible compound glyph runs
  Atmark, AtmarkLBrace, // @ @{
  Less, LessLess, // < <<
  Equal, EqualGreater, // = =>
  Star, StarLBrace, // * *{
  Tilde, TildeEqual, // ~ ~=
  LBracket, LBracketLBracket, // [ [[
  RBracket, RBracketRBracket, // ] ]]
  Colon, ColonColonEqual, ColonStar, ColonEqual, ColonColon, // : ::= :* := ::
  Pipe, PipeLBrace, // | |{
  Hash, HashLParen, HashLBrace, // # #( #{
  Percent, PercentLBrace, // % %{
  Stop, StopLBrace, StopEqual, // . .{ .=
  Question, QuestionLParen, QuestionLBrace, QuestionQuestion, // ? ?( ?{ ??
  Bang, BangEqual, BangLBrace, BangBangLBrace,  // ! != !{ !!{

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of literal token kinds.
  /// </summary>
  _SentinelLiteral = 115,

  IntegerLiteral,
  DecimalLiteral,
  StringLiteral,

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of trivia token kinds.
  /// </summary>
  _SentinelTrivia = 120,

  CommentTrivia,
  WhitespaceTrivia,

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of synthetic token kinds.
  /// </summary>
  _SentinelSynthetic = sbyte.MaxValue - 2,

  /// <summary>
  /// End of buffer/input. Emitted once at the end of the token stream.
  /// </summary>
  Eob = sbyte.MaxValue,
}
