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
  public override int FullWidth => CloseBrace.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ServiceRef;
    yield return OpenBrace;
    foreach (GreenNode m in Members) yield return m;
    yield return CloseBrace;
  }

  public static ServiceLifecycleDecl Parse(Parser p)
  {
    // TypeRef '{' (ServiceCtorDecl | ServiceDtorDecl)* '}'
    TypeRef tref = TypeRef.Parse(p);

    GreenToken ob = p.ExpectSyntax(SyntaxKind.OpenBlockToken);

    ArrayBuilder<GreenNode> members = [];
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
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
    return new ServiceLifecycleDecl(tref, ob, members.ToImmutable(), cb);
  }
}

/// <summary>
/// Service constructor: '(' params ')' '@' '!' BlockExpr
/// </summary>
public sealed partial record ServiceCtorDecl(
  CommaList<ServiceMethodParam> ParamList,
  GreenToken AtSign,
  GreenToken Bang,
  BlockExpr Block)
  : GreenNode(SyntaxKind.ServiceCtorDecl, ParamList.Offset)
{
  public override int FullWidth => Block.EndOffset - ParamList.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ParamList;
    yield return AtSign;
    yield return Bang;
    yield return Block;
  }

  public static ServiceCtorDecl Parse(Parser p)
  {
    // '(' params ')' '@' '!' '{' ... '}'
    CommaList<ServiceMethodParam> @params = CommaList<ServiceMethodParam>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      ServiceMethodParam.Parse);

    GreenToken at = p.ExpectSyntax(SyntaxKind.ServiceImplToken);
    GreenToken bang = p.ExpectSyntax(SyntaxKind.BangToken);
    BlockExpr expr = BlockExpr.Parse(p);
    return new ServiceCtorDecl(@params, at, bang, expr);
  }
}

/// <summary>
/// Service destructor: '~' '(' params ')' TypeExpr BlockExpr
/// </summary>
public sealed record ServiceDtorDecl(
  GreenToken Tilde,
  CommaList<ServiceMethodParam> Params,
  TypeExpr ReturnType,
  BlockExpr Block)
  : GreenNode(SyntaxKind.ServiceDtorDecl, Tilde.Offset)
{
  public override int FullWidth => Block.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Tilde;
    yield return Params;
    yield return ReturnType;
    yield return Block;
  }

  public static ServiceDtorDecl Parse(Parser p)
  {
    // '~' '(' params ')' TypeExpr '{' ... '}'
    GreenToken tilde = p.ExpectSyntax(SyntaxKind.TildeToken);
    CommaList<ServiceMethodParam> @params = CommaList<ServiceMethodParam>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      ServiceMethodParam.Parse);
    TypeExpr retType = TypeExpr.Parse(p);
    BlockExpr expr = BlockExpr.Parse(p);
    return new ServiceDtorDecl(tilde, @params, retType, expr);
  }
}

/// <summary>
/// Service protocol implementation: TypeRef [AngleList]? [Receiver]? '{' methods* '}'
/// </summary>
public sealed record ServiceProtocolDecl(
  GreenNode ServiceRef,
  ProtocolList? ProtocolConstraints,
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
    if (ProtocolConstraints is not null) yield return ProtocolConstraints;
    if (Receiver is not null) yield return Receiver;
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
    GreenNode sref = TypeRef.Parse(p);

    // Optional protocol constraints: '<' TypeRef (',' TypeRef)* '>'
    ProtocolList? protocols = null;
    if (p.MatchRaw(RawKind.Less))
    {
      protocols = ProtocolList.Parse(p);
    }

    // Optional receiver: '(' ident ':' '@' ')'
    ServiceReceiver? receiver = null;
    if (p.MatchRaw(RawKind.LParen))
    {
      receiver = ServiceReceiver.Parse(p);
    }

    GreenToken ob = p.ExpectSyntax(SyntaxKind.OpenBlockToken);

    // Parse method declarations
    ArrayBuilder<GreenNode> methods = [];
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
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
    return new ServiceProtocolDecl(sref, protocols, receiver, ob, methods, cb);
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
  BlockExpr Block)
  : GreenNode(SyntaxKind.ServiceMethodDecl, Name.Offset)
{
  public override int FullWidth => Block.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return Params;
    yield return ReturnType;
    yield return Block;
  }

  public static ServiceMethodDecl Parse(Parser p)
  {
    // Ident ':' ParamList TypeExpr '{' ... '}'
    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    CommaList<ServiceMethodParam> @params = CommaList<ServiceMethodParam>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      ServiceMethodParam.Parse);
    TypeExpr retType = TypeExpr.Parse(p);
    BlockExpr expr = BlockExpr.Parse(p);
    return new ServiceMethodDecl(name, colon, @params, retType, expr);
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

