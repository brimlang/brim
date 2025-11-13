using Brim.Core;
using Brim.Core.Collections;

namespace Brim.Lex.Tests;

public class LexTokenSourceTests
{
  [Fact]
  public void LexesIdentifiersWhitespaceAndTerminators()
  {
    LexTestHost.LexResult result = LexTestHost.Lex("foo  \nbar;");

    TokenKind[] kinds = [.. result.Tokens.Select(t => t.TokenKind)];
    Assert.Equal([
      TokenKind.Identifier,
      TokenKind.WhitespaceTrivia,
      TokenKind.Terminator,
      TokenKind.Identifier,
      TokenKind.Terminator,
      TokenKind.Eob], kinds);

    Assert.Equal("foo", result.Slice(result.Tokens[0]));
    Assert.Equal("  ", result.Slice(result.Tokens[1]));
    Assert.Equal("\n", result.Slice(result.Tokens[2]));
    Assert.Equal("bar", result.Slice(result.Tokens[3]));
    Assert.Equal(";", result.Slice(result.Tokens[4]));
    Assert.Empty(result.Diagnostics);
  }

  [Fact]
  public void ReportsUnsupportedWhitespaceAndContinues()
  {
    LexTestHost.LexResult result = LexTestHost.Lex("foo\u000Bbar");

    TokenKind[] kinds = [.. result.Tokens.Select(t => t.TokenKind)];
    Assert.Equal([TokenKind.Identifier, TokenKind.Identifier, TokenKind.Eob], kinds);

    Diagnostic diagnostic = Assert.Single(result.Diagnostics);
    Assert.Equal(DiagCode.UnsupportedWhitespace, diagnostic.Code);
    Assert.Equal(3, diagnostic.Offset);
  }

  [Fact]
  public void UnterminatedStringProducesDiagnostic()
  {
    LexTestHost.LexResult result = LexTestHost.Lex("\"unterminated");

    LexToken literal = Assert.Single(result.Tokens.Where(t => t.TokenKind == TokenKind.StringLiteral));
    Assert.Equal("\"unterminated", result.Slice(literal));

    Diagnostic diagnostic = Assert.Single(result.Diagnostics);
    Assert.Equal(DiagCode.UnterminatedString, diagnostic.Code);
    Assert.Equal(literal.Offset, diagnostic.Offset);
    Assert.Equal(literal.Length, diagnostic.Length);
  }

  [Fact]
  public void LexesNumericVariantsAndSuffixes()
  {
    const string sample = "42 1.5 0xFF 0b1010 99u16 3i64 1.0f32";
    LexTestHost.LexResult result = LexTestHost.Lex(sample);

    LexToken[] literals = [.. result.Tokens.Where(t => t.TokenKind is TokenKind.IntegerLiteral or TokenKind.DecimalLiteral)];

    Assert.Equal([
      TokenKind.IntegerLiteral,
      TokenKind.DecimalLiteral,
      TokenKind.IntegerLiteral,
      TokenKind.IntegerLiteral,
      TokenKind.IntegerLiteral,
      TokenKind.IntegerLiteral,
      TokenKind.DecimalLiteral], literals.Select(l => l.TokenKind));

    Assert.Equal([
      "42",
      "1.5",
      "0xFF",
      "0b1010",
      "99u16",
      "3i64",
      "1.0f32"], literals.Select(result.Slice));
    Assert.Empty(result.Diagnostics);
  }

  [Fact]
  public void LexesLineCommentsAndTerminators()
  {
    LexTestHost.LexResult result = LexTestHost.Lex("foo-- hi\nbar");

    Assert.Equal([
      TokenKind.Identifier,
      TokenKind.CommentTrivia,
      TokenKind.Terminator,
      TokenKind.Identifier,
      TokenKind.Eob], result.Tokens.Select(t => t.TokenKind));

    Assert.Equal("-- hi", result.Slice(result.Tokens[1]));
    Assert.Equal("\n", result.Slice(result.Tokens[2]));
    Assert.Empty(result.Diagnostics);
  }

  [Fact]
  public void ValidRuneLiteralProducesNoDiagnostics()
  {
    LexTestHost.LexResult result = LexTestHost.Lex("'λ'");

    LexToken rune = Assert.Single(result.Tokens.Where(t => t.TokenKind == TokenKind.RuneLiteral));
    Assert.Equal("'λ'", result.Slice(rune));
    Assert.Empty(result.Diagnostics);
  }

  [Fact]
  public void MultipleRunesDiagnosed()
  {
    LexTestHost.LexResult result = LexTestHost.Lex("'ab'");

    Diagnostic diagnostic = Assert.Single(result.Diagnostics);
    Assert.Equal(DiagCode.MultipleRunesInLiteral, diagnostic.Code);
    Assert.Equal(0, diagnostic.Offset);
  }

  [Fact]
  public void UnterminatedRuneDiagnosed()
  {
    LexTestHost.LexResult result = LexTestHost.Lex("'a");

    Diagnostic diagnostic = Assert.Single(result.Diagnostics);
    Assert.Equal(DiagCode.UnterminatedRune, diagnostic.Code);
    Assert.Equal(0, diagnostic.Offset);
  }

  [Fact]
  public void InvalidCharacterProducesErrorToken()
  {
    LexTestHost.LexResult result = LexTestHost.Lex("\u0001");

    Assert.Equal([TokenKind.Error, TokenKind.Eob], result.Tokens.Select(t => t.TokenKind));

    Diagnostic diagnostic = Assert.Single(result.Diagnostics);
    Assert.Equal(DiagCode.InvalidCharacter, diagnostic.Code);
  }

  [Fact]
  public void TryReadReturnsFalseAfterEob()
  {
    SourceText source = SourceText.From("x");
    DiagnosticList diagnostics = DiagnosticList.Create();
    LexTokenSource lexer = new(source, diagnostics);

    Assert.True(lexer.TryRead(out LexToken identifier));
    Assert.Equal(TokenKind.Identifier, identifier.TokenKind);

    Assert.True(lexer.TryRead(out LexToken eob));
    Assert.Equal(TokenKind.Eob, eob.TokenKind);

    Assert.False(lexer.TryRead(out LexToken afterEob));
    Assert.Equal(TokenKind.Eob, afterEob.TokenKind);
  }
}
