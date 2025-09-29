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
    WriteNode(sb, source, root, 0, isLast: true);
    return new Markup(sb.ToString());
  }

  static void WriteNode(StringBuilder sb, string source, GreenNode node, int depth, bool isLast)
  {
    // tree branch prefix
    if (depth > 0)
    {
      _ = sb.Append(' ');
      for (int i = 0; i < depth - 1; i++)
        _ = sb.Append("│   ");
      _ = sb.Append(isLast ? "└── " : "├── ");
    }

    int width = node.FullWidth;
    (int line, int col) = GetPrimaryLocation(node);
    _ = sb
      .Append(NodeColor(node.Kind))
      .Append(node.Kind)
      .Append("[/]")
      .Append($" [grey]@{line}:{col}[[{width}]][/]");

    GreenToken? tokenForComments = null;
    switch (node)
    {
      case ServiceImpl si:
        _ = sb.Append(" svc=").Append(Escape(si.ServiceRef.GetText(source)));
        _ = sb.Append(" recv=").Append(Escape(si.ReceiverIdent.GetText(source)));
        _ = sb.Append($" init_fields={si.InitDecls.Count} members={si.Members.Count}");
        break;
      case TypeDeclaration td:
        _ = sb.Append(" name=").Append(Escape(td.Name.Identifier.GetText(source)));
        _ = sb.Append(" type=").Append(td.TypeNode.Kind.ToString());
        break;
      case ModuleDirective md:
        _ = sb.Append(" path=").Append(Escape(md.ModuleHeader.GetText(source)));
        break;
      case ExportDirective ed:
        _ = sb.Append(" name=").Append(Escape(ed.Identifier.GetText(source)));
        break;
      case ImportDeclaration id:
        _ = sb.Append(" name=").Append(Escape(id.Identifier.GetText(source)));
        _ = sb.Append(" from=").Append(Escape(id.Path.GetText(source)));
        break;
      case StructShape ss:
        _ = sb.Append($" fields={ss.Fields.Count}");
        break;
      case UnionShape us:
        _ = sb.Append($" variants={us.Variants.Count}");
        break;
      case FlagsShape fs:
        _ = sb.Append($" flags={fs.Members.Count}");
        break;
      case NamedTupleShape nts:
        _ = sb.Append($" elems={nts.Elements.Count}");
        break;
      case ProtocolShape ps:
        _ = sb.Append($" methods={ps.Methods.Count}");
        break;
      case ServiceShape svs:
        _ = sb.Append($" protos={svs.Protocols.Count}");
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
      AppendComments(sb, tokenForComments, source, depth + 1);

    // children
    List<GreenNode> children = [.. node.GetChildren()];
    for (int i = 0; i < children.Count; i++)
      WriteNode(sb, source, children[i], depth + 1, i == children.Count - 1);
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

  static void AppendComments(StringBuilder sb, GreenToken token, string source, int depth)
  {
    if (!token.HasLeading) return;
    foreach (RawToken trivia in token.LeadingTrivia)
    {
      if (trivia.Kind == RawKind.CommentTrivia)
      {
        // indentation similar to children; simple spaces for now
        _ = sb.Append(' ');
        for (int i = 0; i < depth - 1; i++)
          _ = sb.Append("│   ");

        _ = sb.Append("└── [grey]# ");
        SourceText sourceText = SourceText.From(source);
        string txt = trivia.Value(sourceText.Span).Trim().ToString();
        txt = Escape(txt);
        _ = sb.Append(txt).Append($"[/] [dim]@{trivia.Line}:{trivia.Column}[/]\n");
      }
    }
  }

  static string Escape(string s) => s.Replace("[", "[[").Replace("]", "]]");

  static string NodeColor(SyntaxKind kind) => kind switch
  {
    // Specific token colors
    SyntaxKind.ErrorToken => "[red]",
    SyntaxKind.IdentifierToken => "[cyan]",

    // Declarations (types/functions + import/export) magenta
    SyntaxKind.FunctionDeclaration or
    SyntaxKind.TypeDeclaration or
    SyntaxKind.ProtocolDeclaration or
    SyntaxKind.ServiceDeclaration or
    SyntaxKind.FieldDeclaration or
    SyntaxKind.UnionVariantDeclaration or
    SyntaxKind.FlagMemberDeclaration or
    SyntaxKind.ValueDeclaration or
    SyntaxKind.ImportDeclaration => "[magenta]",

    // Directive node (module header) green
    SyntaxKind.ModuleDirective or SyntaxKind.ExportDirective => "[green]",

    // Other structural / container nodes
    SyntaxKind.Module => "[bold blue]",
    SyntaxKind.ModuleHeader or SyntaxKind.ModulePath => "[blue]",
    SyntaxKind.FieldList => "[teal]",
    SyntaxKind.Block or SyntaxKind.ParameterList => "[purple]",
    SyntaxKind.GenericParameterList or SyntaxKind.GenericArgumentList => "[purple]",

    SyntaxKind.ConstraintList or SyntaxKind.MethodSignature => "[purple3]",

    SyntaxKind.GenericType or
    SyntaxKind.StructShape or
    SyntaxKind.UnionShape or
    SyntaxKind.FlagsShape or
    SyntaxKind.NamedTupleShape or
    SyntaxKind.ProtocolShape or
    SyntaxKind.FunctionShape or
    SyntaxKind.ServiceShape => "[yellow]",

    // All other tokens default to grey for low visual weight
    _ => "[grey]",
  };
}
