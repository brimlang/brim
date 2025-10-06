using System.Text;

namespace Brim.Parse.Green;

public sealed record ExportList(
  CommaList List
) : GreenNode(SyntaxKind.ExportList, List.OpenToken.Offset),
  IParsable<ExportList>
{
  public override int FullWidth => List.CloseToken.EndOffset - List.OpenToken.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return List.OpenToken;
    if (List.LeadingTerminator is not null) yield return List.LeadingTerminator;
    foreach (CommaList.Element element in List.Elements) yield return element;
    if (List.TrailingComma is not null) yield return List.TrailingComma;
    if (List.TrailingTerminator is not null) yield return List.TrailingTerminator;
    yield return List.CloseToken;
  }

  public string GetIdentifiersText(ReadOnlySpan<char> source)
  {
    StringBuilder sb = new();
    foreach (CommaList.Element element in List.Elements)
    {
      if (element.Node is not GreenToken gt || gt.Kind != SyntaxKind.IdentifierToken)
        continue;
      if (sb.Length > 0) sb.Append(", ");
      sb.Append(gt.GetText(source));
    }

    return sb.ToString();
  }

  public static ExportList Parse(Parser p) =>
    new(CommaList.Parse(p, SyntaxKind.ExportOpenToken, SyntaxKind.ExportEndToken, SyntaxKind.IdentifierToken));
}

