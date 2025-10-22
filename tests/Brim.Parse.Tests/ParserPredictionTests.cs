namespace Brim.Parse.Tests;

public class ParserPredictionTests
{
  [Fact]
  public void ModuleMemberPredictions_Contain_Dispatcher()
  {
    Assert.Contains(Parser.ModuleMemberPredictions,
      p => p.Action == Parser.ParseIdentifierHead);
  }

  [Fact]
  public void ModuleMemberPredictions_RetainServiceProtocols()
  {
    bool proto = Parser.ModuleMemberPredictions.Any(p =>
      p.Sequence.Length > 1 &&
      p.Sequence[0] == RawKind.Identifier &&
      p.Sequence[1] == RawKind.Less);
    Assert.True(proto);
  }
}
