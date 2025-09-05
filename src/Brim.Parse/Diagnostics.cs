namespace Brim.Parse;

public enum DiagCode
{
  UnexpectedToken,
  MissingToken,
  UnterminatedString,
  InvalidCharacter,
}

public readonly struct Diag
{
  public readonly DiagCode Code;
  public readonly int Offset;
  public readonly ushort Length;
  public readonly ushort Line;
  public readonly ushort Column;
  public readonly ushort ActualKind;      // RawTokenKind (cast)
  public readonly byte ExpectedCount;     // 0..4
  public readonly ushort Expect0;
  public readonly ushort Expect1;
  public readonly ushort Expect2;
  public readonly ushort Expect3;
  public readonly uint Extra;             // variant payload (char code, etc.)

  public Diag(
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
    Code = code;
    Offset = offset;
    Length = (ushort)length;
    Line = (ushort)line;
    Column = (ushort)column;
    ActualKind = actualKind;
    ExpectedCount = expectedCount;
    Expect0 = e0; Expect1 = e1; Expect2 = e2; Expect3 = e3;
    Extra = extra;
  }
}

static class DiagFactory
{
  public static Diag Unexpected(in RawToken actual, ReadOnlySpan<RawTokenKind> expected)
  {
    byte count = (byte)Math.Min(expected.Length, 4);
    ushort e0 = 0, e1 = 0, e2 = 0, e3 = 0;
    if (count > 0) e0 = (ushort)expected[0];
    if (count > 1) e1 = (ushort)expected[1];
    if (count > 2) e2 = (ushort)expected[2];
    if (count > 3) e3 = (ushort)expected[3];
    return new Diag(DiagCode.UnexpectedToken, actual.Offset, actual.Length, actual.Line, actual.Column, (ushort)actual.Kind, count, e0, e1, e2, e3);
  }

  public static Diag Missing(RawTokenKind expectedKind, in RawToken lookahead) =>
    new(DiagCode.MissingToken, lookahead.Offset, 0, lookahead.Line, lookahead.Column, 0, 1, (ushort)expectedKind);

  public static Diag InvalidChar(int offset, int line, int column, char ch) =>
    new(DiagCode.InvalidCharacter, offset, 1, line, column, extra: ch);

  public static Diag UnterminatedString(in RawToken tok) =>
    new(DiagCode.UnterminatedString, tok.Offset, tok.Length, tok.Line, tok.Column, (ushort)tok.Kind);
}