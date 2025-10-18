using Brim.Parse.Collections;

namespace Brim.Parse.Green;

/// <summary>
/// Service lifecycle declaration: TypeRef '{' (ctor | dtor)* '}'
/// Constructor: '(' params ')' '@' '!' BlockExpr
/// Destructor: '~' '(' params ')' TypeExpr BlockExpr
/// </summary>
public sealed record ServiceLifecycleDecl(
  GreenNode ServiceRef,
  GreenToken OpenBrace,
  StructuralArray<GreenNode> Members,
  GreenToken CloseBrace)
  : GreenNode(SyntaxKind.ServiceLifecycleDecl, ServiceRef.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - ServiceRef.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ServiceRef;
    yield return OpenBrace;
    foreach (GreenNode m in Members)
      yield return m;
    yield return CloseBrace;
  }

  public static ServiceLifecycleDecl Parse(Parser p)
  {
    // TypeRef '{' (ServiceCtorDecl | ServiceDtorDecl)* '}'
    GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenNode sref = head;
    if (p.MatchRaw(RawKind.LBracket))
      sref = TypeRef.ParseAfterName(p, head);

    GreenToken ob = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    ImmutableArray<GreenNode>.Builder members = ImmutableArray.CreateBuilder<GreenNode>();
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      // Skip terminators and comments
      if (p.MatchRaw(RawKind.Terminator) || p.MatchRaw(RawKind.CommentTrivia))
      {
        _ = p.ExpectRaw(p.Current.Kind);
        continue;
      }

      // Constructor: '(' params ')' '@' '!' BlockExpr
      if (p.MatchRaw(RawKind.LParen))
      {
        members.Add(ServiceCtorDecl.Parse(p));
        continue;
      }

      // Destructor: '~' '(' params ')' TypeExpr BlockExpr
      if (p.MatchRaw(RawKind.Tilde))
      {
        members.Add(ServiceDtorDecl.Parse(p));
        continue;
      }

      // Unexpected - consume and continue
      _ = p.ExpectRaw(p.Current.Kind);
    }

    GreenToken cb = p.ExpectSyntax(SyntaxKind.CloseBlockToken);
    return new ServiceLifecycleDecl(sref, ob, members.ToImmutable(), cb);
  }
}

/// <summary>
/// Service constructor: '(' params ')' '@' '!' BlockExpr
/// </summary>
public sealed record ServiceCtorDecl(
  CommaList<ServiceMethodParam> Params,
  GreenToken AtSign,
  GreenToken Bang,
  GreenToken OpenBrace)
  : GreenNode(SyntaxKind.ServiceCtorDecl, Params.Offset)
{
  public override int FullWidth => OpenBrace.EndOffset - Params.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Params;
    yield return AtSign;
    yield return Bang;
    yield return OpenBrace;
  }

  public static ServiceCtorDecl Parse(Parser p)
  {
    // '(' params ')' '@' '!' BlockExpr (body skipped)
    CommaList<ServiceMethodParam> @params = CommaList<ServiceMethodParam>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      ServiceMethodParam.Parse);
    GreenToken at = p.ExpectSyntax(SyntaxKind.ServiceImplToken);
    GreenToken bang = p.ExpectSyntax(SyntaxKind.BangToken);
    GreenToken openBrace = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    SkipBlock(p);
    _ = p.ExpectSyntax(SyntaxKind.CloseBlockToken); // Consume the closing brace
    return new ServiceCtorDecl(@params, at, bang, openBrace);
  }

  internal static void SkipBlock(Parser p)
  {
    int depth = 1;
    while (!p.MatchRaw(RawKind.Eob) && depth > 0)
    {
      if (p.MatchRaw(RawKind.LBrace))
      {
        _ = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
        depth++;
        continue;
      }
      if (p.MatchRaw(RawKind.RBrace))
      {
        depth--;
        if (depth == 0)
        {
          break; // Leave the closing brace for the caller to consume
        }
        _ = p.ExpectSyntax(SyntaxKind.CloseBlockToken);
        continue;
      }
      _ = p.ExpectRaw(p.Current.Kind);
    }
    // Note: We do NOT consume the final closing brace here.
    // The caller should consume it.
  }
}

/// <summary>
/// Service destructor: '~' '(' params ')' TypeExpr BlockExpr
/// </summary>
public sealed record ServiceDtorDecl(
  GreenToken Tilde,
  CommaList<ServiceMethodParam> Params,
  TypeExpr ReturnType,
  GreenToken OpenBrace)
  : GreenNode(SyntaxKind.ServiceDtorDecl, Tilde.Offset)
{
  public override int FullWidth => OpenBrace.EndOffset - Tilde.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Tilde;
    yield return Params;
    foreach (GreenNode c in ReturnType.GetChildren())
      yield return c;
    yield return OpenBrace;
  }

  public static ServiceDtorDecl Parse(Parser p)
  {
    // '~' '(' params ')' TypeExpr BlockExpr (body skipped)
    GreenToken tilde = p.ExpectSyntax(SyntaxKind.TildeToken);
    CommaList<ServiceMethodParam> @params = CommaList<ServiceMethodParam>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      ServiceMethodParam.Parse);
    TypeExpr retType = TypeExpr.Parse(p);
    GreenToken openBrace = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    ServiceCtorDecl.SkipBlock(p);
    _ = p.ExpectSyntax(SyntaxKind.CloseBlockToken); // Consume the closing brace
    return new ServiceDtorDecl(tilde, @params, retType, openBrace);
  }
}

/// <summary>
/// Service protocol implementation: TypeRef [AngleList]? [Receiver]? '{' methods* '}'
/// </summary>
public sealed record ServiceProtocolDecl(
  GreenNode ServiceRef,
  GenericArgumentList? ProtocolConstraints,
  ServiceReceiver? Receiver,
  GreenToken OpenBrace,
  StructuralArray<GreenNode> Methods,
  GreenToken CloseBrace)
  : GreenNode(SyntaxKind.ServiceProtocolDecl, ServiceRef.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - ServiceRef.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ServiceRef;
    if (ProtocolConstraints is not null)
    {
      foreach (GreenNode c in ProtocolConstraints.GetChildren())
      {
        yield return c;
      }
    }
    if (Receiver is not null)
    {
      foreach (GreenNode c in Receiver.GetChildren())
      {
        yield return c;
      }
    }
    yield return OpenBrace;
    foreach (GreenNode m in Methods)
    {
      yield return m;
    }
    yield return CloseBrace;
  }

  public static ServiceProtocolDecl Parse(Parser p)
  {
    // TypeRef [AngleList<TypeRef>]? [Receiver]? '{' methods* '}'
    GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenNode sref = head;
    if (p.MatchRaw(RawKind.LBracket))
      sref = TypeRef.ParseAfterName(p, head);

    // Optional protocol constraints: '<' TypeRef (',' TypeRef)* '>'
    GenericArgumentList? protocols = null;
    if (p.MatchRaw(RawKind.Less))
    {
      protocols = GenericArgumentList.Parse(p);
    }

    // Optional receiver: '(' ident ':' '@' ')'
    ServiceReceiver? receiver = null;
    if (p.MatchRaw(RawKind.LParen))
    {
      receiver = ServiceReceiver.Parse(p);
    }

    GreenToken ob = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    // Parse method declarations
    ImmutableArray<GreenNode>.Builder methods = ImmutableArray.CreateBuilder<GreenNode>();
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      // Skip terminators and comments
      if (p.MatchRaw(RawKind.Terminator) || p.MatchRaw(RawKind.CommentTrivia))
      {
        _ = p.ExpectRaw(p.Current.Kind);
        continue;
      }

      // Method: Ident ':' ParamList TypeExpr BlockExpr
      if (p.MatchRaw(RawKind.Identifier))
      {
        methods.Add(ServiceMethodDecl.Parse(p));
        continue;
      }

      // Unexpected - consume and continue
      _ = p.ExpectRaw(p.Current.Kind);
    }

    GreenToken cb = p.ExpectSyntax(SyntaxKind.CloseBlockToken);
    return new ServiceProtocolDecl(sref, protocols, receiver, ob, methods.ToImmutable(), cb);
  }
}

/// <summary>
/// Service receiver parameter: '(' ident ':' '@' ')'
/// </summary>
public sealed record ServiceReceiver(
  GreenToken OpenParen,
  GreenToken Identifier,
  GreenToken Colon,
  GreenToken AtSign,
  GreenToken CloseParen)
  : GreenNode(SyntaxKind.ServiceReceiver, OpenParen.Offset)
{
  public override int FullWidth => CloseParen.EndOffset - OpenParen.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenParen;
    yield return Identifier;
    yield return Colon;
    yield return AtSign;
    yield return CloseParen;
  }

  public static ServiceReceiver Parse(Parser p)
  {
    // '(' ident ':' '@' ')'
    GreenToken op = p.ExpectSyntax(SyntaxKind.OpenParenToken);
    GreenToken id = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken at = p.ExpectSyntax(SyntaxKind.ServiceImplToken);
    GreenToken cp = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    return new ServiceReceiver(op, id, colon, at, cp);
  }
}

/// <summary>
/// Service method declaration: Ident ':' ParamList TypeExpr BlockExpr
/// </summary>
public sealed record ServiceMethodDecl(
  GreenToken Name,
  GreenToken Colon,
  CommaList<ServiceMethodParam> Params,
  TypeExpr ReturnType,
  GreenToken OpenBrace)
  : GreenNode(SyntaxKind.ServiceMethodDecl, Name.Offset)
{
  public override int FullWidth => OpenBrace.EndOffset - Name.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return Params;
    foreach (GreenNode c in ReturnType.GetChildren())
      yield return c;
    yield return OpenBrace;
  }

  public static ServiceMethodDecl Parse(Parser p)
  {
    // Ident ':' ParamList TypeExpr BlockExpr (body skipped)
    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    CommaList<ServiceMethodParam> @params = CommaList<ServiceMethodParam>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      ServiceMethodParam.Parse);
    TypeExpr retType = TypeExpr.Parse(p);
    GreenToken openBrace = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    ServiceCtorDecl.SkipBlock(p);
    _ = p.ExpectSyntax(SyntaxKind.CloseBlockToken); // Consume the closing brace
    return new ServiceMethodDecl(name, colon, @params, retType, openBrace);
  }
}

/// <summary>
/// Service method parameter: ident ':' Type
/// </summary>
public sealed record ServiceMethodParam(
  GreenToken Name,
  GreenToken Colon,
  TypeExpr Type)
  : GreenNode(SyntaxKind.ServiceMethodParam, Name.Offset)
{
  public override int FullWidth => Type.EndOffset - Name.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    foreach (GreenNode c in Type.GetChildren())
      yield return c;
  }

  public static ServiceMethodParam Parse(Parser p)
  {
    // ident ':' Type
    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    TypeExpr type = TypeExpr.Parse(p);
    return new ServiceMethodParam(name, colon, type);
  }
}
