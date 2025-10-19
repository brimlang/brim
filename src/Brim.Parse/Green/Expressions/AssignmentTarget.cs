namespace Brim.Parse.Green;

public sealed record AssignmentTarget(
  GreenToken? Mutator,
  QualifiedIdent Identifier)
  : GreenNode(SyntaxKind.AssignmentTarget, Mutator is not null ? Mutator.Offset : Identifier.Offset)
{
  public override int FullWidth => Identifier.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    if (Mutator is not null) yield return Mutator;
    yield return Identifier;
  }

  public static AssignmentTarget Parse(Parser p)
  {
    GreenToken? mutator = null;
    if (p.MatchRaw(RawKind.Hat))
      mutator = new GreenToken(SyntaxKind.HatToken, p.ExpectRaw(RawKind.Hat));

    QualifiedIdent ident = QualifiedIdent.Parse(p);
    return new AssignmentTarget(mutator, ident);
  }
}
