using Brim.Parse.Collections;
using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class CommaListTests
{
  [Fact]
  public void Element_GetChildren_Order_WithBothLeading()
  {
    // Node token at offset 10
    RawToken rawNode = new(RawKind.Identifier, 10, 3, 1, 1);
    GreenToken node = new(SyntaxKind.IdentifierToken, rawNode);

    // Leading comma and terminator (positions arbitrary for test)
    RawToken rawComma = new(RawKind.Comma, 8, 1, 1, 1);
    GreenToken comma = new(SyntaxKind.CommaToken, rawComma);

    RawToken rawTerm = new(RawKind.Terminator, 9, 1, 1, 2);
    GreenToken term = new(SyntaxKind.TerminatorToken, rawTerm);

    var element = new CommaList<GreenToken>.Element(comma, term, node);

    var children = element.GetChildren().ToArray();
    Assert.Equal(3, children.Length);
    Assert.Same(comma, children[0]);
    Assert.Same(term, children[1]);
    Assert.Same(node, children[2]);
  }

  [Fact]
  public void Element_FullWidth_UsesLeadingTerminator_WhenCommaNull()
  {
    // Node token at offset 10
    RawToken rawNode = new(RawKind.Identifier, 10, 4, 1, 1);
    GreenToken node = new(SyntaxKind.IdentifierToken, rawNode);

    // Leading terminator at end offset 14
    RawToken rawTerm = new(RawKind.Terminator, 14, 1, 1, 5);
    GreenToken term = new(SyntaxKind.TerminatorToken, rawTerm);

    var element = new CommaList<GreenToken>.Element(null, term, node);

    int expected = term.EndOffset - node.Offset; // (14+1) - 10 = 5? EndOffset = Offset + Length => 14+1
    Assert.Equal(expected, element.FullWidth);
  }

  [Fact]
  public void ExportList_GetIdentifiersText_ReturnsCommaSeparatedNames()
  {
    // Source text with two identifiers "one,two"
    string src = "one,two";

    RawToken rawId1 = new(RawKind.Identifier, 0, 3, 1, 1); // "one"
    GreenToken id1 = new(SyntaxKind.IdentifierToken, rawId1);

    RawToken rawId2 = new(RawKind.Identifier, 4, 3, 1, 5); // "two"
    GreenToken id2 = new(SyntaxKind.IdentifierToken, rawId2);

    var e1 = new CommaList<GreenToken>.Element(null, null, id1);
    var e2 = new CommaList<GreenToken>.Element(null, null, id2);

    var elements = StructuralArray.Create(e1, e2);

    // Open/close tokens (positions not important for this test)
    RawToken rawOpen = new(RawKind.LessLess, 0, 2, 1, 1);
    GreenToken open = new(SyntaxKind.ExportOpenToken, rawOpen);
    RawToken rawClose = new(RawKind.GreaterGreater, 6, 2, 1, 7);
    GreenToken close = new(SyntaxKind.ExportEndToken, rawClose);

    var cl = new CommaList<GreenToken>(open, null, elements, null, null, close);
    var exportList = new ExportList(cl);

    string got = exportList.GetIdentifiersText(src.AsSpan());
    Assert.Equal("one, two", got);
  }

  static BrimModule Parse(string src) => Parser.ParseModule(src);

  private static readonly string[] _expected = ["one", "two", "three"];

  [Fact]
  public void CommaList_Parse_Layout_Variations_YieldSameElements()
  {
    string[] layouts =
    [
      "=[m]=;\n<<one,two,three>>;\n",
      "=[m]=;\n<<\none,\ntwo,\nthree\n>>;\n",
      "=[m]=;\n<<\none,\ntwo,\nthree,\n>>;\n",
    ];

    foreach (string src in layouts)
    {
      var m = Parse(src);
      var export = m.Members.OfType<ExportList>().First();
      var cl = export.List;
      Assert.Equal(3, cl.Elements.Length);
      var names = cl.Elements.Select(e => ((GreenToken)e.Node).GetText(src.AsSpan())).ToArray();
      Assert.Equal(_expected, names);
    }
  }
}
