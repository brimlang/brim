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
  Missing,

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of glyph token kinds.
  /// </summary>
  _SentinelGlyphs = 30,

  // Single glyphs -- no compound forms
  LParen, // (
  RParen, // )
  LBrace, // {
  RBrace, // }
  Comma, // ,
  Hat, //^
  Plus, // +
  Minus, // -
  Slash, // /
  Backslash, // \

  // Possible compound glyph runs
  Atmark, AtmarkLParen, AtmarkLBrace, // @ @( @{
  Less, LessLess, // < <<
  Greater, GreaterGreater, // > >>
  Equal, EqualGreater, EqualLBracket, // = => =[
  Star, StarLBrace, // * *{
  Tilde, TildeEqual, // ~ ~=
  LBracket, // [
  RBracket, RBracketEqual, // ] ]=
  Colon, ColonColonEqual, ColonEqual, ColonColon, // : ::= := ::
  Pipe, PipeLParen, PipeLBrace, // | |( |{
  Hash, HashLParen, HashLBrace, // # #( #{
  Percent, PercentLParen, PercentLBrace, // % %( %{
  Stop, StopLBrace, // . .{
  Question, QuestionLBrace, QuestionQuestion, // ? ?{ ??
  Bang, BangEqual, BangLBrace, BangBangLBrace,  // ! != !{ !!{
  Ampersand, AmpersandAmpersand, AmpersandLParen, AmpersandLBrace, // & && &( &{

  // Additional compound operators based on character sequences
  ColonGreater, // :> (cast operator)
  StopStop, // .. (rest pattern)
  LessEqual, // <=
  GreaterEqual, // >=
  EqualEqual, // ==
  PipePipe, // ||

  /// <summary>
  /// Sentinel values indicating unused range for future use.
  _SentinelUnusedRangeStart = 90,
  _SentinelUnusedRangeEnd = 114,

  /// <summary>
  /// Sentinel value indicating the start of the range
  /// of literal token kinds.
  /// </summary>
  _SentinelLiteral = 115,

  IntegerLiteral,
  DecimalLiteral,
  StringLiteral,
  RuneLiteral,

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
  _SentinelSynthetic = 125,
  _Reserved126 = 126,

  /// <summary>
  /// End of buffer/input. Emitted once at the end of the token stream.
  /// </summary>
  Eob = sbyte.MaxValue,
}
