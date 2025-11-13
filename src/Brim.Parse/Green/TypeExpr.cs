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
    switch (p.Current.TokenKind)
    {
      // Aggregate shapes
      case TokenKind.PercentLBrace:
        core = StructShape.Parse(p);
        break;
      case TokenKind.PipeLBrace:
        core = UnionShape.Parse(p);
        break;
      case TokenKind.HashLBrace:
        core = NamedTupleShape.Parse(p);
        break;
      case TokenKind.StopLBrace:
        core = ProtocolShape.Parse(p);
        break;
      case TokenKind.AtmarkLBrace:
        core = ServiceShape.Parse(p);
        break;
      case TokenKind.AmpersandLBrace:
        core = FlagsShape.Parse(p);
        break;

      // Function type
      case TokenKind.LParen:
        core = new FunctionTypeExpr(FunctionShape.Parse(p));
        break;

      default:
        // TypeRef (identifier or keyword)
        GreenNode maybeRef = TypeRef.Parse(p);
        core = maybeRef;
        break;
    }

    // Parse optional TypeSuffix
    GreenToken? suffix = p.Current.TokenKind switch
    {
      TokenKind.Question => p.Expect(SyntaxKind.QuestionToken),
      TokenKind.Bang => p.Expect(SyntaxKind.BangToken),
      _ => null
    };

    return new TypeExpr(core, suffix);
  }

  public override string ToString() => $"{Core.SyntaxKind}{Suffix?.ToString() ?? ""}";
}
