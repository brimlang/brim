namespace Brim.Parse;

public enum DiagCode
{
  UnexpectedToken,
  MissingToken,
  UnterminatedString,
  InvalidCharacter,
  EmptyGenericParamList,
  UnexpectedGenericBody,
  EmptyGenericArgList,
}

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
  uint extra = 0)
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
}

static class DiagFactory
{
  public static Diagnostic Unexpected(in RawToken actual, ReadOnlySpan<RawTokenKind> expected)
  {
    byte count = (byte)Math.Min(expected.Length, 4);
    ushort e0 = 0, e1 = 0, e2 = 0, e3 = 0;
    if (count > 0) e0 = (ushort)expected[0];
    if (count > 1) e1 = (ushort)expected[1];
    if (count > 2) e2 = (ushort)expected[2];
    if (count > 3) e3 = (ushort)expected[3];
    return new Diagnostic(DiagCode.UnexpectedToken, actual.Offset, actual.Length, actual.Line, actual.Column, (ushort)actual.Kind, count, e0, e1, e2, e3);
  }

  public static Diagnostic Missing(RawTokenKind expectedKind, in RawToken lookahead) =>
    new(DiagCode.MissingToken, lookahead.Offset, 0, lookahead.Line, lookahead.Column, 0, 1, (ushort)expectedKind);

  public static Diagnostic InvalidChar(int offset, int line, int column, char ch) =>
    new(DiagCode.InvalidCharacter, offset, 1, line, column, extra: ch);

  public static Diagnostic UnterminatedString(in RawToken tok) =>
    new(DiagCode.UnterminatedString, tok.Offset, tok.Length, tok.Line, tok.Column, (ushort)tok.Kind);

  public static Diagnostic EmptyGenericParamList(in RawToken open) =>
    new(DiagCode.EmptyGenericParamList, open.Offset, open.Length, open.Line, open.Column);

  public static Diagnostic UnexpectedGenericBody(in RawToken look) =>
    new(DiagCode.UnexpectedGenericBody, look.Offset, look.Length, look.Line, look.Column, (ushort)look.Kind);

  public static Diagnostic EmptyGenericArgList(in RawToken open) =>
    new(DiagCode.EmptyGenericArgList, open.Offset, open.Length, open.Line, open.Column);
}
