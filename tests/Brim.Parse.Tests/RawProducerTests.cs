using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

public class RawProducerTests
{
  static List<RawToken> Lex(string input)
  {
    RawProducer p = new(SourceText.From(input), DiagSink.Create());
    List<RawToken> list = [];
    while (p.TryRead(out RawToken t)) { list.Add(t); if (t.Kind == RawKind.Eob) break; }
    return list;
  }

  [Theory]
  [InlineData("A")]
  [InlineData("a")]
  [InlineData("\u01C5")]
  [InlineData("\u02B0")]
  [InlineData("\u216B")]
  [InlineData("A\u0301")]
  [InlineData("A\u0951")]
  [InlineData("A0")]
  [InlineData("A_")]
  [InlineData("A\u202A")]
  [InlineData("_foo")]
  [InlineData("foo_bar")]
  [InlineData("foo123")]
  [InlineData("f\u0301oo")]
  [InlineData("变量")]
  [InlineData("προβλημa")]
  [InlineData("foo\u0301")]
  [InlineData("foo123bar")]
  [InlineData("fоо")] // mixed scripts
  public void ValidIdentifierUnicodeCategories(string identifier) => Assert.True(Utilities.IsValidIdentifier(identifier));

  [Theory]
  [InlineData("")]
  [InlineData("1foo")]
  [InlineData("!foo")]
  [InlineData("foo!")]
  [InlineData("foo bar")]
  [InlineData("foo-bar")]
  [InlineData("123")]
  [InlineData("!@#")]
  [InlineData(" foo")]
  [InlineData("foo\nbar")]
  [InlineData("\u0301foo")]
  [InlineData("😀")]
  [InlineData("\u0301")]
  [InlineData("foo\u0000bar")]
  [InlineData("foo\tbar")]
  [InlineData("foo!bar")]
  public void InvalidIdentifierCases(string identifier) => Assert.False(Utilities.IsValidIdentifier(identifier));

  [Fact]
  public void ProducesUnexpectedCharError()
  {
    var toks = Lex("foo $");
    Assert.Contains(toks, static t => t.Kind == RawKind.Error);
  }

  [Fact]
  public void TokenizesLineComments()
  {
    string input = "foo -- this is a comment\nbar --another";
    var toks = Lex(input);
    var comments = toks.Where(t => t.Kind == RawKind.CommentTrivia).ToArray();
    Assert.Equal(2, comments.Length);
    Assert.Equal("-- this is a comment", new string(comments[0].Value(input)));
    Assert.Equal("--another", new string(comments[1].Value(input)));
    Assert.Contains(toks, t => t.Kind == RawKind.Identifier && t.Value(input).SequenceEqual("foo"));
    Assert.Contains(toks, t => t.Kind == RawKind.Identifier && t.Value(input).SequenceEqual("bar"));
  }

  [Fact]
  public void TokenizesMultiCharSymbolsGreedily()
  {
    var toks = Lex("=> *{ ~=");
    Assert.Contains(toks, t => t.Kind == RawKind.EqualGreater);
    Assert.Contains(toks, t => t.Kind == RawKind.StarLBrace);
    Assert.Contains(toks, t => t.Kind == RawKind.TildeEqual);
  }

  [Fact]
  public void GreedyLessLessOverLess()
  {
    var toks = Lex("<< <");
    Assert.Contains(toks, t => t.Kind == RawKind.LessLess);
    Assert.Contains(toks, t => t.Kind == RawKind.Less);
  }

  [Fact]
  public void ColonFamilyTokens()
  {
    var toks = Lex(": :: := :* :");
    Assert.Contains(toks, t => t.Kind == RawKind.Colon);
    Assert.Contains(toks, t => t.Kind == RawKind.ColonColon);
    Assert.Contains(toks, t => t.Kind == RawKind.ColonEqual);
    Assert.Contains(toks, t => t.Kind == RawKind.ColonStar);
  }

  [Fact]
  public void UnterminatedStringVariantsMixedBehavior()
  {
    var lone = Lex("\"");
    Assert.Contains(lone, t => t.Kind == RawKind.StringLiteral);
    var dangling = Lex("\"foo\\");
    Assert.Contains(dangling, t => t.Kind == RawKind.Error);
  }

  [Fact]
  public void SequentialOffsetsAreMonotonic()
  {
    var toks = Lex("foo : bar");
    int prevEnd = -1;
    foreach (var t in toks.Where(t => t.Kind != RawKind.Eob))
    {
      int start = t.Offset;
      int end = t.Offset + t.Length;
      Assert.True(start >= 0);
      if (prevEnd >= 0) Assert.True(start >= prevEnd, $"Token {t.Kind} starts before prior end");
      prevEnd = Math.Max(prevEnd, end);
    }
  }

  [Fact]
  public void SymbolTableMultiSymsAreSortedAndPrefixed()
  {
    foreach (var kv in RawSymbolTable.SymbolTable)
    {
      var arr = kv.Value.multiSyms;
      for (int i = 1; i < arr.Length; i++) Assert.True(arr[i - 1].symbol.Length >= arr[i].symbol.Length);
      foreach (var (symbol, _) in arr) Assert.True(symbol.Length > 0 && symbol[0] == kv.Key);
    }
  }
}
