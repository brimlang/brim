using Brim.Parse.Green;
using Spectre.Console;

static class GreenNodeExtensions
{
  public static Markup Format<T>(this T n, ReadOnlySpan<char> source)
    where T : GreenNode
  {
    return n switch
    {
      ModuleDirective mh => mh.Format(source),
      ExportDirective ed => ed.Format(source),
      ImportDeclaration id => id.Format(source),
      StructDeclaration sd => sd.Format(source),
      GreenToken gt => gt.Format(source),
      _ => Markup.FromInterpolated($"[red]Unknown node type:[/] {n.Kind}\n"),
    };
  }

  public static Markup Format(this GreenToken token, ReadOnlySpan<char> source)
  {
    return token.Kind switch
    {
      SyntaxKind.ErrorToken when token.Diagnostics[0] is Diagnostic diag =>
        Markup.FromInterpolated($"[red]Error:[/] {diag.code} {token.Token.Kind}@{token.Token.Line}:{token.Token.Column}({token.Offset}): '{token.Token.Value(source).ToString()}'\n"),
      SyntaxKind.TerminatorToken => Markup.FromInterpolated($"[green]Token:[/] [yellow]{token.Token.Kind}[/]\n"),
      _ => Markup.FromInterpolated($"[green]Token:[/] [yellow]{token.Token.Kind}[/]\n")
    };
  }

  public static Markup Format(this StructDeclaration n, ReadOnlySpan<char> source)
  {
    string name = n.Identifier.GetText(source);
    string fieldsAndTypes = string.Empty;
    for (int i = 0; i < n.Fields.Count; i++)
    {
      FieldDeclaration field = n.Fields[i];
      string fieldName = field.Identifier.GetText(source);
      string fieldType = field.TypeAnnotation.GetText(source);
      fieldsAndTypes += $"{fieldName}: {fieldType}";
      if (i < n.Fields.Count - 1)
      {
        fieldsAndTypes += ", ";
      }
    }

    return Markup.FromInterpolated($"[green]Struct:[/] [yellow]{name}[/] with {n.Fields.Count} fields: {fieldsAndTypes}\n");
  }

  public static Markup Format(this ModuleDirective n, ReadOnlySpan<char> source)
  {
    string path = n.ModuleHeader.GetText(source);
    return Markup.FromInterpolated($"[blue]ModuleHeader:[/] [yellow]{path}[/]\n");
  }

  public static Markup Format(this ExportDirective n, ReadOnlySpan<char> source)
  {
    string name = n.Identifier.GetText(source);
    return Markup.FromInterpolated($"[blue]Export:[/] [yellow]{name}[/]\n");
  }

  public static Markup Format(this ImportDeclaration n, ReadOnlySpan<char> source)
  {
    string name = n.Identifier.GetText(source);
    string header = n.ModuleHeader.GetText(source);
    return Markup.FromInterpolated($"[blue]Import:[/] '[yellow]{name}[/]' from '[yellow]{header}[/]'\n");
  }
}
