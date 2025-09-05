using System.Text;

namespace Brim.Parse;

public static class DiagRenderer
{
  public static string Render(in Diagnostic d)
  {
    return d.Code switch
    {
      DiagCode.UnexpectedToken => RenderUnexpected(d),
      DiagCode.MissingToken => RenderMissing(d),
      DiagCode.UnterminatedString => "unterminated string literal",
      DiagCode.InvalidCharacter => RenderInvalidChar(d),
      DiagCode.EmptyGenericParamList => "empty generic parameter list",
      DiagCode.UnexpectedGenericBody => "unexpected generic declaration body",
      DiagCode.EmptyGenericArgList => "empty generic argument list",
      _ => d.Code.ToString()
    };
  }

  static string KindName(ushort kind) => kind == 0 ? "<none>" : ((RawTokenKind)kind).ToString();

  static string RenderUnexpected(in Diagnostic d)
  {
    StringBuilder sb = new();
    _ = sb.Append("unexpected token ").Append(KindName(d.ActualKind));
    if (d.ExpectedCount > 0)
    {
      _ = sb.Append(", expected ");
      for (int i = 0; i < d.ExpectedCount; i++)
      {
        if (i > 0) _ = sb.Append(" | ");
        _ = sb.Append(KindName(i switch { 0 => d.Expect0, 1 => d.Expect1, 2 => d.Expect2, 3 => d.Expect3, _ => 0 }));
      }
    }
    return sb.ToString();
  }

  static string RenderMissing(in Diagnostic d)
    => $"missing token {KindName(d.Expect0)}";

  static string RenderInvalidChar(in Diagnostic d) =>
    $"invalid character '{(char.IsControl((char)d.Extra) ? ' ' : (char)d.Extra)}' (U+{(int)(char)d.Extra:X4})";
}
