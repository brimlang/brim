---
id: core.grammar
layer: core
title: Core Grammar
authors: ['trippwill', 'assistant']
updated: 2025-09-21
status: accepted
version: 0.1.0
---

# Core Grammar

This document captures the canonical grammar for the Brim language.

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

UserService := @{ perms :Perms; audit :seq[str] }

UserService {
  (seed :Perms) @! {
    @{ perms = seed; audit = seq{} }
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
    svc.audit = concat(svc.audit, seq{"add:" ++ name})
    User%{ name = name; perms = Area|{ Admin = svc.perms } }
  }

  ban :(user :User) unit {
    svc.audit = concat(svc.audit, seq{"ban:" ++ user.name})
  }

  login :(uname :str, pass :str) User! {
    check(uname, pass, svc.perms) =>
      true  => User%{ name = uname; perms = Area|{ Client = svc.perms } }
      false => !!{ mkerr("Not Authorized") }
  }

  logout :(user :User) unit {
    svc.audit = concat(svc.audit, seq{"logout:" ++ user.name})
  }
}

-- Function Type Declarations and Definitions
adder := (i32, i32) i32
add_a :adder = (a, b) => a + b
add_b :(a :i32, b :i32) i32 => a + b
add_d :(a :i32, b :i32) i32 { a + b }

fold :(xs :seq[i32], seed :i32, f :(i32, i32) i32) i32 {
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

| Tier | Operators                   | Assoc |
| ---- | --------------------------- | ----- |
| 90   | OP_NOT OP_NEG               | R     |
| 80   | OP_MULT OP_DIV OP_MOD       | L     |
| 75   | OP_ADD OP_SUB               | L     |
| 70   | OP_LT OP_GT OP_LEQ OP_GEQ   | N     |
| 65   | OP_EQ OP_NEQ                | N     |
| 50   | OP_AND                      | L     |
| 45   | OP_OR                       | L     |


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
SEQ_KW         = 'seq'
BUF_KW         = 'buf'
VOID_KW        = 'void'
UNIT_KW        = 'unit'
BOOL_KW        = 'bool'
BOOL_TRUE      = 'true'
BOOL_FALSE     = 'false'
STR_KW         = 'str'
RUNE_KW        = 'rune'
ERR_KW         = 'err'
I8_KW          = 'i8'
I16_KW         = 'i16'
I32_KW         = 'i32'
I64_KW         = 'i64'
U8_KW          = 'u8'
U16_KW         = 'u16'
U32_KW         = 'u32'
U64_KW         = 'u64'

REST           = '..'
GUARD          = '??'
ELSE           = '::'  -- For future ternary expr
OPTIONAL       = '?'   -- postfix propagation-on-success marker
FALLIBLE       = '!'   -- postfix propagation-on-failure marker

STRING         = '"' "allowed string contents" '"'
RUNE           = ''' "allowed rune contents" '''
INTEGER        = "integer literal"
DECIMAL        = "decimal literal"
```

#### Lexical Overview

| Surface                           | Category      | Notes                                                                         |
| -----------------------------     | ------------- | -------                                                                       |
| Identifier                        | lexical       | Unicode identifier; normalized once rules land.                               |
| Integer / Decimal / String / Rune | literal       | Standard literal forms; see `spec/core/numeric_literals.md` for typing rules. |
| `-- comment`                      | trivia        | Runs to end of line; collapses into a single trivia token.                    |
| Terminator (`\n`, `;`)            | separator     | Consecutive newlines/semicolons collapse to one `TERM`.                       |

Compound glyphs (operators, shape openers, etc.) use longest-match lexing. Sequences up to three characters (e.g., `::=`, `<<`, `|{`, `*{`, `!!{`, `[[`, `]]`, `.{`, `@{`, `??`) are single tokens today. If a longer sequence is introduced, it must extend this table; existing multi-char tokens never regress to shorter fragments.

See `spec/unicode.md` for UTF‑8 handling, identifier normalization, and allowed whitespace characters.

#### Binding & Module Surfaces

| Surface                           | Category          | Notes                                                 |
| --------------------------------- | ----------------- | -------                                               |
| `Name[T?] := TypeExpr`            | type declaration  | Nominal when the right-hand side is a shape literal.  |
| `name :Type = expr`               | value binding     | Immutable binding; initialization required.           |
| `^name :Type = expr`              | mutable binding   | Mutable binding.                                      |
| `name ~= expr`                    | lifecycle bind    | Service handle binding with destructor on scope exit. |
| `alias ::= pkg::ns`               | module alias      | Top-level only import binding.                        |
| `<< Name >>`                      | export list       | Module-level export of previously bound symbol.       |

#### Function & Member Surfaces

| Surface                                    | Category     | Notes                                                               |
| ---------------------------------          | ------------ | -------                                                             |
| `(Type, ...) Ret`                          | type         | Function type.                                                      |
| `(params) => expr` / `(params) => { ... }` | expression   | Function literal with optional block body.                          |
| `f :(Type, ...) Ret = ...`                 | declaration  | Named function/value binding.                                       |
| `expr.member(args?)`                       | expression   | Member access or method invocation; literals do not expose members. |
| `pkg::ns::Name`                            | path         | Namespace-qualified identifier.                                     |
| `[[pkg::ns]]`                              | module       | Module header; must be first declaration.                           |

#### Control, Aggregates, and Patterns

| Surface                                          | Category     | Notes                                                                   |
| ---------------------------------                | ------------ | -------                                                                 |
| `scrutinee =>`                                   | control      | Starts a `MatchExpr`; see match productions.                            |
| `?? expr`                                        | control      | Pattern guard following `MatchArm`.                                     |
| `expr :> Type`                                   | conversion   | Checked cast; see semantics for errors.                                 |
| `Type := %{ field :Type, ... }`                  | type         | Struct shape declaration.                                               |
| `Type%{ field = expr, ... }`                     | construct    | Struct construction literal.                                            |
| `Type := |{ Variant :Type?, ... }`              | type         | Union shape declaration.                                                |
| `Type|{ Variant (= expr)? }`                    | construct    | Union construction (with optional payload).                             |
| `seq{ expr, ... }`                               | construct    | Growable sequence literal (element type inferred or via `seq[T]{...}`). |
| `buf[T; N]{ expr, ... }`                         | construct    | Fixed-length buffer literal; element count must equal `N`.              |
| `Type := @{ field :Type, ... }`                  | type         | Service field layout declaration.                                       |
| `(pattern elements)` / `_` / `Variant(pattern?)` | pattern      | See pattern grammar for allowed forms.                                  |


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
                  | BOOL_KW
                  | UNIT_KW
                  | VOID_KW
                  | STR_KW
                  | RUNE_KW
                  | ERR_KW
                  | I8_KW
                  | I16_KW
                  | I32_KW
                  | I64_KW
                  | U8_KW
                  | U16_KW
                  | U32_KW
                  | U64_KW
                  | SEQ_KW GenericArgs?
                  | BufType

BufType           : BUF_KW BufTypeArgs
BufTypeArgs       : '[' TypeExpr ';' BufLenExpr ']'
BufLenExpr        : IDENT
                  | INTEGER

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
                 | BOOL_TRUE
                 | BOOL_FALSE

Construct        : TupleInit
                 | StructInit
                 | UnionInit
                 | FlagsInit
                 | ServiceInit
                 | OptionalInit
                 | FallibleInit
                 | UnitInit
                 | SeqInit
                 | BufInit

TupleInit        : ShapeHeader<TUPLE_SHAPE>   CommaListOpt<Expr>      ShapeClose
StructInit       : ShapeHeader<STRUCT_SHAPE>  CommaList<FieldInit>    ShapeClose
UnionInit        : ShapeHeader<UNION_SHAPE>   CommaList<VariantInit>  ShapeClose
FlagsInit        : ShapeHeader<FLAGS_SHAPE>   CommaList<IDENT>        ShapeClose
ServiceInit      : ShapeHeader<SERVICE_SHAPE> CommaListOpt<FieldInit> ShapeClose
OptionalInit     : OPT_SHAPE Expr? '}'
FallibleInit     : OK_SHAPE Expr '}'
                 | ERR_SHAPE Expr '}'

UnitInit         : UNIT_KW '{' NEWLINE? '}'
SeqInit          : SEQ_KW GenericArgs? '{' CommaListOpt<Expr> '}'
BufInit          : BUF_KW BufTypeArgs? '{' CommaListOpt<Expr> '}'

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
