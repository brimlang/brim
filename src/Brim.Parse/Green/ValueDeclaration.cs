using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record ValueDeclaration(
  GreenToken? Mutator,
  GreenToken Name,
  GreenToken Colon,
  TypeExpr TypeNode,
  GreenToken Equal,
  StructuralArray<GreenNode> Initializer,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.ValueDeclaration, (Mutator ?? Name).Offset)
{
  public bool IsMutable => Mutator is not null;
  public override int FullWidth => Terminator.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    if (Mutator is not null) yield return Mutator;
    yield return Name;
    yield return Colon;
    foreach (GreenNode child in TypeNode.GetChildren())
      yield return child;
    yield return Equal;
    foreach (GreenNode n in Initializer) yield return n;
    yield return Terminator;
  }

  public static ValueDeclaration Parse(Parser p)
  {
    GreenToken? mutator = null;
    if (p.MatchRaw(RawKind.Hat))
      mutator = new GreenToken(SyntaxKind.HatToken, p.ExpectRaw(RawKind.Hat));

    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    TypeExpr typeExpr = TypeExpr.Parse(p);

    // Check if this looks like a function header without body (unsupported)
    if (p.MatchRaw(RawKind.Terminator) && mutator is null)
    {
      // This is likely "name :(params) RetType" without "= expr" or "{ body }"
      // Emit UnsupportedModuleMember diagnostic for function definitions
      p.AddDiagUnsupportedModuleMember(name.Token);

      GreenToken terminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);
      return new ValueDeclaration(mutator, name, colon, typeExpr,
        new GreenToken(SyntaxKind.ErrorToken, p.Current), [], terminator);
    }

    GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);

    ArrayBuilder<GreenNode> init = [];
    // Consume initializer tokens structurally until a Terminator (structure-only phase)
    // Handle blocks specially to consume them entirely including their closing brace
    while (!p.MatchRaw(RawKind.Terminator) && !p.MatchRaw(RawKind.Eob))
    {
      if (p.MatchRaw(RawKind.LBrace))
      {
        // Consume block structurally
        init.Add(new GreenToken(SyntaxKind.Undefined, p.ExpectRaw(RawKind.LBrace)));
        int depth = 1;
        while (depth > 0 && !p.MatchRaw(RawKind.Eob))
        {
          if (p.MatchRaw(RawKind.LBrace))
          {
            depth++;
            init.Add(new GreenToken(SyntaxKind.Undefined, p.ExpectRaw(RawKind.LBrace)));
          }
          else if (p.MatchRaw(RawKind.RBrace))
          {
            depth--;
            init.Add(new GreenToken(SyntaxKind.Undefined, p.ExpectRaw(RawKind.RBrace)));
          }
          else
          {
            init.Add(new GreenToken(SyntaxKind.Undefined, p.ExpectRaw(p.Current.CoreToken.Kind)));
          }
        }
      }
      else
      {
        init.Add(new GreenToken(SyntaxKind.Undefined, p.ExpectRaw(p.Current.CoreToken.Kind)));
      }
    }

    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ValueDeclaration(mutator, name, colon, typeExpr, eq, init, term);
  }
}
