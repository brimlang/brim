namespace Brim.Parse;

/// <summary>
/// A significant token plus its attached leading trivia. All trivia is modeled as leading trivia on the
/// following token (or EOF). There is no trailing trivia concept.
/// </summary>
/// <param name="CoreToken">The significant or boundary token (includes Terminator and Eob).</param>
/// <param name="LeadingTrivia">Trivia immediately preceding; may be empty.</param>
public readonly record struct SignificantToken(
  RawToken CoreToken,
  StructuralArray<RawToken> LeadingTrivia)
{
  public bool HasLeading => LeadingTrivia.Count > 0;
  public RawKind Kind => CoreToken.Kind;
}
