namespace Brim.Core.Tests;

public class OptTests
{
  [Fact]
  public void Some_HasValueAndReturnsValue()
  {
    Opt<string> opt = new("value");

    Assert.True(opt.HasValue);
    Assert.False(opt.IsNone);
    Assert.Equal("value", opt.Value);
    Assert.Equal("value", opt.GetValueOrDefault());
    Assert.Equal("value", opt.GetValueOrDefault("fallback"));
    Assert.True(opt.TryGet(out string? extracted));
    Assert.Equal("value", extracted);
  }

  [Fact]
  public void None_ProvidesDefaultsAndThrowsOnValue()
  {
    Opt<int> opt = Opt<int>.None;

    Assert.True(opt.IsNone);
    Assert.False(opt.HasValue);
    Assert.Throws<InvalidOperationException>(() => _ = opt.Value);
    Assert.Equal(0, opt.GetValueOrDefault());
    Assert.Equal(42, opt.GetValueOrDefault(42));
    Assert.False(opt.TryGet(out int value));
    Assert.Equal(0, value);
  }

  [Fact]
  public void Equality_ComparesPresenceAndValue()
  {
    Opt<int> first = new(1);
    Opt<int> second = new(1);
    Opt<int> different = new(2);

    Assert.True(first == second);
    Assert.False(first != second);
    Assert.False(first == different);
    Assert.False(first == Opt<int>.None);
    Assert.True(Opt<int>.None == default);
  }
}
