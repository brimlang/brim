namespace Brim.Core;

public enum DiagCode
{
  UnexpectedToken,
  MissingToken,
  UnterminatedString,
  UnterminatedRune,
  InvalidCharacter,
  InvalidUtf8Encoding,
  InvalidStringUtf8,
  InvalidUnicodeEscape,
  UnknownEscapeSequence,
  InvalidRuneScalar,
  MultipleRunesInLiteral,
  InvalidIdentifierStart,
  InvalidIdentifierChar,
  UnsupportedWhitespace,
  EmptyGenericParamList,
  UnexpectedGenericBody,
  EmptyGenericArgList,
  MissingModuleHeader,
  TooManyErrors,
  InvalidGenericConstraint,
  UnsupportedModuleMember,
}

public enum DiagPhase : byte { Lex = 0, Parse = 1, Semantic = 2 }

public enum DiagSeverity : byte { Error = 0, Warning = 1, Info = 2 }

public readonly struct Diagnostic(
  DiagCode code,
  int offset,
  int length,
  int line,
  int column,
  TokenKind actualKind,
  TokenKind expect0,
  TokenKind expect1,
  TokenKind expect2,
  TokenKind expect3,
  byte expectedCount,
  uint extra,
  DiagPhase phase,
  DiagSeverity severity)
{
  public DiagCode Code { get; } = code;
  public int Offset { get; } = offset;
  public ushort Length { get; } = (ushort)length;
  public ushort Line { get; } = (ushort)line;
  public ushort Column { get; } = (ushort)column;
  public TokenKind ActualKind { get; } = actualKind;
  public byte ExpectedCount { get; } = expectedCount;
  public TokenKind Expect0 { get; } = expect0;
  public TokenKind Expect1 { get; } = expect1;
  public TokenKind Expect2 { get; } = expect2;
  public TokenKind Expect3 { get; } = expect3;
  public uint Extra { get; } = extra;
  public DiagPhase Phase { get; } = phase;
  public DiagSeverity Severity { get; } = severity;

  public Diagnostic(
    DiagCode code,
    int offset,
    int length,
    int line,
    int column,
    DiagPhase phase = DiagPhase.Parse,
    DiagSeverity severity = DiagSeverity.Error,
    TokenKind actualKind = default,
    uint extra = 0)
    : this(code, offset, length, line, column, actualKind, default, default, default, default, 0, extra, phase, severity)
  {
  }

  public static class Lex
  {
    public static Diagnostic InvalidChar(int offset, int line, int column, uint codePoint) =>
      new(DiagCode.InvalidCharacter, offset, 1, line, column, DiagPhase.Lex, DiagSeverity.Error, extra: codePoint);

    public static Diagnostic UnterminatedString(int offset, int length, int line, int column) =>
      new(DiagCode.UnterminatedString, offset, length, line, column, DiagPhase.Lex, DiagSeverity.Error, TokenKind.StringLiteral);

    public static Diagnostic UnterminatedRune(int offset, int length, int line, int column) =>
      new(DiagCode.UnterminatedRune, offset, length, line, column, DiagPhase.Lex, DiagSeverity.Error, TokenKind.RuneLiteral);

    public static Diagnostic InvalidUtf8Encoding(int offset, int line, int column) =>
      new(DiagCode.InvalidUtf8Encoding, offset, 1, line, column, DiagPhase.Lex);

    public static Diagnostic InvalidStringUtf8(int offset, int length, int line, int column) =>
      new(DiagCode.InvalidStringUtf8, offset, length, line, column, DiagPhase.Lex, DiagSeverity.Error, TokenKind.StringLiteral);

    public static Diagnostic InvalidUnicodeEscape(int offset, int line, int column, int length, uint codePoint) =>
      new(DiagCode.InvalidUnicodeEscape, offset, length, line, column, DiagPhase.Lex, DiagSeverity.Error, extra: codePoint);

    public static Diagnostic UnknownEscapeSequence(int offset, int line, int column, int length) =>
      new(DiagCode.UnknownEscapeSequence, offset, length, line, column, DiagPhase.Lex);

    public static Diagnostic InvalidRuneScalar(int offset, int length, int line, int column) =>
      new(DiagCode.InvalidRuneScalar, offset, length, line, column, DiagPhase.Lex, DiagSeverity.Error, TokenKind.RuneLiteral);

    public static Diagnostic MultipleRunesInLiteral(int offset, int length, int line, int column) =>
      new(DiagCode.MultipleRunesInLiteral, offset, length, line, column, DiagPhase.Lex, DiagSeverity.Error, TokenKind.RuneLiteral);

    public static Diagnostic InvalidIdentifierStart(int offset, int line, int column, uint codePoint) =>
      new(DiagCode.InvalidIdentifierStart, offset, 1, line, column, DiagPhase.Lex, DiagSeverity.Error, extra: codePoint);

    public static Diagnostic InvalidIdentifierChar(int offset, int line, int column, int length, uint codePoint) =>
      new(DiagCode.InvalidIdentifierChar, offset, length, line, column, DiagPhase.Lex, DiagSeverity.Error, extra: codePoint);

    public static Diagnostic UnsupportedWhitespace(int offset, int line, int column, uint codePoint) =>
      new(DiagCode.UnsupportedWhitespace, offset, 1, line, column, DiagPhase.Lex, DiagSeverity.Error, extra: codePoint);
  }

  public static class Parse
  {
    public static Diagnostic Missing<T>(TokenKind expectedKind, T tok) where T : IToken =>
      new(DiagCode.MissingToken, tok.Offset, tok.Length, tok.Line, tok.Column, default, expectedKind, default, default, default, 1, 0, DiagPhase.Parse, DiagSeverity.Error);

    public static Diagnostic EmptyGenericParamList<T>(T tok) where T : IToken =>
      new(DiagCode.EmptyGenericParamList, tok.Offset, tok.Length, tok.Line, tok.Column);

    public static Diagnostic UnexpectedGenericBody<T>(T tok) where T : IToken =>
      new(DiagCode.UnexpectedGenericBody, tok.Offset, tok.Length, tok.Line, tok.Column, actualKind: tok.TokenKind);

    public static Diagnostic EmptyGenericArgList<T>(T tok) where T : IToken =>
      new(DiagCode.EmptyGenericArgList, tok.Offset, tok.Length, tok.Line, tok.Column);

    public static Diagnostic MissingModuleHeader<T>(T tok) where T : IToken =>
      new(DiagCode.MissingModuleHeader, tok.Offset, tok.Length, tok.Line, tok.Column);

    public static Diagnostic TooManyErrors(int offset, int line, int column) =>
      new(DiagCode.TooManyErrors, offset, 0, line, column);

    public static Diagnostic InvalidGenericConstraint<T>(T tok) where T : IToken =>
      new(DiagCode.InvalidGenericConstraint, tok.Offset, tok.Length, tok.Line, tok.Column, actualKind: tok.TokenKind);

    public static Diagnostic UnsupportedModuleMember<T>(T tok) where T : IToken =>
      new(DiagCode.UnsupportedModuleMember, tok.Offset, tok.Length, tok.Line, tok.Column, actualKind: tok.TokenKind);

    public static Diagnostic Unexpected<T>(
      T tok,
      ReadOnlySpan<TokenKind> expected) where T : IToken
    {
      byte count = (byte)Math.Min(expected.Length, 4);
      TokenKind e0 = default, e1 = default, e2 = default, e3 = default;
      if (count > 0) e0 = expected[0];
      if (count > 1) e1 = expected[1];
      if (count > 2) e2 = expected[2];
      if (count > 3) e3 = expected[3];
      return new Diagnostic(
        code: DiagCode.UnexpectedToken,
        offset: tok.Offset,
        length: tok.Length,
        line: tok.Line,
        column: tok.Column,
        actualKind: tok.TokenKind,
        expect0: e0,
        expect1: e1,
        expect2: e2,
        expect3: e3,
        expectedCount: count,
        extra: 0,
        phase: DiagPhase.Parse,
        severity: DiagSeverity.Error);
    }
  }
}
