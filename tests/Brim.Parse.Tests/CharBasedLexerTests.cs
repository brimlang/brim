using Brim.Parse.Collections;
using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

/// <summary>
/// Comprehensive test suite for the char-based lexer (RawProducer).
/// Tests focus on Unicode correctness, compound token greedy matching,
/// keyword recognition, rune literals, and edge cases.
/// </summary>
public class CharBasedLexerTests
{
  /// <summary>
  /// Lex string input and return all tokens except EOB.
  /// </summary>
  static List<RawToken> Lex(string input)
  {
    RawProducer producer = new(SourceText.From(input), DiagnosticList.Create());
    List<RawToken> tokens = [];

    while (producer.TryRead(out RawToken token))
    {
      if (token.Kind == RawKind.Eob) break;
      tokens.Add(token);
    }

    return tokens;
  }

  /// <summary>
  /// Lex input and return tokens with their text content.
  /// </summary>
  static List<(RawKind Kind, string Text)> LexWithText(string input)
  {
    SourceText source = SourceText.From(input);
    List<(RawKind, string)> result = [];

    foreach (RawToken token in Lex(input))
    {
      string text = new(token.Value(source.Span));
      result.Add((token.Kind, text));
    }

    return result;
  }

  /// <summary>
  /// Lex input and return diagnostics encountered.
  /// </summary>
  static List<Diagnostic> LexWithDiagnostics(string input)
  {
    DiagnosticList sink = DiagnosticList.Create();
    RawProducer producer = new(SourceText.From(input), sink);

    while (producer.TryRead(out RawToken token))
    {
      if (token.Kind == RawKind.Eob) break;
    }

    return [.. sink.GetSortedDiagnostics()];
  }

  #region Basic Lexer Tests

  [Fact]
  public void EmptyInput_ProducesOnlyEob()
  {
    List<RawToken> tokens = Lex("");
    Assert.Empty(tokens);
  }

  [Fact]
  public void WhitespaceOnly_ProducesWhitespaceToken()
  {
    List<RawToken> tokens = Lex("   \t  ");
    Assert.Single(tokens);
    Assert.Equal(RawKind.WhitespaceTrivia, tokens[0].Kind);
  }

  [Fact]
  public void NewlineOnly_ProducesTerminatorToken()
  {
    List<RawToken> tokens = Lex("\n");
    Assert.Single(tokens);
    Assert.Equal(RawKind.Terminator, tokens[0].Kind);
  }

  [Fact]
  public void MixedWhitespace_ProducesCorrectTokens()
  {
    var tokens = LexWithText("  \t\n  ");
    Assert.Equal(3, tokens.Count);
    Assert.Equal((RawKind.WhitespaceTrivia, "  \t"), tokens[0]);
    Assert.Equal((RawKind.Terminator, "\n"), tokens[1]);
    Assert.Equal((RawKind.WhitespaceTrivia, "  "), tokens[2]);
  }

  #endregion

  #region Unicode Identifier Tests

  [Theory]
  [InlineData("identifier", RawKind.Identifier)]
  [InlineData("_underscore", RawKind.Identifier)]
  [InlineData("_", RawKind.Identifier)]
  [InlineData("a1", RawKind.Identifier)]
  [InlineData("test123", RawKind.Identifier)]
  [InlineData("PascalCase", RawKind.Identifier)]
  [InlineData("camelCase", RawKind.Identifier)]
  public void BasicIdentifiers_LexCorrectly(string identifier, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(identifier);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("café", RawKind.Identifier)]          // French accents
  [InlineData("變量名", RawKind.Identifier)]         // Chinese characters
  [InlineData("العربية", RawKind.Identifier)]       // Arabic script
  [InlineData("переменная", RawKind.Identifier)]    // Cyrillic script
  [InlineData("π", RawKind.Identifier)]             // Greek letter
  [InlineData("Ω", RawKind.Identifier)]             // Greek capital
  [InlineData("naïve", RawKind.Identifier)]         // Diacritic
  [InlineData("José", RawKind.Identifier)]          // Spanish accent
  [InlineData("Björk", RawKind.Identifier)]         // Nordic characters
  [InlineData("हिंदी", RawKind.Identifier)]           // Hindi Devanagari
  [InlineData("日本語", RawKind.Identifier)]          // Japanese
  public void UnicodeIdentifiers_LexCorrectly(string identifier, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(identifier);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Fact]
  public void IdentifierNormalization_PreservesOriginalForm()
  {
    // Test that the lexer preserves original identifier form
    // Normalization happens during semantic analysis if needed
    string composed = "café";           // U+0063 U+0061 U+0066 U+00E9
    string decomposed = "cafe\u0301";   // U+0063 U+0061 U+0066 U+0065 U+0301

    var composedTokens = LexWithText(composed);
    var decomposedTokens = LexWithText(decomposed);

    Assert.Single(composedTokens);
    Assert.Single(decomposedTokens);
    Assert.Equal(RawKind.Identifier, composedTokens[0].Kind);
    Assert.Equal(RawKind.Identifier, decomposedTokens[0].Kind);
    Assert.Equal("café", composedTokens[0].Text);
    Assert.Equal("cafe\u0301", decomposedTokens[0].Text);
  }

  #endregion

  #region Keyword Tests

  [Theory]
  [InlineData("true", RawKind.Identifier)]
  [InlineData("false", RawKind.Identifier)]
  [InlineData("void", RawKind.Identifier)]
  [InlineData("unit", RawKind.Identifier)]
  [InlineData("bool", RawKind.Identifier)]
  [InlineData("str", RawKind.Identifier)]
  [InlineData("rune", RawKind.Identifier)]
  [InlineData("err", RawKind.Identifier)]
  public void BasicKeywords_RecognizedCorrectly(string keyword, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(keyword);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("i8", RawKind.Identifier)]
  [InlineData("i16", RawKind.Identifier)]
  [InlineData("i32", RawKind.Identifier)]
  [InlineData("i64", RawKind.Identifier)]
  [InlineData("u8", RawKind.Identifier)]
  [InlineData("u16", RawKind.Identifier)]
  [InlineData("u32", RawKind.Identifier)]
  [InlineData("u64", RawKind.Identifier)]
  [InlineData("f32", RawKind.Identifier)]
  [InlineData("f64", RawKind.Identifier)]
  public void NumericTypeKeywords_RecognizedCorrectly(string keyword, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(keyword);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("True")]   // Wrong case
  [InlineData("TRUE")]   // Wrong case
  [InlineData("truE")]   // Wrong case
  [InlineData("i32x")]   // Extended
  [InlineData("u8_")]    // With underscore
  public void NonKeywords_LexAsIdentifiers(string input)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(RawKind.Identifier, tokens[0].Kind);
  }

  [Fact]
  public void KeywordsWithSpacing_LexSeparately()
  {
    var tokens = LexWithText("true false");
    Assert.Equal(3, tokens.Count);
    Assert.Equal((RawKind.Identifier, "true"), tokens[0]);
    Assert.Equal((RawKind.WhitespaceTrivia, " "), tokens[1]);
    Assert.Equal((RawKind.Identifier, "false"), tokens[2]);
  }

  #endregion

  #region Compound Token Tests

  [Theory]
  [InlineData(":=", RawKind.ColonEqual)]
  [InlineData("::=", RawKind.ColonColonEqual)]
  [InlineData("::", RawKind.ColonColon)]
  [InlineData(":>", RawKind.ColonGreater)]
  public void ColonOperators_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("&&", RawKind.AmpersandAmpersand)]
  [InlineData("||", RawKind.PipePipe)]
  [InlineData("==", RawKind.EqualEqual)]
  [InlineData("!=", RawKind.BangEqual)]
  [InlineData("<=", RawKind.LessEqual)]
  [InlineData(">=", RawKind.GreaterEqual)]
  [InlineData(">>", RawKind.GreaterGreater)]
  [InlineData("<<", RawKind.LessLess)]
  public void LogicalOperators_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("[[", RawKind.LBracketLBracket)]
  [InlineData("]]", RawKind.RBracketRBracket)]
  [InlineData("!{", RawKind.BangLBrace)]
  [InlineData("!!{", RawKind.BangBangLBrace)]
  [InlineData("?{", RawKind.QuestionLBrace)]
  [InlineData("#{", RawKind.HashLBrace)]
  [InlineData(".{", RawKind.StopLBrace)]
  [InlineData("@{", RawKind.AtmarkLBrace)]
  [InlineData("*{", RawKind.StarLBrace)]
  [InlineData("|{", RawKind.PipeLBrace)]
  [InlineData("%{", RawKind.PercentLBrace)]
  [InlineData("&{", RawKind.AmpersandLBrace)]
  public void BraceOperators_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("=>", RawKind.EqualGreater)]
  [InlineData("~=", RawKind.TildeEqual)]
  [InlineData("..", RawKind.StopStop)]
  [InlineData("??", RawKind.QuestionQuestion)]
  public void MiscOperators_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("#(", RawKind.HashLParen)]
  [InlineData("%(", RawKind.PercentLParen)]
  [InlineData("|(", RawKind.PipeLParen)]
  [InlineData("&(", RawKind.AmpersandLParen)]
  [InlineData("@(", RawKind.AtmarkLParen)]
  public void SigilParenOperators_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Fact]
  public void CompoundTokens_GreedyMatching_LongestWins()
  {
    // Should lex as ColonColonEqual, not Colon + ColonEqual
    List<RawToken> tokens = Lex("::=");
    Assert.Single(tokens);
    Assert.Equal(RawKind.ColonColonEqual, tokens[0].Kind);
  }

  [Fact]
  public void CompoundTokens_GreedyMatching_BangBangLBrace()
  {
    // Should lex as BangBangLBrace, not Bang + BangLBrace
    List<RawToken> tokens = Lex("!!{");
    Assert.Single(tokens);
    Assert.Equal(RawKind.BangBangLBrace, tokens[0].Kind);
  }

  [Fact]
  public void CompoundTokens_WithSpacing_LexSeparately()
  {
    var tokens = LexWithText(": :=");
    Assert.Equal(3, tokens.Count);
    Assert.Equal((RawKind.Colon, ":"), tokens[0]);
    Assert.Equal((RawKind.WhitespaceTrivia, " "), tokens[1]);
    Assert.Equal((RawKind.ColonEqual, ":="), tokens[2]);
  }

  [Fact]
  public void CompoundTokens_PartialMatches_FallBackToShorter()
  {
    var tokens = LexWithText(":x");
    Assert.Equal(2, tokens.Count);
    Assert.Equal((RawKind.Colon, ":"), tokens[0]);
    Assert.Equal((RawKind.Identifier, "x"), tokens[1]);
  }

  #endregion

  #region Single Character Token Tests

  [Theory]
  [InlineData(":", RawKind.Colon)]
  [InlineData("!", RawKind.Bang)]
  [InlineData("?", RawKind.Question)]
  [InlineData("#", RawKind.Hash)]
  [InlineData("<", RawKind.Less)]
  [InlineData(".", RawKind.Stop)]
  [InlineData("@", RawKind.Atmark)]
  [InlineData("=", RawKind.Equal)]
  [InlineData("*", RawKind.Star)]
  [InlineData("~", RawKind.Tilde)]
  [InlineData("|", RawKind.Pipe)]
  [InlineData("%", RawKind.Percent)]
  [InlineData("[", RawKind.LBracket)]
  [InlineData("]", RawKind.RBracket)]
  [InlineData("-", RawKind.Minus)]
  [InlineData("&", RawKind.Ampersand)]
  [InlineData("(", RawKind.LParen)]
  [InlineData(")", RawKind.RParen)]
  [InlineData("{", RawKind.LBrace)]
  [InlineData("}", RawKind.RBrace)]
  [InlineData(",", RawKind.Comma)]
  [InlineData("^", RawKind.Hat)]
  [InlineData("+", RawKind.Plus)]
  [InlineData(">", RawKind.Greater)]
  [InlineData("/", RawKind.Slash)]
  [InlineData("\\", RawKind.Backslash)]
  public void SingleCharTokens_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  #endregion

  #region String Literal Tests

  [Theory]
  [InlineData("\"hello\"", RawKind.StringLiteral)]
  [InlineData("\"\"", RawKind.StringLiteral)]                    // Empty string
  [InlineData("\"world 123\"", RawKind.StringLiteral)]         // With numbers
  [InlineData("\"hello\\nworld\"", RawKind.StringLiteral)]     // With escape
  [InlineData("\"quote: \\\"test\\\"\"", RawKind.StringLiteral)] // Escaped quotes
  public void BasicStringLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("\"hello 世界\"", RawKind.StringLiteral)]         // Unicode content
  [InlineData("\"café العربية\"", RawKind.StringLiteral)]      // Mixed scripts
  [InlineData("\"Björk naïve José\"", RawKind.StringLiteral)] // European diacritics
  [InlineData("\"π Ω α β\"", RawKind.StringLiteral)]          // Greek letters
  public void UnicodeStringLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Fact]
  public void EmojiStringLiterals_LexCorrectly()
  {
    // Emoji in strings work fine since we don't parse individual characters
    List<RawToken> tokens = Lex("\"🚀 emoji test\"");
    Assert.Single(tokens);
    Assert.Equal(RawKind.StringLiteral, tokens[0].Kind);
  }

  [Fact]
  public void UnterminatedString_ProducesError()
  {
    var tokens = Lex("\"unterminated");
    Assert.Single(tokens);
    Assert.Equal(RawKind.Error, tokens[0].Kind);

    var diagnostics = LexWithDiagnostics("\"unterminated");
    Assert.Single(diagnostics);
    // Assuming UnterminatedString diagnostic exists
  }

  #endregion

  #region Rune Literal Tests

  [Theory]
  [InlineData("'a'", RawKind.RuneLiteral)]
  [InlineData("'1'", RawKind.RuneLiteral)]
  [InlineData("' '", RawKind.RuneLiteral)]   // Space
  [InlineData("'_'", RawKind.RuneLiteral)]   // Underscore
  public void BasicRuneLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("'π'", RawKind.RuneLiteral)]     // Greek letter
  [InlineData("'中'", RawKind.RuneLiteral)]    // Chinese character
  [InlineData("'🚀'", RawKind.RuneLiteral)]    // Emoji (4-byte UTF-8) - now works!
  [InlineData("'ñ'", RawKind.RuneLiteral)]     // Spanish n-tilde
  [InlineData("'ü'", RawKind.RuneLiteral)]     // German umlaut
  public void UnicodeRuneLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Fact]
  public void EmojiRuneLiteral_NowWorksCorrectly()
  {
    // With focused Rune support, emoji now work correctly
    var tokens = Lex("'🚀'");
    Assert.Single(tokens);
    Assert.Equal(RawKind.RuneLiteral, tokens[0].Kind);
  }

  [Theory]
  [InlineData("'\\n'", RawKind.RuneLiteral)]   // Newline escape
  [InlineData("'\\t'", RawKind.RuneLiteral)]   // Tab escape
  [InlineData("'\\r'", RawKind.RuneLiteral)]   // Carriage return
  [InlineData("'\\''", RawKind.RuneLiteral)]   // Single quote escape
  [InlineData("'\\\\'", RawKind.RuneLiteral)]  // Backslash escape
  public void EscapedRuneLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Fact]
  public void UnterminatedRuneLiteral_ProducesError()
  {
    var tokens = Lex("'a");
    Assert.Single(tokens);
    Assert.Equal(RawKind.Error, tokens[0].Kind);

    var diagnostics = LexWithDiagnostics("'a");
    Assert.Single(diagnostics);
    // Should be UnterminatedRune diagnostic
  }

  [Fact]
  public void MultipleRunesInLiteral_ProducesError()
  {
    var tokens = Lex("'ab'");
    Assert.Single(tokens);
    Assert.Equal(RawKind.Error, tokens[0].Kind);

    var diagnostics = LexWithDiagnostics("'ab'");
    Assert.Single(diagnostics);
    // Should be MultipleRunesInLiteral diagnostic
  }

  [Fact]
  public void MultipleEmojiInLiteral_ProducesError()
  {
    var tokens = Lex("'🚀🎯'");
    Assert.Single(tokens);
    Assert.Equal(RawKind.Error, tokens[0].Kind);

    var diagnostics = LexWithDiagnostics("'🚀🎯'");
    Assert.Single(diagnostics);
    // Should be MultipleRunesInLiteral diagnostic
  }

  [Fact]
  public void EmptyRuneLiteral_IsAccepted()
  {
    // Empty rune literal is lexically valid (semantic validation happens later)
    var tokens = Lex("''");
    Assert.Single(tokens);
    Assert.Equal(RawKind.RuneLiteral, tokens[0].Kind);
  }

  #endregion

  #region Number Literal Tests

  [Theory]
  [InlineData("0", RawKind.IntegerLiteral)]
  [InlineData("42", RawKind.IntegerLiteral)]
  [InlineData("123456", RawKind.IntegerLiteral)]
  [InlineData("999", RawKind.IntegerLiteral)]
  public void BasicIntegerLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("0x42", RawKind.IntegerLiteral)]
  [InlineData("0xFF", RawKind.IntegerLiteral)]
  [InlineData("0xDEADBEEF", RawKind.IntegerLiteral)]
  [InlineData("0x0", RawKind.IntegerLiteral)]
  public void HexIntegerLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("0b1010", RawKind.IntegerLiteral)]
  [InlineData("0b11111111", RawKind.IntegerLiteral)]
  [InlineData("0b0", RawKind.IntegerLiteral)]
  [InlineData("0b1", RawKind.IntegerLiteral)]
  public void BinaryIntegerLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("3.14", RawKind.DecimalLiteral)]
  [InlineData("0.0", RawKind.DecimalLiteral)]
  [InlineData("123.456", RawKind.DecimalLiteral)]
  public void DecimalLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("42i8", RawKind.IntegerLiteral)]
  [InlineData("42i16", RawKind.IntegerLiteral)]
  [InlineData("42i32", RawKind.IntegerLiteral)]
  [InlineData("42i64", RawKind.IntegerLiteral)]
  [InlineData("42u8", RawKind.IntegerLiteral)]
  [InlineData("42u16", RawKind.IntegerLiteral)]
  [InlineData("42u32", RawKind.IntegerLiteral)]
  [InlineData("42u64", RawKind.IntegerLiteral)]
  public void TypedIntegerLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("3.14f32", RawKind.DecimalLiteral)]
  [InlineData("3.14f64", RawKind.DecimalLiteral)]
  public void TypedDecimalLiterals_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  #endregion

  #region Comment Tests

  [Fact]
  public void LineComment_LexCorrectly()
  {
    List<RawToken> tokens = Lex("-- This is a comment");
    Assert.Single(tokens);
    Assert.Equal(RawKind.CommentTrivia, tokens[0].Kind);
  }

  [Fact]
  public void LineCommentWithUnicode_LexCorrectly()
  {
    List<RawToken> tokens = Lex("-- 这是注释 العربية comment 🚀");
    Assert.Single(tokens);
    Assert.Equal(RawKind.CommentTrivia, tokens[0].Kind);
  }

  [Fact]
  public void CommentFollowedByNewline_LexesBothTokens()
  {
    var tokens = LexWithText("-- comment\ntest");
    Assert.Equal(3, tokens.Count);
    Assert.Equal((RawKind.CommentTrivia, "-- comment"), tokens[0]);
    Assert.Equal((RawKind.Terminator, "\n"), tokens[1]);
    Assert.Equal((RawKind.Identifier, "test"), tokens[2]);
  }

  #endregion

  #region Terminator Tests

  [Theory]
  [InlineData("\n", "\n")]
  [InlineData("\n\n", "\n\n")]
  [InlineData(";", ";")]
  [InlineData(";;", ";;")]
  [InlineData(";\n", ";\n")]
  [InlineData("\n;", "\n;")]
  [InlineData(";\n;", ";\n;")]
  public void Terminators_LexCorrectly(string input, string expectedValue)
  {
    var tokens = LexWithText(input);
    Assert.Single(tokens);
    Assert.Equal(RawKind.Terminator, tokens[0].Kind);
    Assert.Equal(expectedValue, tokens[0].Text);
  }

  #endregion

  #region Whitespace and Error Tests

  [Theory]
  [InlineData(" ", RawKind.WhitespaceTrivia)]
  [InlineData("\t", RawKind.WhitespaceTrivia)]
  [InlineData("\r", RawKind.WhitespaceTrivia)]
  [InlineData("  \t\r  ", RawKind.WhitespaceTrivia)]
  public void AllowedWhitespace_LexCorrectly(string input, RawKind expectedKind)
  {
    List<RawToken> tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(expectedKind, tokens[0].Kind);
  }

  [Theory]
  [InlineData("\u00A0")]    // Non-breaking space
  [InlineData("\u2000")]    // En quad
  [InlineData("\u2001")]    // Em quad
  [InlineData("\u2028")]    // Line separator
  [InlineData("\u2029")]    // Paragraph separator
  public void UnsupportedWhitespace_ProducesError(string input)
  {
    var tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(RawKind.Error, tokens[0].Kind);

    var diagnostics = LexWithDiagnostics(input);
    Assert.Single(diagnostics);
    // Assuming UnsupportedWhitespace diagnostic exists
  }

  [Theory]
  [InlineData("§")]         // Section sign
  [InlineData("©")]         // Copyright
  [InlineData("®")]         // Registered trademark
  [InlineData("°")]         // Degree sign
  public void InvalidCharacters_ProduceError(string input)
  {
    var tokens = Lex(input);
    Assert.Single(tokens);
    Assert.Equal(RawKind.Error, tokens[0].Kind);

    var diagnostics = LexWithDiagnostics(input);
    Assert.Single(diagnostics);
    // Assuming InvalidChar diagnostic exists
  }

  #endregion

  #region Real-World Integration Tests

  [Fact]
  public void ComplexUnicodeExpression_LexesCorrectly()
  {
    string input = "变量名 := true && café <= العربية";
    var tokens = LexWithText(input);

    Assert.Equal(13, tokens.Count);  // Updated to match actual output
    Assert.Equal((RawKind.Identifier, "变量名"), tokens[0]);
    Assert.Equal((RawKind.WhitespaceTrivia, " "), tokens[1]);
    Assert.Equal((RawKind.ColonEqual, ":="), tokens[2]);
    Assert.Equal((RawKind.WhitespaceTrivia, " "), tokens[3]);
    Assert.Equal((RawKind.Identifier, "true"), tokens[4]);
    Assert.Equal((RawKind.WhitespaceTrivia, " "), tokens[5]);
    Assert.Equal((RawKind.AmpersandAmpersand, "&&"), tokens[6]);
    Assert.Equal((RawKind.WhitespaceTrivia, " "), tokens[7]);
    Assert.Equal((RawKind.Identifier, "café"), tokens[8]);
    Assert.Equal((RawKind.WhitespaceTrivia, " "), tokens[9]);
    Assert.Equal((RawKind.LessEqual, "<="), tokens[10]);
    Assert.Equal((RawKind.WhitespaceTrivia, " "), tokens[11]);
    Assert.Equal((RawKind.Identifier, "العربية"), tokens[12]);
  }

  [Fact]
  public void ModuleDeclaration_LexesCorrectly()
  {
    string input = "[[unicode::test]]";
    var tokens = LexWithText(input);

    Assert.Equal(5, tokens.Count);
    Assert.Equal((RawKind.LBracketLBracket, "[["), tokens[0]);
    Assert.Equal((RawKind.Identifier, "unicode"), tokens[1]);
    Assert.Equal((RawKind.ColonColon, "::"), tokens[2]);
    Assert.Equal((RawKind.Identifier, "test"), tokens[3]);
    Assert.Equal((RawKind.RBracketRBracket, "]]"), tokens[4]);
  }

  [Fact]
  public void TypicalBrimCodeSnippet_LexesCorrectly()
  {
    string input = @"x ::= 42u32
y := x >= 10 && x != 0
result .{ success: true, value: x }";

    var tokens = LexWithText(input);

    // Verify key tokens are present and correctly lexed
    var tokenKinds = tokens.Select(t => t.Kind).ToList();

    Assert.Contains(RawKind.Identifier, tokenKinds);
    Assert.Contains(RawKind.ColonColonEqual, tokenKinds);
    Assert.Contains(RawKind.IntegerLiteral, tokenKinds);
    Assert.Contains(RawKind.ColonEqual, tokenKinds);
    Assert.Contains(RawKind.GreaterEqual, tokenKinds);
    Assert.Contains(RawKind.AmpersandAmpersand, tokenKinds);
    Assert.Contains(RawKind.BangEqual, tokenKinds);
    Assert.Contains(RawKind.StopLBrace, tokenKinds);
    Assert.Contains(RawKind.Identifier, tokenKinds);
    Assert.Contains(RawKind.Terminator, tokenKinds);
  }

  [Fact]
  public void MixedQuoteLiterals_LexCorrectly()
  {
    string input = "'a' \"string\" 'π' \"🚀\" '🚀'";
    var tokens = LexWithText(input);

    var literalTokens = tokens.Where(t => t.Kind is RawKind.RuneLiteral or RawKind.StringLiteral).ToList();
    Assert.Equal(5, literalTokens.Count);
    Assert.Equal((RawKind.RuneLiteral, "'a'"), literalTokens[0]);
    Assert.Equal((RawKind.StringLiteral, "\"string\""), literalTokens[1]);
    Assert.Equal((RawKind.RuneLiteral, "'π'"), literalTokens[2]);
    Assert.Equal((RawKind.StringLiteral, "\"🚀\""), literalTokens[3]);
    Assert.Equal((RawKind.RuneLiteral, "'🚀'"), literalTokens[4]); // Now works!
  }

  [Fact]
  public void AllOperatorTokens_CanBeLexed()
  {
    // Test all compound operators in one go
    string input = "::= :: :> := && || == != <= >= >> << [[ ]] !{ !!{ ?{ #{ #( .{ @{ @( *{ |{ |( %{ %( &{ &( => ~= .. ??";
    var tokens = LexWithText(input);

    var operatorTokens = tokens.Where(t => t.Kind != RawKind.WhitespaceTrivia).ToList();

    // Verify we get all the compound operators
    var expectedOperators = new[]
    {
      RawKind.ColonColonEqual, RawKind.ColonColon, RawKind.ColonGreater, RawKind.ColonEqual,
      RawKind.AmpersandAmpersand, RawKind.PipePipe, RawKind.EqualEqual, RawKind.BangEqual,
      RawKind.LessEqual, RawKind.GreaterEqual, RawKind.GreaterGreater, RawKind.LessLess,
      RawKind.LBracketLBracket, RawKind.RBracketRBracket, RawKind.BangLBrace, RawKind.BangBangLBrace,
      RawKind.QuestionLBrace, RawKind.HashLBrace, RawKind.HashLParen, RawKind.StopLBrace, RawKind.AtmarkLBrace,
      RawKind.AtmarkLParen, RawKind.StarLBrace, RawKind.PipeLBrace, RawKind.PipeLParen, RawKind.PercentLBrace,
      RawKind.PercentLParen, RawKind.AmpersandLBrace, RawKind.AmpersandLParen, RawKind.EqualGreater,
      RawKind.TildeEqual, RawKind.StopStop, RawKind.QuestionQuestion
    };

    Assert.Equal(expectedOperators.Length, operatorTokens.Count);
    for (int i = 0; i < expectedOperators.Length; i++)
    {
      Assert.Equal(expectedOperators[i], operatorTokens[i].Kind);
    }
  }

  #endregion
}
