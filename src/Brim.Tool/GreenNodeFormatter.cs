using System.Text;
using Brim.Parse;
using Brim.Parse.Green;
using Spectre.Console;

namespace Brim.Tool;

public static class GreenNodeFormatter
{
  public static Markup RenderTree(string source, GreenNode root)
  {
    StringBuilder sb = new();
    WriteNode(sb, source, root, 0, isLast:true);
    return new Markup(sb.ToString());
  }

  static void WriteNode(StringBuilder sb, string source, GreenNode node, int depth, bool isLast)
  {
    // tree branch prefix
    if (depth > 0)
    {
      sb.Append(' ');
      for (int i = 0; i < depth - 1; i++) sb.Append("│   ");
      sb.Append(isLast ? "└── " : "├── ");
    }

    int width = node.FullWidth;
    (int line, int col) = GetPrimaryLocation(node);
    sb.Append(NodeColor(node.Kind)).Append(node.Kind).Append("[/]")
      .Append($" [grey]@{line}:{col}[[{width}]][/]");

  GreenToken? tokenForComments = null;
  switch (node)
    {
      case GreenToken t:
    sb.Append(" ").Append(TokenSummary(t, source));
    tokenForComments = t;
        break;
      case ModuleDirective md:
        sb.Append(" path=").Append(Escape(md.ModuleHeader.GetText(source)));
        break;
      case ExportDirective ed:
        sb.Append(" name=").Append(Escape(ed.Identifier.GetText(source)));
        break;
      case ImportDeclaration id:
        sb.Append(" name=").Append(Escape(id.Identifier.GetText(source)));
        sb.Append(" from=").Append(Escape(id.ModuleHeader.GetText(source)));
        break;
      case StructDeclaration sd:
        sb.Append(" name=").Append(Escape(sd.Identifier.GetText(source))).Append($" fields={sd.Fields.Count}");
        break;
      case UnionDeclaration ud:
        sb.Append(" name=").Append(Escape(ud.Identifier.GetText(source))).Append($" variants={ud.Variants.Count}");
        break;
    }
    sb.Append('\n');

    // append comments after newline so they are on their own lines
    if (tokenForComments is not null)
      AppendComments(sb, tokenForComments, source, depth + 1);

    // children
    IReadOnlyList<GreenNode> children = node.GetChildren().ToList();
    for (int i = 0; i < children.Count; i++)
      WriteNode(sb, source, children[i], depth + 1, i == children.Count - 1);
  }

  static string TokenSummary(GreenToken t, string source)
  {
    if (t.Token.Kind == RawTokenKind.Error) return "[red]<error>[/]";
    string text = t.GetText(source);
    if (t.SyntaxKind == SyntaxKind.TerminatorToken)
    {
      // visualize newline(s)
      text = text.Replace("\r", "\\r").Replace("\n", "\\n");
    }
    if (text.Length > 16) text = text[..16] + "…";
    text = Escape(text);
    return $"'{text}'"; // line:col now shown with the node header
  }

  static (int line, int col) GetPrimaryLocation(GreenNode node)
  {
    // For tokens, use their own coordinates
    if (node is GreenToken gt)
      return (gt.Token.Line, gt.Token.Column);

    // Walk first-token descendant
    foreach (GreenNode child in node.GetChildren())
    {
      (int line, int col) loc = GetPrimaryLocation(child);
      if (loc.line != 0 || loc.col != 0) return loc; // found
    }
    return (0, 0); // unknown
  }

  static void AppendComments(StringBuilder sb, GreenToken token, string source, int depth)
  {
    if (!token.HasLeading) return;
    foreach (RawToken trivia in token.LeadingTrivia)
    {
      if (trivia.Kind == RawTokenKind.CommentTrivia)
      {
        // indentation similar to children; simple spaces for now
        sb.Append(' ');
        for (int i = 0; i < depth - 1; i++) sb.Append("│   ");
        sb.Append("└── [grey]# ");
        string txt = new string(trivia.Value(source)).Trim();
        txt = Escape(txt);
  sb.Append(txt).Append($"[/] [dim]@{trivia.Line}:{trivia.Column}[/]\n");
      }
    }
  }

  static string Escape(string s) => s.Replace("[", "[[").Replace("]", "]]");

  static string NodeColor(SyntaxKind kind) => kind switch
  {
  // Specific token colors
  SyntaxKind.ErrorToken => "[red]",
  SyntaxKind.IdentifierToken => "[cyan]",
  // All other tokens default to grey for low visual weight
  _ when kind <= SyntaxKind.EobToken => "[grey]",

  // Declarations (types/functions + import/export) magenta
  SyntaxKind.FunctionDeclaration or
  SyntaxKind.StructDeclaration or
  SyntaxKind.UnionDeclaration or
  SyntaxKind.FieldDeclaration or
  SyntaxKind.UnionVariantDeclaration or
 SyntaxKind.ImportDeclaration => "[magenta]",

  // Directive node (module header) green
  SyntaxKind.ModuleDirective or SyntaxKind.ExportDirective => "[green]",

  // Other structural / container nodes
  SyntaxKind.Module => "[bold blue]",
  SyntaxKind.ModuleHeader or SyntaxKind.ModulePath => "[blue]",
  SyntaxKind.FieldList => "[teal]",
  SyntaxKind.Block or SyntaxKind.ParameterList => "[purple]",
  SyntaxKind.GenericParameterList or SyntaxKind.GenericArgumentList => "[purple]",
  SyntaxKind.GenericType => "[yellow]",
  _ => "[yellow]"
  };
}
