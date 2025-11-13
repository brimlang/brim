namespace Brim.Core;

/// <summary>
/// Kinds of tokens in Brim source code.
/// </summary>
public enum TokenKind : sbyte
{
  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of special kinds that are not real tokens.
  /// </summary>
  _SentinelSpecial = -10,
  Any = -2, // Wildcard for parsing

  /// <summary>
  /// Default uninitialized value.
  /// </summary>
  Unitialized = 0,

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of trivia token kinds.
  /// </summary>
  _SentinelTrivia = 1,

  CommentTrivia,
  WhitespaceTrivia,

  _SentinelCore = 10,

  Identifier,
  Terminator,

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of glyph token kinds.
  /// </summary>
  _SentinelGlyphs = 20,

  // Single glyphs -- no compound forms
  LParen, // (
  RParen, // )
  LBrace, // {
  RBrace, // }
  Comma, // ,
  Hat, //^
  Plus, // +
  Minus, // -
  MinusGreater, // ->
  Slash, // /
  Backslash, // \

  // Possible compound glyph runs
  Atmark, AtmarkLParen, AtmarkLBrace, // @ @( @{
  Less, LessLess, LessEqual, // < << <=
  Greater, GreaterGreater, GreaterEqual, // > >> >=
  Equal, EqualGreater, EqualLBracket, EqualEqual, // = => =[ ==
  Star, StarLBrace, // * *{
  Tilde, TildeEqual, // ~ ~=
  LBracket, // [
  RBracket, RBracketEqual, // ] ]=
  Colon, ColonColonEqual, ColonEqual, ColonColon, ColonGreater, // : ::= := :: :>
  Pipe, PipeLParen, PipeLBrace, PipePipe, PipeGreater, PipePipeGreater, // | |( |{ || |> ||>
  Hash, HashLParen, HashLBrace, // # #( #{
  Percent, PercentLParen, PercentLBrace, // % %( %{
  Stop, StopLBrace, StopStop, // . .{ ..
  Question, QuestionLBrace, QuestionQuestion, // ? ?{ ??
  Bang, BangEqual, BangLBrace, BangBangLBrace,  // ! != !{ !!{
  Ampersand, AmpersandAmpersand, AmpersandLParen, AmpersandLBrace, // & && &( &{

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of literal token kinds.
  /// </summary>
  _SentinelLiteral = 105,

  IntegerLiteral,
  DecimalLiteral,
  StringLiteral,
  RuneLiteral,
  BooleanLiteral,

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of synthetic token kinds.
  /// </summary>
  _SentinelSynthetic = 120,

  Error,
  Missing,

  /// <summary>
  /// End of buffer/input. Emitted once at the end of the token stream.
  /// </summary>
  Eob = sbyte.MaxValue,
}
