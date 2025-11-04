using System.Text;

namespace Brim.Parse.Green;

public sealed record ExportList(
  CommaList<GreenToken> List
) : GreenNode(SyntaxKind.ExportList, List.Offset),
  IParsable<ExportList>
{
  public override int FullWidth => List.EndOffset - List.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return List;
  }

  public string GetIdentifiersText(ReadOnlySpan<char> source)
  {
    StringBuilder sb = new();
    foreach (CommaList<GreenToken>.Element element in List.Elements)
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
