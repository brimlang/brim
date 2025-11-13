namespace Brim.Parse.Green;

public sealed record MatchArmList(StructuralArray<MatchArm> Arms)
  : GreenNode(SyntaxKind.MatchArmList, Arms.Length > 0 ? Arms[0].Offset : 0)
{
  public override int FullWidth {
    get
    {
      if (Arms.Length == 0)
        return 0;
      MatchArm last = Arms[^1];
      return last.EndOffset - Offset;
    }
  }

  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (MatchArm arm in Arms)
      yield return arm;
  }
}
