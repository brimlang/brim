namespace Brim.Parse.Tests;

public class ParserPredictionTests
{
  [Fact]
  public void ModuleMemberPredictions_Contain_AliasForm()
  {
    // Ensure there is a prediction for (Identifier, ColonEqual, Identifier)
    bool found = Parser.ModuleMemberPredictions.Any(p =>
    p.Sequence[0] == RawKind.Identifier &&
    p.Sequence[1] == RawKind.ColonEqual &&
    p.Sequence[2] == RawKind.Identifier);
    Assert.True(found);
  }

  [Fact]
  public void ModuleMemberPredictions_Contain_Protocol_And_Service()
  {
    bool proto = Parser.ModuleMemberPredictions.Any(p =>
      p.Sequence[0] == RawKind.Identifier && p.Sequence[1] == RawKind.Colon && p.Sequence[2] == RawKind.Stop);
    bool svc = Parser.ModuleMemberPredictions.Any(p =>
      p.Sequence[0] == RawKind.Identifier && p.Sequence[1] == RawKind.Colon && p.Sequence[2] == RawKind.Hat);
    Assert.True(proto);
    Assert.True(svc);
  }
}
