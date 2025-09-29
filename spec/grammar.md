---
id: canon.grammar
layer: canon
title: Core Grammar
authors: ['trippwill', 'assistant']
updated: 2025-01-22
status: accepted
version: 0.1.0
canon:
  - spec/grammar.md
  - spec/unicode.md
  - spec/fundamentals.md
  - spec/sample.brim
---

# Core Grammar

This document captures the canonical grammar for the Brim language.

NOTE: The grammar uses a very custom variant of EBNF called HGF (hinky grammar format). See the [Notations & Conventions](#notation-conventions) section below.

## Grammar Rules

1. **Binary termination**: Every construct has exactly two forms: single-line with explicit newline terminator, or multi-line ending with structural delimiter.
2. **Whitespace is not significant** unless contained in a terminal literal.
3. **Rules are written for clarity** to contributors, not generators.

## Notation & Conventions

Fences labeled hgf are in 'hinky grammar format', a highly custom variant of EBNF.


| Form            | Meaning / Style                                | Example                   |
| --------------- | ---------------------------------------------- | ------------------------- |
| `"..."`         | Prose description                              | "identifier"              |
| `NAME = ...`    | Token rule (ALL_CAPS)                          | `LINE_COMMENT = '--'`     |
| `'...'`         | Literal token                                  | `'{'`                     |
| `<NAME>`        | Character Name (avoid markdown escaping)       | `<PIPE>`                  |
| `Name : ...`    | Production rule (PascalCase noun)              | `FunctionLiteral : …`     |
| `{...}`         | Adjacency required                             | `{TypeRef SERVICE_SHAPE}` |
| `[...]`         | Grouping                                       | `[Declaration TERM]+`     |
| `<PIPE>`        | Alternation                                    | `Expr <PIPE> Block`       |
| `-`             | Character-class subtraction                    | `[Letter - Digit]`        |
| `*` / `+` / `?` | Quantifiers (zero-or-more / one-or-more / opt) | `T*`                      |
| Templates       | PascalCase `Template<A, B>`                    | `CommaList<Expr>`         |
| `^`             | Exclusion                                      | `[^TERM]*`                |


## Lexical Overview


| Surface                           | Category      | Notes                                                                         |
| -----------------------------     | ------------- | -------                                                                       |
| Identifier                        | lexical       | Unicode identifier; normalized once rules land.                               |
| Integer / Decimal / String / Rune | literal       | Standard literal forms; see `spec/core/numeric_literals.md` for typing rules. |
| `-- comment`                      | trivia        | Runs to end of line; collapses into a single trivia token.                    |
| Terminator (`\n`)                 | separator     | Consecutive newlines collapse to one `TERM`.                                  |

Compound glyphs (operators, shape openers, etc.) use longest-match lexing. Sequences up to three characters (e.g., `::=`, `<<`, `|{`, `*{`, `!!{`, `[[`, `]]`, `.{`, `@{`, `??`) are single tokens today. If a longer sequence is introduced, it must extend this table; existing multi-char tokens never regress to shorter fragments.

See `spec/unicode.md` for UTF‑8 handling, identifier normalization, and allowed whitespace characters.

### Binding & Module Surfaces


| Surface               | Category          | Notes                                                 |
| ----------------------| ----------------- | ------------------------------------------------------|
| `Name[T?] := TypeExpr`| type declaration  | Nominal when the right-hand side is a shape literal.  |
| `name :Type = expr`   | value binding     | Immutable binding; initialization required.           |
| `^name :Type = expr`  | mutable binding   | Mutable binding; initialization required.             |
| `name ~= expr`        | lifecycle bind    | Service handle binding with destructor on scope exit. |
| `alias ::= pkg::ns`   | module alias      | Top-level only import binding.                        |
| `<< Name >>`          | export list       | Module-level export of previously bound symbol.       |


### Function & Member Surfaces


| Surface                                    | Category     | Notes                                      |
| ---------------------------------          | ------------ | -------                                    |
| `(Type, ...) Ret`                          | type         | Function type.                             |
| `(params) => expr` / `(params) => { ... }` | expression   | Function literal with optional block body. |
| `f :(Type, ...) Ret = ...`                 | declaration  | Named function/value binding.              |
| `expr.member(args?)`                       | expression   | Member access or method invocation.        |
| `pkg::ns::Name`                            | path         | Namespace-qualified identifier.            |
| `[[pkg::ns]]`                              | module       | Module header; must be first declaration.  |


### Control, Aggregates, and Patterns


| Surface                             | Category   | Notes                                                                   |
| ----------------------------------- | ---------- | ----------------------------------------------------------------------- |
| `scrutinee =>`                      | control    | Starts a `MatchExpr`; see match productions.                            |
| `?? expr`                           | control    | Pattern guard following `MatchArm`.                                     |
| `expr :> Type`                      | conversion | Checked cast; see semantics for errors.                                 |
| `Type := %{ field :Type, ... }`     | type       | Struct shape declaration.                                               |
| `Type%{ field = expr, ... }`        | construct  | Struct construction literal.                                            |
| `Type := \|{ Variant :Type?, ... }` | type       | Union shape declaration.                                                |
| `Type\|{ Variant (= expr)? }`       | construct  | Union construction (with optional payload).                             |
| `seq[T]{ expr, ... }`               | construct  | Growable sequence literal (element type inferred or via `seq[T]{...}`). |
| `buf[T; N]{ expr, ... }`            | construct  | Fixed-length buffer literal; element count must equal `N`.              |
| `Type := @{ field :Type, ... }`     | type       | Service field layout declaration.                                       |
| --                                  | pattern    | See pattern grammar for allowed forms.                                  |


## Operator Tokens & Precedence

Infix Tokens:

```hgf
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

```hgf
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


## Token Groups

```hgf
IDENT          = "identifier"
NEWLINE        = '\n'
TERM           = NEWLINE
LINE_COMMENT   = '--'

MOD_PATH_SEP  = '::'
MOD_PATH_OPEN = '[['
MOD_PATH_CLOSE= ']]'
ARROW          = '=>'
CAST           = ':>'
MUTABLE        = '^'
SERVICE_HANDLE = '@'
DESTRUCTOR     = '~'

-- Binding Tokens
BIND_MODULE    = '::='
BIND_TYPE      = ':='
BIND_SERVICE   = '~='
BIND_VALUE     = '='

-- Aggregate Shape Tokens
TUPLE_SHAPE    = '#{'
STRUCT_SHAPE   = '%{'
UNION_SHAPE    = '|{'
FLAGS_SHAPE    = '&{'
PROTOCOL_SHAPE = '.{'
SERVICE_SHAPE  = '@{'
OPT_SHAPE      = '?{'
OK_SHAPE       = '!{'
ERR_SHAPE      = '!!{'

-- Type Keywords
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

-- Pattern Tokens
REST           = '..'
GUARD          = '??'

-- Propagation Tokens
OPTIONAL       = '?'
FALLIBLE       = '!'

-- Literal Tokens
STRING         = '"' "allowed string contents" '"'
RUNE           = ''' "allowed rune contents" '''
INTEGER        = "integer literal"
DECIMAL        = "decimal literal"
```


## Structural Templates

Capture common structural patterns in templates to reduce boilerplate.

```hgf
CommaList<T>         : T [',' T]* [',']?
CommaListOpt<T>      : [CommaList<T>]?
CommaLineList<T>     : T [',']?
                     | T ',' [TERM T ',']* TERM T [',']?

FieldList<T>         : T [TERM T]* [TERM]?
BlockList<T>         : '{' TERM CommaLineList<T> TERM '}'

AggregateShape<H, T> : H [ CommaList<T> '}' | TERM CommaLineList<T> TERM '}' ]
TypedConstruct<H, T> : { TypeRef AggregateShape<H, T> }

ParenList<T>         : '(' CommaList<T> ')'
                     | '(' TERM CommaLineList<T> TERM ')'

ParenListOpt<T>      : '(' CommaListOpt<T> ')'
                     | '(' TERM CommaLineList<T> TERM ')'

Terminated<T>        : T TERM
TerminatedList<T>    : Terminated<T> [Terminated<T>]*

ModuleRef            : IDENT [MOD_PATH_SEP IDENT]*
```

## Trivia

```hgf
LineCommentTrivia : LINE_COMMENT [^TERM]* TERM
```

## Module Structure

```hgf
Module            : ModuleHeader [TERM ModuleBody]?
ModuleHeader      : MOD_PATH_OPEN ModuleRef MOD_PATH_CLOSE
ModuleBody        : TerminatedList<Declaration>?
```

## Block Structure

```hgf
Block          : '{' TerminatedList<BlockEntry>? Expr '}'

BlockEntry     : BindingDecl
               | Expr
```

## Types

```hgf
TypeExpr          : TypePrimary TypeSuffix?

TypePrimary       : TypeRef
                  | FunctionTypeExpr
                  | SeqTypeExpr
                  | BufTypeExpr
                  | AggregateTypeExpr

TypeSuffix        : OPTIONAL
                  | FALLIBLE

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
                  | SERVICE_HANDLE

FunctionTypeExpr   : '(' CommaListOpt<TypeExpr> ')' TypeExpr
SeqTypeExpr        : { SEQ_KW '[' } TypeExpr ']'
BufTypeExpr        : { BUF_KW '[' } TypeExpr [',' INTEGER]? ']'

AggregateTypeExpr  : AggregateShape<STRUCT_SHAPE, FieldDeclaration>
                   | AggregateShape<UNION_SHAPE, UnionVariantDeclaration>
                   | AggregateShape<TUPLE_SHAPE, NamedTupleElement>
                   | AggregateShape<FLAGS_SHAPE, FlagMemberDeclaration>
                   | AggregateShape<PROTOCOL_SHAPE, MethodSignature>
                   | AggregateShape<SERVICE_SHAPE, FieldDeclaration>

UnionVariantDeclaration : IDENT [':' TypeExpr]?
FlagMemberDeclaration   : IDENT
MethodSignature         : IDENT ':' ParamList TypeExpr
NamedTupleElement       : IDENT ':' TypeExpr
FieldDeclaration        : IDENT ':' TypeExpr

GenericArgs        : '<' CommaListOpt<TypeExpr> '>'
GenericParams      : '<' CommaListOpt<GenericParam> '>'
GenericParam       : IDENT [':' ConstraintList]?
ConstraintList     : TypeRef ['+' TypeRef]*
```

## Declarations

```hgf
Declaration  : ImportDecl
             | TypeDecl
             | FuncDecl
             | ExportDecl
             | ServiceDecl
             | BindingDecl

ImportDecl   : IDENT BIND_MODULE ModuleRef
TypeDecl     : TypeRef BIND_TYPE TypeExpr
FuncDecl     : IDENT GenericParams? ':' ParamList TypeExpr FunctionBody

ParamList    : ParenListOpt<ParamDecl>
ParamDecl    : IDENT ':' TypeExpr

ExportDecl   : '<<' ExportList '>>'
ExportList   : CommaListOpt<IDENT>
             | TERM CommaLineList<IDENT> TERM

BindingDecl  : LocalBinding
             | MutAssign

LocalBinding : IDENT ':' TypeExpr BIND_VALUE Expr
             | MUTABLE IDENT ':' TypeExpr BIND_VALUE Expr

MutAssign    : AssignTarget BIND_VALUE Expr
AssignTarget : IDENT ('.' IDENT)*

-- Service Declarations
ServiceDecl   : ProtocolDecl
              | LifecycleDecl

ProtocolDecl  : { TypeRef '<' CommaListOpt<TypeRef> '>' Receiver? } MethodBody
Receiver      : '(' IDENT ':' SERVICE_HANDLE ')'
MethodBody    : BlockList<MethodDecl>
MethodDecl    : IDENT ':' ParamList TypeExpr Block

LifecycleDecl   : TypeRef LifecycleBody
LifecycleBody   : BlockList<LifecycleMember>
LifecycleMember : ServiceCtorDecl
                | ServiceDtorDecl

ServiceCtorDecl : ParamList '@' '!' Block
ServiceDtorDecl : '~' ParamList TypeExpr Block
```

## Expressions

```hgf
Expr             : Literal
                 | IDENT
                 | CallOrAccessExpr
                 | ConstructExpr
                 | '(' Expr ')'
                 | Block
                 | MatchExpr
                 | FunctionLiteral

FunctionLiteral  : LambdaParams FunctionBody
LambdaParams     : ParenListOpt<IDENT>
FunctionBody     : ARROW Expr
                 | Block

OperatorExpr     : PrefixOp* BinaryExpr
MatchExpr        : OperatorExpr ARROW MatchArmList
MatchArmList     : FieldList<MatchArm>
MatchArm         : Pattern GuardExpr? ARROW MatchTarget
MatchTarget      : Expr
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
                 | CallArgs
                 | CAST TypeExpr
CallArgs         : ParenListOpt<Expr>
PropagationOp    : OPTIONAL
                 | FALLIBLE

PrimaryExpr      : IDENT
                 | Literal
                 | ConstructExpr
                 | '(' Expr ')'

Literal          : STRING
                 | RUNE
                 | INTEGER
                 | DECIMAL
                 | BOOL_TRUE
                 | BOOL_FALSE

ConstructExpr    : TypedConstruct<TUPLE_SHAPE, Expr>
                 | TypedConstruct<STRUCT_SHAPE, FieldInit>
                 | UnionExpr
                 | TypedConstruct<FLAGS_SHAPE, IDENT>
                 | ServiceExpr
                 | OPT_SHAPE Expr? '}'
                 | OK_SHAPE Expr '}'
                 | ERR_SHAPE Expr '}'
                 | UNIT_KW '{}'
                 | { SEQ_KW GenericArgs? } [ CommaListOpt<Expr> '}'
                                           | TERM CommaLineList<Expr> TERM '}']
                 | { BUF_KW BufTypeArgs? } [ CommaListOpt<Expr> '}'
                                           | TERM CommaLineList<Expr> TERM '}']

UnionExpr        : TypeRef UNION_SHAPE VariantInit '}'

ServiceExpr      : TypedConstruct<SERVICE_SHAPE, FieldInit>
                 | AggregateShape<SERVICE_SHAPE, FieldInit>

FieldInit        : IDENT BIND_VALUE Expr
VariantInit      : IDENT [BIND_VALUE Expr]?
```

## Pattern Grammar

```hgf
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
TuplePattern     : ParenListOpt<Pattern>
StructPattern    : ParenList<FieldPattern>
VariantPattern   : IDENT [ParenListOpt<Pattern> ]?
FlagsPattern     : ParenList<SignedFlag>
                 | ParenList<IDENT>
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
