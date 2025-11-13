namespace Brim.Parse.Green;

public sealed record LambdaParams(
  int OffsetValue,
  StructuralArray<LambdaParams.Parameter> Parameters)
  : GreenNode(SyntaxKind.LambdaParams, OffsetValue)
{
  public override int FullWidth => Parameters.Length > 0
    ? Parameters[^1].EndOffset - Offset
    : 0;

  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (Parameter parameter in Parameters)
      yield return parameter;
  }

  public static LambdaParams From(ArrayBuilder<Parameter> builder, int offset)
    => new(offset, builder.ToImmutable());

  public static LambdaParams Empty(int offset)
    => new(offset, StructuralArray<Parameter>.Empty);

  public sealed record Parameter(
    GreenToken? LeadingComma,
    GreenToken Identifier)
    : GreenNode(SyntaxKind.ListElement, (LeadingComma ?? Identifier).Offset)
  {
    public override int FullWidth => Identifier.EndOffset - Offset;

    public override IEnumerable<GreenNode> GetChildren()
    {
      if (LeadingComma is not null)
        yield return LeadingComma;

      yield return Identifier;
    }
  }
}
