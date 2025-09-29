namespace Brim.Parse;

public enum DiagCode
{
  UnexpectedToken,
  MissingToken,
  UnterminatedString,
  UnterminatedRune,
  InvalidCharacter,

  // Unicode-specific diagnostics (UENC*, USTR*, URUN*, UIDENT*, ULEX*)
  InvalidUtf8Encoding,     // UENC001
  InvalidStringUtf8,       // USTR001  
  InvalidUnicodeEscape,    // USTR002
  UnknownEscapeSequence,   // USTR003
  InvalidRuneScalar,       // URUN001
  MultipleRunesInLiteral,  // URUN002
  InvalidIdentifierStart,  // UIDENT001
  InvalidIdentifierChar,   // UIDENT002
  UnsupportedWhitespace,   // ULEX001
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
  ushort actualKind = 0,
  byte expectedCount = 0,
  ushort e0 = 0,
  ushort e1 = 0,
  ushort e2 = 0,
  ushort e3 = 0,
  uint extra = 0,
  DiagPhase phase = DiagPhase.Parse,
  DiagSeverity severity = DiagSeverity.Error)
{
  public DiagCode Code { get; } = code;
  public int Offset { get; } = offset;
  public ushort Length { get; } = (ushort)length;
  public ushort Line { get; } = (ushort)line;
  public ushort Column { get; } = (ushort)column;
  public ushort ActualKind { get; } = actualKind;
  public byte ExpectedCount { get; } = expectedCount;
  public ushort Expect0 { get; } = e0;
  public ushort Expect1 { get; } = e1;
  public ushort Expect2 { get; } = e2;
  public ushort Expect3 { get; } = e3;
  public uint Extra { get; } = extra;
  public DiagPhase Phase { get; } = phase;
  public DiagSeverity Severity { get; } = severity;

  public static Diagnostic Unexpected(in RawToken actual, ReadOnlySpan<RawKind> expected)
  {
    byte count = (byte)Math.Min(expected.Length, 4);
    ushort e0 = 0, e1 = 0, e2 = 0, e3 = 0;
    if (count > 0) e0 = (ushort)expected[0];
    if (count > 1) e1 = (ushort)expected[1];
    if (count > 2) e2 = (ushort)expected[2];
    if (count > 3) e3 = (ushort)expected[3];
    return new Diagnostic(DiagCode.UnexpectedToken, actual.Offset, actual.Length, actual.Line, actual.Column, (ushort)actual.Kind, count, e0, e1, e2, e3, 0, DiagPhase.Parse, DiagSeverity.Error);
  }

  public static Diagnostic Missing(RawKind expectedKind, in RawToken lookahead) =>
    new(DiagCode.MissingToken, lookahead.Offset, 0, lookahead.Line, lookahead.Column, 0, 1, (ushort)expectedKind, 0, 0, 0, 0, DiagPhase.Parse, DiagSeverity.Error);

  public static Diagnostic InvalidChar(int offset, int line, int column, char ch) =>
    new(DiagCode.InvalidCharacter, offset, 1, line, column, 0, 0, 0, 0, 0, 0, ch, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic UnterminatedString(in RawToken tok) =>
    new(DiagCode.UnterminatedString, tok.Offset, tok.Length, tok.Line, tok.Column, (ushort)tok.Kind, 0, 0, 0, 0, 0, 0, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic UnterminatedRune(in RawToken tok) =>
    new(DiagCode.UnterminatedRune, tok.Offset, tok.Length, tok.Line, tok.Column, (ushort)tok.Kind, 0, 0, 0, 0, 0, 0, DiagPhase.Lex, DiagSeverity.Error);

  // Unicode-specific diagnostics
  public static Diagnostic InvalidUtf8Encoding(int offset, int line, int column) =>
    new(DiagCode.InvalidUtf8Encoding, offset, 1, line, column, 0, 0, 0, 0, 0, 0, 0, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic InvalidStringUtf8(in RawToken tok) =>
    new(DiagCode.InvalidStringUtf8, tok.Offset, tok.Length, tok.Line, tok.Column, (ushort)tok.Kind, 0, 0, 0, 0, 0, 0, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic InvalidUnicodeEscape(int offset, int line, int column, int length, uint codePoint) =>
    new(DiagCode.InvalidUnicodeEscape, offset, length, line, column, 0, 0, 0, 0, 0, 0, codePoint, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic UnknownEscapeSequence(int offset, int line, int column, int length) =>
    new(DiagCode.UnknownEscapeSequence, offset, length, line, column, 0, 0, 0, 0, 0, 0, 0, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic InvalidRuneScalar(in RawToken tok) =>
    new(DiagCode.InvalidRuneScalar, tok.Offset, tok.Length, tok.Line, tok.Column, (ushort)tok.Kind, 0, 0, 0, 0, 0, 0, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic MultipleRunesInLiteral(in RawToken tok) =>
    new(DiagCode.MultipleRunesInLiteral, tok.Offset, tok.Length, tok.Line, tok.Column, (ushort)tok.Kind, 0, 0, 0, 0, 0, 0, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic InvalidIdentifierStart(int offset, int line, int column, uint codePoint) =>
    new(DiagCode.InvalidIdentifierStart, offset, 1, line, column, 0, 0, 0, 0, 0, 0, codePoint, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic InvalidIdentifierChar(int offset, int line, int column, int length, uint codePoint) =>
    new(DiagCode.InvalidIdentifierChar, offset, length, line, column, 0, 0, 0, 0, 0, 0, codePoint, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic UnsupportedWhitespace(int offset, int line, int column, uint codePoint) =>
    new(DiagCode.UnsupportedWhitespace, offset, 1, line, column, 0, 0, 0, 0, 0, 0, codePoint, DiagPhase.Lex, DiagSeverity.Error);

  public static Diagnostic EmptyGenericParamList(in RawToken open) =>
    new(DiagCode.EmptyGenericParamList, open.Offset, open.Length, open.Line, open.Column, 0, 0, 0, 0, 0, 0, 0, DiagPhase.Parse, DiagSeverity.Error);

  public static Diagnostic UnexpectedGenericBody(in RawToken look) =>
    new(DiagCode.UnexpectedGenericBody, look.Offset, look.Length, look.Line, look.Column, (ushort)look.Kind, 0, 0, 0, 0, 0, 0, DiagPhase.Parse, DiagSeverity.Error);

  public static Diagnostic EmptyGenericArgList(in RawToken open) =>
    new(DiagCode.EmptyGenericArgList, open.Offset, open.Length, open.Line, open.Column, 0, 0, 0, 0, 0, 0, 0, DiagPhase.Parse, DiagSeverity.Error);

  public static Diagnostic MissingModuleHeader(in RawToken look) =>
    new(DiagCode.MissingModuleHeader, look.Offset, 0, look.Line, look.Column, 0, 0, 0, 0, 0, 0, 0, DiagPhase.Parse, DiagSeverity.Error);

  public static Diagnostic TooManyErrors(in RawToken look) =>
    new(DiagCode.TooManyErrors, look.Offset, 0, look.Line, look.Column, 0, 0, 0, 0, 0, 0, 0, DiagPhase.Parse, DiagSeverity.Error);

  public static Diagnostic InvalidGenericConstraint(in RawToken look) =>
    new(DiagCode.InvalidGenericConstraint, look.Offset, look.Length, look.Line, look.Column, (ushort)look.Kind, 0, 0, 0, 0, 0, 0, DiagPhase.Parse, DiagSeverity.Error);

  public static Diagnostic UnsupportedModuleMember(in RawToken look) =>
    new(DiagCode.UnsupportedModuleMember, look.Offset, look.Length, look.Line, look.Column, (ushort)look.Kind, 0, 0, 0, 0, 0, 0, DiagPhase.Parse, DiagSeverity.Error);
}
