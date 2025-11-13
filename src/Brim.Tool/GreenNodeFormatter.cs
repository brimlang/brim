using System.Text;
using Brim.Core;
using Brim.Parse;
using Brim.Parse.Green;

using Spectre.Console;

namespace Brim.Tool;

public static class GreenNodeFormatter
{
  public static Markup RenderTree(string source, GreenNode root)
  {
    StringBuilder sb = new();
    Walk(sb, source, root, "", isLast: true);
    return new Markup(sb.ToString());
  }

  static void Walk(StringBuilder sb, string source, GreenNode node, string indent, bool isLast)
  {
    // Draw tree branch
    _ = sb.Append(indent);
    if (indent.Length > 0)
      _ = sb.Append(isLast ? "└── " : "├── ");

    // Node name with color
    string color = GetColor(node.SyntaxKind);
    _ = sb.Append($"[{color}]").Append(node.SyntaxKind).Append("[/]");

    // Location metadata
    (int line, int col) = GetLocation(node);
    int width = node.FullWidth;
    _ = sb.Append($" [grey46]@{line}:{col}[[{width}]][/]");

    // Token value if applicable
    if (node is GreenToken token)
    {
      string text = token.GetText(source);
      if (token.SyntaxKind == SyntaxKind.TerminatorToken)
        text = text.Replace("\r", "\\r").Replace("\n", "\\n");
      _ = sb.Append(" [grey78]'").Append(Escape(text)).Append("'[/]");
    }

    _ = sb.Append('\n');

    // Process children
    List<GreenNode> children = [.. node.GetChildren()];

    // For tokens with trivia, show trivia as pseudo-children first
    int triviaCount = 0;
    if (node is GreenToken tok && tok.HasLeading)
    {
      foreach (TriviaToken trivia in tok.LeadingTrivia)
      {
        if (trivia.TokenKind == TokenKind.CommentTrivia)
        {
          triviaCount++;
        }
      }
    }

    string childIndent = indent.Length == 0
      ? " "
      : indent + (isLast ? "    " : "│   ");

    // Render trivia first
    if (node is GreenToken tkn && tkn.HasLeading)
    {
      SourceText sourceText = SourceText.From(source);
      int triviaIndex = 0;
      foreach (TriviaToken trivia in tkn.LeadingTrivia)
      {
        if (trivia.TokenKind == TokenKind.CommentTrivia)
        {
          _ = sb.Append(childIndent);
          bool isLastTrivia = (triviaIndex == triviaCount - 1) && children.Count == 0;
          _ = sb.Append(isLastTrivia ? "└── " : "├── ");

          string comment = trivia.Chars(sourceText.Span).Trim().ToString();
          _ = sb.Append("[green3]# ").Append(Escape(comment)).Append("[/]")
            .Append($" [grey46]@{trivia.Line}:{trivia.Column}[[{trivia.Length}]][/]\n");
          triviaIndex++;
        }
      }
    }

    // Then render actual children
    for (int i = 0; i < children.Count; i++)
      Walk(sb, source, children[i], childIndent, i == children.Count - 1);
  }

  static (int line, int col) GetLocation(GreenNode node)
  {
    if (node is GreenToken token)
      return (token.CoreToken.Line, token.CoreToken.Column);

    foreach (GreenNode child in node.GetChildren())
    {
      (int line, int col) = GetLocation(child);
      if (line != 0 || col != 0)
        return (line, col);
    }
    return (0, 0);
  }

  static string Escape(string s) => s.Replace("[", "[[").Replace("]", "]]");

  static string GetColor(SyntaxKind kind) => kind switch
  {
    // Structural - bold blue
    SyntaxKind.Module => "blue bold",
    SyntaxKind.ModuleDirective => "dodgerblue1",

    // Declarations - distinct bright colors
    SyntaxKind.FunctionDeclaration => "hotpink",
    SyntaxKind.TypeDeclaration => "cyan1",
    SyntaxKind.ValueDeclaration => "yellow1",
    SyntaxKind.ImportDeclaration => "lime",
    SyntaxKind.ExportList => "lime",

    // Service/Protocol - teal/aqua tones
    SyntaxKind.ServiceDeclaration => "aquamarine1",
    SyntaxKind.ProtocolDeclaration => "turquoise2",
    SyntaxKind.ServiceLifecycleDecl => "cyan3",
    SyntaxKind.ServiceProtocolDecl => "cadetblue",

    // Type shapes - orange/yellow tones
    SyntaxKind.StructShape => "orange1",
    SyntaxKind.UnionShape => "darkorange",
    SyntaxKind.FlagsShape => "gold1",
    SyntaxKind.NamedTupleShape => "yellow3",
    SyntaxKind.ProtocolShape => "khaki1",
    SyntaxKind.FunctionShape => "sandybrown",
    SyntaxKind.ServiceShape => "lightsalmon1",

    // Field/variant declarations - olive/green
    SyntaxKind.FieldDeclaration => "darkseagreen",
    SyntaxKind.UnionVariantDeclaration => "palegreen3",
    SyntaxKind.FlagMemberDeclaration => "darkolivegreen1",

    // Expressions - purple/magenta tones
    SyntaxKind.BlockExpr => "mediumpurple1",
    SyntaxKind.CallExpr => "orchid",
    SyntaxKind.MatchExpr => "plum2",

    // Constructs - violet tones
    SyntaxKind.ServiceConstruct => "violet",
    SyntaxKind.StructConstruct => "mediumorchid1",
    SyntaxKind.SeqConstruct => "thistle1",

    // Lists and containers - muted greys
    SyntaxKind.CommaList => "grey69",
    SyntaxKind.ListElement => "grey62",
    SyntaxKind.ParameterList => "grey66",
    SyntaxKind.FieldList => "grey63",

    // Generic parameters - light grey
    SyntaxKind.GenericParameterList => "grey70",
    SyntaxKind.GenericArgumentList => "grey70",

    // Type references - steel/slate
    SyntaxKind.TypeRef => "lightsteelblue",
    SyntaxKind.QualifiedIdentifier => "lightskyblue3",

    // Tokens - color by category
    SyntaxKind.IdentifierToken => "cornflowerblue",
    SyntaxKind.IntToken => "lightgreen",
    SyntaxKind.StrToken => "lightgreen",
    SyntaxKind.DecimalToken => "lightgreen",
    SyntaxKind.TerminatorToken => "grey50",

    // Keywords and operators - various
    SyntaxKind.ErrorToken => "red bold",
    _ when kind.ToString().EndsWith("Token") => "grey58",

    // Default
    _ => "grey74",
  };
}
