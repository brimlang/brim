namespace Brim.Parse.Green;

public sealed record TypeExpr(
  GreenNode Core,
  GreenToken? Suffix
) : GreenNode(SyntaxKind.TypeExpr, Core.Offset)
{
  public override int FullWidth => (Suffix?.EndOffset ?? Core.EndOffset) - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Core;
    if (Suffix is not null) yield return Suffix;
  }

  public static TypeExpr Parse(Parser p)
  {
    // Parse TypeCore
    GreenNode core;
    switch (p.Current.Kind)
    {
      // Aggregate shapes
      case RawKind.PercentLBrace:
        core = StructShape.Parse(p);
        break;
      case RawKind.PipeLBrace:
        core = UnionShape.Parse(p);
        break;
      case RawKind.HashLBrace:
        core = NamedTupleShape.Parse(p);
        break;
      case RawKind.StopLBrace:
        core = ProtocolShape.Parse(p);
        break;
      case RawKind.AtmarkLBrace:
        core = ServiceShape.Parse(p);
        break;
      case RawKind.AmpersandLBrace:
        core = FlagsShape.Parse(p);
        break;

      // Function type
      case RawKind.LParen:
        core = new FunctionTypeExpr(FunctionShape.Parse(p));
        break;

      default:
        // TypeRef (identifier or keyword)
        GreenNode maybeRef = TypeRef.Parse(p);
        core = maybeRef;
        break;
    }

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
