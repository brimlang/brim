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
    WriteNode(sb, source, root, [], isLast: true);
    return new Markup(sb.ToString());
  }

  static void WriteNode(StringBuilder sb, string source, GreenNode node, List<bool> ancestorStates, bool isLast)
  {
    // tree branch prefix - track which ancestor levels need continuation lines
    // ancestorStates has one entry per ancestor level above this node
    // The root has empty ancestorStates and no prefix; all others get a prefix
    if (ancestorStates.Count > 0)
    {
      _ = sb.Append(' ');
      // Print continuation lines for all ancestors except the immediate parent
      for (int i = 0; i < ancestorStates.Count - 1; i++)
        _ = sb.Append(ancestorStates[i] ? "    " : "│   ");
      _ = sb.Append(isLast ? "└── " : "├── ");
    }

    int width = node.FullWidth;
    (int line, int col) = GetPrimaryLocation(node);
    _ = sb
      .Append(NodeColor(node.Kind))
      .Append(node.Kind)
      .Append("[/]")
      .Append($" [dim]@{line}:{col}[[{width}]][/]");

    GreenToken? tokenForComments = null;
    switch (node)
    {
      case ServiceImpl si:
        _ = sb.Append(" [yellow]svc=[/]").Append(Escape(si.ServiceRef.GetText(source)));
        _ = sb.Append(" [yellow]recv=[/]").Append(Escape(si.ReceiverIdent.GetText(source)));
        _ = sb.Append($" [dim]init_fields={si.InitDecls.Count} members={si.Members.Count}[/]");
        break;
      case TypeDeclaration td:
        _ = sb.Append(" [yellow]name=[/]").Append(Escape(td.Name.Identifier.GetText(source)));
        _ = sb.Append(" [yellow]type=[/]").Append(td.TypeNode.ToString());
        break;
      case ModuleDirective md:
        _ = sb.Append(" [yellow]path=[/]").Append(Escape(md.ModuleHeader.GetText(source)));
        break;
      case ExportList ed:
        _ = sb.Append(" [yellow]name=[/]").Append(Escape(ed.GetIdentifiersText(source)));
        break;
      case ImportDeclaration id:
        _ = sb.Append(" [yellow]name=[/]").Append(Escape(id.Identifier.GetText(source)));
        _ = sb.Append(" [yellow]from=[/]").Append(Escape(id.Path.GetText(source)));
        break;
      case StructShape ss:
        _ = sb.Append($" [dim]fields={ss.FieldList.Elements.Count}[/]");
        break;
      case UnionShape us:
        _ = sb.Append($" [dim]variants={us.VariantList.Elements.Count}[/]");
        break;
      case FlagsShape fs:
        _ = sb.Append($" [dim]flags={fs.MemberList.Elements.Count}[/]");
        break;
      case NamedTupleShape nts:
        _ = sb.Append($" [dim]elems={nts.ElementList.Elements.Count}[/]");
        break;
      case ProtocolShape ps:
        _ = sb.Append($" [dim]methods={ps.MethodList.Elements.Count}[/]");
        break;
      case ServiceShape svs:
        _ = sb.Append($" [dim]protos={svs.ProtocolList.Elements.Count}[/]");
        break;
      case GreenToken t:
        _ = sb.Append(' ').Append(TokenSummary(t, source));
        tokenForComments = t;
        break;
      default:
        break;
    }

    _ = sb.Append('\n');

    // append comments after newline so they are on their own lines
    if (tokenForComments is not null)
      AppendComments(sb, tokenForComments, source, ancestorStates, isLast);

    // children - build proper ancestor tracking for indentation
    List<GreenNode> children = [.. node.GetChildren()];
    if (children.Count > 0)
    {
      // Root node (ancestorStates empty): add a sentinel to trigger prefix for children
      // Other nodes: extend ancestor chain with this node's continuation state
      List<bool> childAncestors = ancestorStates.Count == 0
        ? [false]  // Sentinel: root's children get ` ├──` prefix with no continuation bars above
        : [.. ancestorStates, isLast];
        
      for (int i = 0; i < children.Count; i++)
        WriteNode(sb, source, children[i], childAncestors, i == children.Count - 1);
    }
  }

  static string TokenSummary(GreenToken t, string source)
  {
    // if (t.Token.Kind == RawKind.Error) return "[red]<error>[/]";
    string text = t.GetText(source);
    if (t.SyntaxKind == SyntaxKind.TerminatorToken)
    {
      // visualize newline(s)
      text = text.Replace("\r", "\\r").Replace("\n", "\\n");
    }
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

  static void AppendComments(StringBuilder sb, GreenToken token, string source, List<bool> ancestorStates, bool isLast)
  {
    if (!token.HasLeading) return;
    foreach (RawToken trivia in token.LeadingTrivia)
    {
      if (trivia.Kind == RawKind.CommentTrivia)
      {
        _ = sb.Append(' ');
        // Draw continuation lines for all ancestors
        for (int i = 0; i < ancestorStates.Count; i++)
          _ = sb.Append(ancestorStates[i] ? "    " : "│   ");
        // The token itself: if it's not the last child, draw continuation; otherwise spaces
        _ = sb.Append(isLast ? "    " : "│   ");
        // Add extra indentation since comment is conceptually inside the token
        _ = sb.Append("    ");

        SourceText sourceText = SourceText.From(source);
        string txt = trivia.Value(sourceText.Span).Trim().ToString();
        txt = Escape(txt);
        _ = sb.Append("[dim]# ").Append(txt).Append($" @{trivia.Line}:{trivia.Column}[/]\n");
      }
    }
  }

  static string Escape(string s) => s.Replace("[", "[[").Replace("]", "]]");

  static string NodeColor(SyntaxKind kind) => kind switch
  {
    // Error tokens - bright red
    SyntaxKind.ErrorToken => "[red bold]",

    // Key structural nodes - bold
    SyntaxKind.Module => "[blue bold]",
    SyntaxKind.ModuleDirective => "[green bold]",

    // Declarations - distinct colors
    SyntaxKind.FunctionDeclaration => "[magenta]",
    SyntaxKind.TypeDeclaration => "[cyan]",
    SyntaxKind.ValueDeclaration => "[yellow]",
    SyntaxKind.ImportDeclaration => "[green]",
    SyntaxKind.ExportList => "[green]",
    
    // Service/Protocol specific
    SyntaxKind.ProtocolDeclaration or
    SyntaxKind.ServiceDeclaration or
    SyntaxKind.ServiceLifecycleDecl or
    SyntaxKind.ServiceProtocolDecl => "[teal]",

    // Type shapes - yellow tones
    SyntaxKind.StructShape or
    SyntaxKind.UnionShape or
    SyntaxKind.FlagsShape or
    SyntaxKind.NamedTupleShape or
    SyntaxKind.ProtocolShape or
    SyntaxKind.FunctionShape or
    SyntaxKind.ServiceShape => "[yellow]",

    // Field/variant declarations
    SyntaxKind.FieldDeclaration or
    SyntaxKind.UnionVariantDeclaration or
    SyntaxKind.FlagMemberDeclaration => "[olive]",

    // Expressions - purple tones
    SyntaxKind.BlockExpr or
    SyntaxKind.CallExpr or
    SyntaxKind.MatchExpr => "[purple]",

    SyntaxKind.ServiceConstruct or
    SyntaxKind.StructConstruct or
    SyntaxKind.SeqConstruct => "[mediumpurple]",

    // Lists and containers - muted purple
    SyntaxKind.CommaList or
    SyntaxKind.ListElement or
    SyntaxKind.ParameterList or
    SyntaxKind.FieldList => "[grey69]",

    // Generic parameters - muted
    SyntaxKind.GenericParameterList or
    SyntaxKind.GenericArgumentList => "[grey69]",

    // Type references and identifiers
    SyntaxKind.TypeRef or
    SyntaxKind.QualifiedIdentifier => "[grey78]",
    SyntaxKind.IdentifierToken => "[cyan]",

    // Tokens - very muted
    SyntaxKind.TerminatorToken => "[grey50]",
    _ when kind.ToString().EndsWith("Token") => "[grey58]",

    // Everything else - light grey
    _ => "[grey74]",
  };
}
