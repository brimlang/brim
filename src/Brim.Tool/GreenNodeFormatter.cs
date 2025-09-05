using System.Text;
using Brim.Parse;
using Brim.Parse.Green;
using Spectre.Console;

namespace Brim.Tool;

/// <summary>
/// Stateless re-usable formatter (struct) configured with options once.
/// Call Render(source, node) to obtain markup for any green node.
/// </summary>
public readonly struct GreenNodeFormatter
{
  readonly RenderFlags _flags;

  public GreenNodeFormatter(RenderFlags flags = RenderFlags.Default) => _flags = flags;

  public Markup Render(ReadOnlySpan<char> source, GreenNode node) => node switch
  {
    GreenToken gt => FormatToken(gt, source),
    ModuleDirective md => FormatModuleDirective(md, source),
    ExportDirective ed => FormatExportDirective(ed, source),
    ImportDeclaration id => FormatImportDirective(id, source),
    StructDeclaration sd => FormatStructDeclaration(sd, source),
    UnionDeclaration ud => FormatUnionDeclaration(ud, source),
    _ => new Markup($"[red]Unknown node type:[/] {node.Kind}\n")
  };

  Markup FormatToken(GreenToken token, ReadOnlySpan<char> source)
  {
    StringBuilder sb = new();

    if ((_flags & RenderFlags.LeadingComments) != 0 && token.HasLeading)
      AppendTrivia(sb, token.LeadingTrivia, source, leading: true);

    sb.Append("[green]Token:[/] ")
      .Append("[yellow]")
      .Append(token.Token.Kind)
      .Append("[/]\n");

    if ((_flags & RenderFlags.TrailingComments) != 0 && token.HasTrailing)
      AppendTrivia(sb, token.TrailingTrivia, source, leading: false);

    return new Markup(sb.ToString());
  }

  Markup FormatModuleDirective(ModuleDirective n, ReadOnlySpan<char> source)
  {
    string path = n.ModuleHeader.GetText(source);
    return new Markup($"[blue]ModuleHeader:[/] [yellow]{Escape(path)}[/]\n");
  }

  Markup FormatExportDirective(ExportDirective n, ReadOnlySpan<char> source)
  {
    string name = n.Identifier.GetText(source);
    return new Markup($"[blue]Export:[/] [yellow]{Escape(name)}[/]\n");
  }

  Markup FormatImportDirective(ImportDeclaration n, ReadOnlySpan<char> source)
  {
    string name = n.Identifier.GetText(source);
    string header = n.ModuleHeader.GetText(source);
    return new Markup($"[blue]Import:[/] '[yellow]{Escape(name)}[/]' from '[yellow]{Escape(header)}[/]'\n");
  }

  Markup FormatStructDeclaration(StructDeclaration n, ReadOnlySpan<char> source)
  {
    string name = n.Identifier.GetText(source);
    StringBuilder sb = new();
    sb.Append("[green]Struct:[/] [yellow]").Append(Escape(name)).Append("[/] with ")
      .Append(n.Fields.Count).Append(" fields: ");
    for (int i = 0; i < n.Fields.Count; i++)
    {
      FieldDeclaration field = n.Fields[i];
      string fieldName = field.Identifier.GetText(source);
      string fieldType = field.TypeAnnotation.GetText(source);
      if (i > 0) sb.Append(", ");
      sb.Append(Escape(fieldName)).Append(": ").Append(Escape(fieldType));
    }
    sb.Append('\n');
    return new Markup(sb.ToString());
  }

  Markup FormatUnionDeclaration(UnionDeclaration n, ReadOnlySpan<char> source)
  {
    string name = n.Identifier.GetText(source);
    StringBuilder sb = new();
    sb.Append("[green]Union:[/] [yellow]").Append(Escape(name)).Append("[/] with ")
      .Append(n.Variants.Count).Append(" variants: ");
    for (int i = 0; i < n.Variants.Count; i++)
    {
      UnionVariantDeclaration uv = n.Variants[i];
      string variantName = uv.Identifier.GetText(source);
      string variantType = uv.Type.GetText(source);
      if (i > 0) sb.Append(", ");
      sb.Append(Escape(variantName)).Append(": ").Append(Escape(variantType));
    }
    sb.Append('\n');
    return new Markup(sb.ToString());
  }

  void AppendTrivia(StringBuilder sb, StructuralArray<RawToken> trivia, ReadOnlySpan<char> source, bool leading)
  {
    for (int i = 0; i < trivia.Count; i++)
    {
      RawToken t = trivia[i];
      if (t.Kind == RawTokenKind.CommentTrivia && ((_flags & RenderFlags.Comments) != 0))
      {
        string txt = new string(t.Value(source))
          .Replace("[", "[[")
          .Replace("]", "]]")
          .TrimEnd('\r', '\n');
        sb.Append("[grey]").Append(txt).Append("[/]\n");
      }
      else if (t.Kind == RawTokenKind.WhitespaceTrivia && ((_flags & RenderFlags.Whitespace) != 0))
      {
        string txt = new string(t.Value(source));
        if (txt.Contains('\n')) sb.Append("[dim]⏎[/]\n");
      }
    }
  }

  static string Escape(string s) => s
    .Replace("[", "[[")
    .Replace("]", "]]");
}

[System.Flags]
public enum RenderFlags : uint
{
  None = 0,
  Comments = 1 << 0,
  LeadingComments = 1 << 1,
  TrailingComments = 1 << 2,
  Whitespace = 1 << 3,
  Default = Comments | LeadingComments | TrailingComments
}
