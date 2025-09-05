namespace Brim.Parse;

/// <summary>
/// A significant token plus its attached leading and trailing trivia (always retained, never discarded).
/// </summary>
/// <param name="Token">The significant or boundary token (includes Terminator and Eof).</param>
/// <param name="LeadingTrivia">Trivia immediately preceding; may be empty.</param>
/// <param name="TrailingTrivia">Trivia immediately following up to (but not including) the next significant/terminator/EOF token.</param>
public readonly record struct SignificantToken(
  RawToken Token,
  StructuralArray<RawToken> LeadingTrivia,
  StructuralArray<RawToken> TrailingTrivia)
{
  public bool HasLeading => LeadingTrivia.Count > 0;
  public bool HasTrailing => TrailingTrivia.Count > 0;
}
