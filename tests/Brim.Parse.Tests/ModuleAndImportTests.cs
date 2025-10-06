using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class ModuleAndImportTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void ImportAlias_Parses_BasicPath()
  {
    string src = "[[m]];\nio ::= std::io;\n";
    var m = Parse(src);
    var imp = Assert.IsType<ImportDeclaration>(m.Members.First());
    Assert.Equal("io", imp.Identifier.GetText(src));
    // Path parts are tokens: Ident, '::', Ident
    var parts = imp.Path.Parts;
    Assert.Equal(3, parts.Count);
    Assert.Equal(SyntaxKind.IdentifierToken, parts[0].SyntaxKind);
    Assert.Equal("std", parts[0].GetText(src));
    Assert.Equal(SyntaxKind.ModulePathSepToken, parts[1].SyntaxKind);
    Assert.Equal("::", parts[1].GetText(src));
    Assert.Equal(SyntaxKind.IdentifierToken, parts[2].SyntaxKind);
    Assert.Equal("io", parts[2].GetText(src));
  }

  [Fact]
  public void ImportAlias_Parses_MultiSegmentPath()
  {
    string src = "[[m]];\nio ::= std::net::http;\n";
    var m = Parse(src);
    var imp = Assert.IsType<ImportDeclaration>(m.Members.First());
    var parts = imp.Path.Parts;
    Assert.Equal(5, parts.Count);
    Assert.Equal("std", parts[0].GetText(src));
    Assert.Equal("net", parts[2].GetText(src));
    Assert.Equal("http", parts[4].GetText(src));
  }

  [Fact]
  public void ImportAlias_TrailingSep_EmitsMissingIdentifier()
  {
    string src = "[[m]];\nio ::= std::\n";
    var m = Parse(src);
    Assert.Contains(m.Diagnostics, d => d.Code == DiagCode.MissingToken);
  }

  [Fact]
  public void TypeAlias_NamedTupleEmpty_EmitsUnexpected()
  {
    string src = "[[m]];\nAlias := #{};\n";
    var m = Parse(src);
    Assert.Contains(m.Diagnostics, d => d.Code == DiagCode.UnexpectedToken);
  }
}

