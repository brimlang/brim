namespace Brim.Parse;

public static class Tokens
{
  public static bool IsEob(in TokenView token) => token.Kind == RawKind.Eob;
  public static bool IsDefault(in TokenView token) => token.Kind == RawKind.Default;
  public static bool IsError(in TokenView token) => token.Kind == RawKind.Error;

  public static TokenView AsTokenView(in TokenView token) => token;
}


