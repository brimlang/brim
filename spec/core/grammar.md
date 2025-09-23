---
id: core.grammar
layer: core
title: Core Grammar
authors: ['trippwill', 'assistant']
updated: 2025-09-21
status: accepted
version: 0.1.1
---

# Core Grammar

This document captures the canonical grammar for the Brim language during the pre-release 0.1.1 cut. The grammar remains LL(k ≤ 4) and aligns with the lexer policy described in `AGENTS.md`.

## Sample Syntax

```brim
[[acme::hello]]

<<
Perms
User
UserProtocol
AuthProtocol
add_user
>>

panic: (str?) void = (msg) => {
  -- print msg and crash
}

Name  := str
-- width is assigned by compiler based on number of elements
Perms := &{read; write; exec}
Area  := |{
  Admin  :Perms
  Client :Perms
  Banned
}

User  := %{name  :Name; perms :Area}

AuthProtocol := .{
  login  :(str, str) User!
  logout :(User) unit
}

UserProtocol := .{
  add :(str, bool) User!
  ban :(User) unit
}

UserService := @{ perms :Perms; audit :list[str] }

UserService {
  (seed :Perms) @! {
    @{ perms = seed; audit = [] }
  }

  ~(svc :@) unit {
    std::log::flush(svc.audit)
  }
}

UserService<UserProtocol,AuthProtocol>(svc :@) {
  add :(name :str, is_admin :bool) User! {
    svc.perms = is_admin =>
      true  => Perms&{read,write,exec}
      false => Perms&{read}
    svc.audit = svc.audit ++ ["add:" ++ name]
    User%{ name = name; perms = Area|{ Admin = svc.perms } }
  }

  ban :(user :User) unit {
    svc.audit = svc.audit ++ ["ban:" ++ user.name]
  }

  login :(uname :str, pass :str) User! {
    check(uname, pass, svc.perms) =>
      true  => User%{ name = uname; perms = Area|{ Client = svc.perms } }
      false => !!{ mkerr("Not Authorized") }
  }

  logout :(user :User) unit {
    svc.audit = svc.audit ++ ["logout:" ++ user.name]
  }
}

-- Function Type Declarations and Definitions
adder := (i32, i32) i32
add_a :adder = (a, b) => a + b
add_b :(a :i32, b :i32) i32 => a + b
add_d :(a :i32, b :i32) i32 { a + b }

fold :(xs :list[i32], seed :i32, f :(i32, i32) i32) i32 {
  ^acc = seed
  -- no loops
  acc
}

AuthService := @{ foo :u32; bar :bool }

AuthService {
  (a :u32, b :bool) @! {
    start()
    @{ foo = a; bar = b }
  }

  ~(svc :@) unit {
    if std::runtime::armed(svc.foo) {
      end(svc.foo)
    }
  }
}

AuthService<Auth>(svc :@) {
  login :(uname :str, pass :str) User! {
    std::atoi(pass) == svc.foo =>
      true  => User%{ id = uname; auth = svc.bar }
      false => !!{ mkerr("Not Authorized") }
  }
}

pass :() unit {
  -- Basic propagation
  opt?                -- IDENT + PropagationOp '?'
  res!                -- IDENT + PropagationOp '!'

  -- Multiple casts then propagate
  value :> A :> B?    -- Primary IDENT + CAST A + CAST B + '?'
  value :> A :> B!    -- same with '!'

  -- Parenthesized cast chain then propagate
  (value :> A)?       -- '(' Expr ')' as Primary, CAST before '?'
  (value :> A :> B)!  -- grouped version (parentheses optional here)

  -- Method / call chains before propagation
  svc.login(user, pass)?
  data.normalize().encode()!

  -- Cast after call, then propagate
  get() :> Intermediate :> Final?

  -- Mixed prefix operators before call/access chains
  -foo?
  !maybeVal?
  -!result!

  -- Service init then method + propagation
  ServiceA@{ }.start()?

  -- Chained casts with no propagation
  x :> A :> B :> C

  -- Propagation alone after a parenthesized expression
  ( (foo) )?
}

-- expressions that should not parse
fail :() unit {
  expr? :> T          -- propagation must be final; CAST cannot follow PropagationOp
  expr! :> T          -- same reason
  svc.method()?.next()   -- nothing may follow a PropagationOp
  foo()? :> T?        -- CAST after propagation (prop must be last)
  x ?                 -- (space) not adjacency; '?' must directly follow chain (no TERM)
  :> T                -- missing left operand (needs CallOrAccessExpr before CAST)
}
```

Constructors use the `@Service: (params) ServiceType` header and must establish fields with exactly one `@{ ... }` literal in the body. Destructors appear as `~Service: () Type` and always execute on scope exit when the binding drops, even if construction failed, propagating the constructor’s error. Reads and writes to service state occur through `@.field` and `^@.field` respectively.

## Grammar

1. Whitespace is not significant unless contained in a terminal literal.
2. Rules are written for clarity to contributors, not generators.

### Notation & Conventions

| Form             | Meaning / Style                                | Example               |
|------------------|------------------------------------------------|-----------------------|
| `"..."`          | Prose description                              | "identifier"         |
| `NAME = ...`     | Token rule (ALL_CAPS)                          | `LINE_COMMENT = '--'` |
| `'...'`          | Literal token                                  | `'{'`                 |
| `Name : ...`     | Production rule (PascalCase noun)              | `FunctionLiteral : …` |
| `{...}`          | Adjacency required                             | `{TypeRef SERVICE_SHAPE}` |
| `[...]`          | Grouping / optional sequences                   | `[Declaration TERM]+` |
| `\|`              | Alternation                                    | `Expr \| Block`        |
| `-`              | Character-class subtraction                    | `[Letter - Digit]`    |
| `*` / `+` / `?`  | Quantifiers (zero-or-more / one-or-more / opt) | `CommaList<T>` uses `*` |
| Templates        | PascalCase `List`/`Seq` helpers                | `CommaList<Expr>`     |
| Trivia           | PascalCase + `Trivia` suffix                   | `LineCommentTrivia`   |

### Structural Templates

```
CommaList<T>        : T [',' T]* [',']?
CommaListOpt<T>     : [CommaList<T>]?
TerminatedList<T>   : T [TERM T]* [TERM]?
ShapeHeader<S>      : {TypeRef S} NEWLINE?
ShapeClose          : NEWLINE? '}'
ShapeBlock<S, T>    : S NEWLINE? TerminatedList<T> ShapeClose
```

### Operator Tokens & Precedence

Infix Tokens:

```
OP_MULT  = '*'
OP_DIV   = '/'
OP_MOD   = '%'
OP_ADD   = '+'
OP_SUB   = '-'
OP_AND   = '&&'
OP_OR    = '||'
OP_EQ    = '=='
OP_NEQ   = '!='
OP_LT    = '<'
OP_GT    = '>'
OP_LEQ   = '<='
OP_GEQ   = '>='
```

Prefix Tokens:

```
OP_NEG   = OP_SUB
OP_NOT   = '!'
```

Precedence and Associativity:

|Tier|Operators                  |Assoc|
|----|---------------------------|-----|
|  90| OP_NOT OP_NEG             |R    |
|  80| OP_MULT OP_DIV OP_MOD     |L    |
|  75| OP_ADD OP_SUB             |L    |
|  70| OP_LT OP_GT OP_LEQ OP_GEQ |N    |
|  65| OP_EQ OP_NEQ              |N    |
|  50| OP_AND                    |L    |
|  45| OP_OR                     |L    |


### Token Groups

```
IDENT          = "identifier"
NEWLINE        = '\n'
TERM           = NEWLINE | ';'
LINE_COMMENT   = '--'

ARROW          = '=>'
CAST           = ':>'
MUTABLE        = '^'
SERVICE_MEMBER = '@'  -- service member introducer
DESTRUCTOR     = '~'  -- destructor introducer

BIND_MODULE    = '::='
BIND_TYPE      = ':='
BIND_SERVICE   = '~='
BIND_VALUE     = '='

TUPLE_SHAPE    = '#{'
STRUCT_SHAPE   = '%{'
UNION_SHAPE    = '|{'
FLAGS_SHAPE    = '&{'
PROTOCOL_SHAPE = '.{'
SERVICE_SHAPE  = '@{'
OPT_SHAPE      = '?{'
OK_SHAPE       = '!{'
ERR_SHAPE      = '!!{'

REST           = '..'
GUARD          = '??'
ELSE           = '::' -- For future ternary expr
OPTIONAL       = '?'   -- postfix propagation-on-success marker
FALLIBLE       = '!'   -- postfix propagation-on-failure marker

STRING         = '"' "allowed string contents" '"'
RUNE           = '\'' "allowed rune contents" '\''
INTEGER        = "integer literal"
DECIMAL        = "decimal literal"
```

### Module & Declarations

```
=== Trivia ===
LineCommentTrivia : LINE_COMMENT [^TERM]* TERM

=== Module ===
Module            : ModuleHeader TERM+ ModuleBody
ModuleHeader      : '[[' ModulePath ']]'
ModulePath        : IDENT ['::' IDENT]*
ModuleBody        : [Declaration TERM]+

=== Types ===
TypeExpr          : PrimaryType TypePostfix?
TypePostfix       : OPTIONAL
                  | FALLIBLE

PrimaryType       : TypeRef
                  | FunctionShape
                  | StructShape
                  | UnionShape
                  | FlagsShape
                  | ProtocolShape
                  | ServiceShape

TypeRef           : IDENT GenericArgs?

FunctionShape     : '(' NEWLINE? CommaListOpt<TypeExpr> NEWLINE? ')' TypeExpr

TupleShape        : ShapeBlock<TUPLE_SHAPE, TypeExpr>
StructShape       : ShapeBlock<STRUCT_SHAPE, FieldDecl>
UnionShape        : ShapeBlock<UNION_SHAPE, VariantDecl>
FlagsShape        : ShapeBlock<FLAGS_SHAPE, IDENT>
ProtocolShape     : ShapeBlock<PROTOCOL_SHAPE, MethodDecl>
ServiceShape      : ShapeBlock<SERVICE_SHAPE, ServiceField>

FieldDecl         : IDENT ':' TypeExpr
VariantDecl       : IDENT [':' TypeExpr]?
MethodDecl        : IDENT ':' FunctionShape
ServiceField      : IDENT ':' TypeExpr

--- Generics ---
GenericParam      : IDENT [':' ConstraintList]?
ConstraintList    : TypeRef ['+' TypeRef]*
GenericArgs       : '[' CommaList<TypeExpr> ']'
GenericParams     : '[' CommaList<GenericParam> ']'

=== Declarations ===
Declaration            : ImportDecl
                       | ExportDecl
                       | TypeDecl
                       | ServiceDecl
                       | ServiceLifecycleDecl
                       | ServiceProtocolDecl
                       | ValueDecl
                       | FunctionDecl

ImportDecl             : IDENT BIND_MODULE ModulePath
ExportDecl             : '<<' NEWLINE? IDENT [TERM IDENT]* NEWLINE? '>>'
TypeDecl               : IDENT GenericParams? BIND_TYPE TypeExpr
ServiceDecl            : IDENT GenericParams? BIND_SERVICE ServiceInit
ServiceLifecycleDecl   : TypeRef NEWLINE? ServiceLifecycleBody
ServiceProtocolDecl    : TypeRef '<' CommaListOpt<TypeRef> '>' ParamList NEWLINE? ServiceMethodBody
ValueConstDecl         : IDENT ':' TypeExpr BIND_VALUE Expr
ValueMutDecl           : MUTABLE IDENT ':' TypeExpr BIND_VALUE Expr
ValueDecl              : ValueConstDecl
                       | ValueMutDecl

ServiceLifecycleBody   : '{' NEWLINE? [ServiceLifecycleMember TERM]* ServiceLifecycleMember? NEWLINE? '}'
ServiceLifecycleMember : ServiceCtorDecl
                       | ServiceDtorDecl

ServiceCtorDecl        : ParamList '@' '!' NEWLINE? FunctionBody
ServiceDtorDecl        : '~' ParamList TypeExpr NEWLINE? FunctionBody
ServiceMethodBody      : '{' NEWLINE? [ServiceMethodDecl TERM]* ServiceMethodDecl? NEWLINE? '}'
ServiceMethodDecl      : IDENT ':' ParamList TypeExpr NEWLINE? FunctionBody
FunctionDecl           : IDENT GenericParams? ':' ParamList TypeExpr NEWLINE? FunctionBody

ParamList              : '(' NEWLINE?  CommaListOpt<ParamDecl> NEWLINE? ')'
ParamDecl              : IDENT ':' TypeExpr
FunctionBody           : ARROW OperatorExpr
                       | Block
```

### Expressions

```
Expr             : FunctionLiteral
                 | MatchExpr
                 | OperatorExpr
                 | Block

FunctionLiteral  : LambdaParams FunctionBody
LambdaParams     : '(' NEWLINE? LambdaParamList? NEWLINE? ')'
LambdaParamList  : LambdaParam [',' LambdaParam]* [',']?
LambdaParam      : IDENT [':' TypeExpr]?

OperatorExpr     : PrefixOp* BinaryExpr
MatchExpr        : OperatorExpr ARROW MatchArmList
MatchArmList     : TerminatedList<MatchArm>
MatchArm         : Pattern GuardExpr? ARROW [Block | OperatorExpr]
GuardExpr        : GUARD OperatorExpr

BinaryExpr       : CallOrAccessExpr [InfixOp PrefixOp* CallOrAccessExpr]*

PrefixOp         : OP_NEG | OP_NOT
InfixOp          : OP_MULT | OP_DIV | OP_MOD
                 | OP_ADD | OP_SUB
                 | OP_LT | OP_GT | OP_LEQ | OP_GEQ
                 | OP_EQ | OP_NEQ
                 | OP_AND
                 | OP_OR

CallOrAccessExpr : PrimaryExpr AccessTail* PropagationOp?
AccessTail       : '.' IDENT
                 | '(' CommaListOpt<Expr> ')'
                 | CAST TypeExpr
PropagationOp    : OPTIONAL
                 | FALLIBLE

PrimaryExpr      : Construct
                 | IDENT
                 | Literal
                 | '(' Expr ')'

Literal          : STRING
                 | RUNE
                 | INTEGER
                 | DECIMAL

Construct        : TupleInit
                 | StructInit
                 | UnionInit
                 | FlagsInit
                 | ServiceInit
                 | OptionalInit
                 | FallibleInit

TupleInit        : ShapeHeader<TUPLE_SHAPE>   CommaListOpt<Expr>      ShapeClose
StructInit       : ShapeHeader<STRUCT_SHAPE>  CommaList<FieldInit>    ShapeClose
UnionInit        : ShapeHeader<UNION_SHAPE>   CommaList<VariantInit>  ShapeClose
FlagsInit        : ShapeHeader<FLAGS_SHAPE>   CommaList<IDENT>        ShapeClose
ServiceInit      : ShapeHeader<SERVICE_SHAPE> CommaListOpt<FieldInit> ShapeClose
OptionalInit     : OPT_SHAPE Expr? '}'
FallibleInit     : OK_SHAPE Expr '}'
                 | ERR_SHAPE Expr '}'

FieldInit        : IDENT BIND_VALUE Expr
VariantInit      : IDENT [BIND_VALUE Expr]?
```

### Statements

```
Statement        : Declaration
                 | MutAssignStmt
                 | ExprStmt

MutAssignStmt    : { MUTABLE IDENT } BIND_VALUE Expr

ExprStmt         : Expr
Block            : '{' [Statement TERM]* Expr? '}'
```

### Pattern Grammar

```
Pattern          : WildcardPattern
                 | BindingPattern
                 | LiteralPattern
                 | TuplePattern
                 | StructPattern
                 | VariantPattern
                 | FlagsPattern
                 | ListPattern
                 | OptionalPattern
                 | FalliblePattern

WildcardPattern  : '_'
BindingPattern   : IDENT
LiteralPattern   : Literal
TuplePattern     : '(' CommaListOpt<Pattern> ')'
StructPattern    : '(' CommaList<FieldPattern> ')'
VariantPattern   : IDENT ['(' CommaListOpt<Pattern> ')']?
FlagsPattern     : '(' CommaList<SignedFlag> ')'
                 | '(' CommaList<IDENT> ')'
ListPattern      : '(' ListElements? ')'
OptionalPattern  : '?(' Pattern? ')'
FalliblePattern  : '!(' Pattern  ')'
                 | '!!(' Pattern? ')'

FieldPattern     : IDENT BIND_VALUE Pattern
ListElements     : Pattern [',' Pattern]* [',' RestPattern]?
                 | RestPattern
RestPattern      : REST IDENT?
SignedFlag       : '+' IDENT
                 | '-' IDENT
```
