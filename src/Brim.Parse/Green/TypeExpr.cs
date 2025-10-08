namespace Brim.Parse.Green;

public sealed record TypeExpr(
  GreenNode Core,
  GreenToken? Suffix
) : GreenNode(SyntaxKind.TypeExpr, Core.Offset)
{
  public override int FullWidth => (Suffix?.EndOffset ?? Core.EndOffset) - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (GreenNode child in Core.GetChildren())
      yield return child;
    if (Suffix is not null) yield return Suffix;
  }

  public static TypeExpr Parse(Parser p)
  {
    // Parse TypeCore
    GreenNode core = p.Current.Kind switch
    {
      // Aggregate shapes
      RawKind.PercentLBrace => StructShape.Parse(p),
      RawKind.PipeLBrace => UnionShape.Parse(p),
      RawKind.HashLBrace => NamedTupleShape.Parse(p),
      RawKind.StopLBrace => ProtocolShape.Parse(p),
      RawKind.AtmarkLBrace => ServiceShape.Parse(p),
      RawKind.AmpersandLBrace => FlagsShape.Parse(p),

      // Function type
      RawKind.LParen => new FunctionTypeExpr(FunctionShape.Parse(p)),

      // TypeRef (identifier or keyword)
      _ => TypeRef.Parse(p)
    };

    // Parse optional TypeSuffix
    GreenToken? suffix = p.Current.Kind switch
    {
      RawKind.Question => p.ExpectSyntax(SyntaxKind.QuestionToken),
      RawKind.Bang => p.ExpectSyntax(SyntaxKind.BangToken),
      _ => null
    };

    return new TypeExpr(core, suffix);
  }
}
