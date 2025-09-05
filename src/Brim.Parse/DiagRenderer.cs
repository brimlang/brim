using System.Text;

namespace Brim.Parse;

public static class DiagRenderer
{
  public static string Render(in Diag d)
  {
    return d.Code switch
    {
      DiagCode.UnexpectedToken => RenderUnexpected(d),
      DiagCode.MissingToken => RenderMissing(d),
      DiagCode.UnterminatedString => "unterminated string literal",
      DiagCode.InvalidCharacter => RenderInvalidChar(d),
      _ => d.Code.ToString()
    };
  }

  static string KindName(ushort kind) => kind == 0 ? "<none>" : ((RawTokenKind)kind).ToString();

  static string RenderUnexpected(in Diag d)
  {
    StringBuilder sb = new();
    sb.Append("unexpected token ").Append(KindName(d.ActualKind));
    if (d.ExpectedCount > 0)
    {
      sb.Append(", expected ");
      for (int i = 0; i < d.ExpectedCount; i++)
      {
        if (i > 0) sb.Append(" | ");
        sb.Append(KindName(i switch { 0 => d.Expect0, 1 => d.Expect1, 2 => d.Expect2, 3 => d.Expect3, _ => (ushort)0 }));
      }
    }
    return sb.ToString();
  }

  static string RenderMissing(in Diag d)
    => $"missing token {KindName(d.Expect0)}";

  static string RenderInvalidChar(in Diag d) =>
    $"invalid character '{(char.IsControl((char)d.Extra) ? ' ' : (char)d.Extra)}' (U+{((int)(char)d.Extra):X4})";
}
