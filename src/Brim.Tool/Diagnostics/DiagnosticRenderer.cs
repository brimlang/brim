using System.Text;
using Brim.Core;
using Brim.Parse;

namespace Brim.Tool.Diagnostics;

public static class DiagnosticRenderer
{
  public static string Render(in Diagnostic d)
  {
    string core = d.Code switch
    {
      DiagCode.UnexpectedToken => RenderUnexpected(d),
      DiagCode.MissingToken => RenderMissing(d),
      DiagCode.UnterminatedString => "unterminated string literal",
      DiagCode.InvalidCharacter => RenderInvalidChar(d),
      DiagCode.EmptyGenericParamList => "empty generic parameter list",
      DiagCode.UnexpectedGenericBody => "unexpected generic declaration body",
      DiagCode.EmptyGenericArgList => "empty generic argument list",
      DiagCode.MissingModuleHeader => "missing required module header",
      DiagCode.TooManyErrors => "too many errors; further diagnostics suppressed",
      _ => d.Code.ToString()
    };
    return $"{d.Severity.ToString().ToLower()}[{d.Phase.ToString().ToLower()}]: {core}";
  }

  static string KindName(TokenKind kind) => kind == default ? "<none>" : kind.ToString();

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
        _ = sb.Append(KindName(i switch { 0 => d.Expect0, 1 => d.Expect1, 2 => d.Expect2, 3 => d.Expect3, _ => default }));
      }
    }
    return sb.ToString();
  }

  static string RenderMissing(in Diagnostic d)
    => $"missing token {KindName(d.Expect0)}";

  static string RenderInvalidChar(in Diagnostic d) =>
    $"invalid character '{(char.IsControl((char)d.Extra) ? ' ' : (char)d.Extra)}' (U+{(int)(char)d.Extra:X4})";
}
