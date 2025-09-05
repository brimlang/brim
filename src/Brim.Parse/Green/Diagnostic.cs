using System.Collections.Immutable;

namespace Brim.Parse.Green;

public record struct Diagnostic(Diagnostic.CodeKind code, params ImmutableArray<object> args)
{
  public enum CodeKind
  {
    UnexpectedToken,
    MissingToken,
  }

  internal static Diagnostic UnexpectedToken(RawTokenKind found)
    => new(CodeKind.UnexpectedToken, found);

  internal static Diagnostic MissingToken(SyntaxKind expected)
    => new(CodeKind.MissingToken, expected);
}

