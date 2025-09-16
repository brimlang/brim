using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record StructShape(
  GreenToken Open,
  StructuralArray<FieldDeclaration> Fields,
  GreenToken Close)
  : GreenNode(SyntaxKind.StructShape, Open.Offset)
{
  public override int FullWidth => Close.EndOffset - Open.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    foreach (FieldDeclaration f in Fields) yield return f;
    yield return Close;
  }

  public static StructShape Parse(Parser p)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.StructToken);
    ImmutableArray<FieldDeclaration>.Builder fields = ImmutableArray.CreateBuilder<FieldDeclaration>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        FieldDeclaration field = FieldDeclaration.Parse(p);
        fields.Add(field);
        if (field.TrailingComma is null) break;
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break;
        if (p.Current.CoreToken.Offset == before) break; // progress guard
      }
    }

    StructuralArray<FieldDeclaration> arr = [.. fields];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new StructShape(open, arr, close);
  }
}

