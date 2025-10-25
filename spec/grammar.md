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

<!--toc:start-->
- [Core Grammar](#core-grammar)
  - [Grammar Rules](#grammar-rules)
  - [Notation & Conventions](#notation-conventions)
  - [Lexical Overview](#lexical-overview)
    - [Binding & Module Surfaces](#binding-module-surfaces)
    - [Function & Member Surfaces](#function-member-surfaces)
    - [Control, Aggregates & Construction](#control-aggregates-construction)
  - [Operator Tokens & Precedence](#operator-tokens-precedence)
  - [Token Groups](#token-groups)
  - [Structural Templates](#structural-templates)
  - [Trivia](#trivia)
  - [Module Structure](#module-structure)
  - [Block Structure](#block-structure)
  - [Types](#types)
  - [Declarations](#declarations)
  - [Expressions](#expressions)
  - [Pattern Grammar](#pattern-grammar)
<!--toc:end-->

This document captures the canonical grammar for the Brim language.

NOTE: The grammar uses a very custom variant of EBNF called HGF (hinky grammar format). See the [Notations & Conventions](#notation-conventions) section below.

See `spec/unicode.md` for UTF‑8 handling, identifier normalization, and allowed whitespace characters.

## Grammar Rules

1. **Binary termination**: Every construct has exactly two forms: single-line with explicit newline terminator, or multi-line ending with structural delimiter.
   - A semicolon (`;`) is equivalent to a newline terminator; runs of `;` and newlines collapse to a single `TERM` token.
2. **Whitespace is not significant** unless contained in a terminal literal.
3. **Rules are written for clarity** to contributors, not generators.

## Notation & Conventions

Fences labeled hgf are in 'hinky grammar format', a highly custom variant of EBNF.


| Form            | Meaning / Style                          | Example                   |
| --------------- | ---------------------------------------- | ------------------------- |
| `"..."`         | Prose description                        | "identifier"              |
| `NAME = ...`    | Token rule (ALL_CAPS)                    | `LINE_COMMENT = '--'`     |
| `'...'`         | Literal token                            | `'{'`                     |
| `<NAME>`        | Character Name (avoid markdown escaping) | `<PIPE>`                  |
| `Name : ...`    | Production rule (PascalCase noun)        | `FunctionLiteral : …`     |
| `{...}`         | Adjacency required                       | `{TypeRef SERVICE_SHAPE}` |
| `[...]`         | Grouping                                 | `[Declaration TERM]+`     |
| `-`             | Character-class subtraction              | `[Letter - Digit]`        |
| `*` / `+` / `?` | Quantifiers 0+ 1+ 0/1                    | `T*`                      |
| Templates       | PascalCase `Template<A, B>`              | `CommaList<Expr>`         |
| `^`             | Exclusion                                | `[^TERM]*`                |


## Lexical Overview


| Surface                           | Category  | Notes                                           |
| --------------------------------- | --------- | ----------------------------------------------- |
| Identifier                        | lexical   | Unicode identifier; normalized once rules land. |
| Integer / Decimal / String / Rune | literal   | Standard literal forms                          |
| `-- comment`                      | trivia    | Runs to end of line                             |
| Terminator (`\n` or `;`)          | separator | Runs of newlines/semicolons collapse to one `TERM`. |


### Binding & Module Surfaces


| Surface                | Category         | Notes                                                 |
| ---------------------- | ---------------- | ----------------------------------------------------- |
| `Name[T?] := TypeExpr` | type declaration | Nominal when the right-hand side is a shape literal.  |
| `name :Type = expr`    | value binding    | Immutable binding; initialization required.           |
| `^name :Type = expr`   | mutable binding  | Mutable binding; initialization required.             |
| `name :Type ~= expr`   | lifecycle bind   | Service handle binding with destructor on scope exit. |
| `alias ::= pkg::ns`    | module alias     | Top-level only import binding.                        |
| `<< Name, Other >>`    | export list      | Module-level export of previously bound symbol.       |


### Function & Member Surfaces


| Surface                                    | Category     | Notes                                                        |
| ---------------------------------          | ------------ | ------------------------------------------------------------ |
| `(Type, ...) Ret`                          | type         | Function type expression.                                    |
| `Name := (Type, ...) Ret`                  | type decl    | Function type alias (params are types only).                 |
| `name :(Type, ...) Ret = expr`             | value decl   | Function value binding (often with lambda).                  |
| `name :(param :Type, ...) Ret { body }`    | combined     | Function declaration shorthand (params are named). **NOT IMPLEMENTED** |
| `name :(param :Type, ...) Ret => expr`     | combined     | Function declaration with arrow body. **NOT IMPLEMENTED**    |
| `|params|> expr` / `||> expr`              | expression   | Function literal with optional block body.                   |
| `expr.member(args?)`                       | expression   | Member access or method invocation.                          |
| `=[pkg::ns]=`                              | module       | Module header; must be first declaration.                    |


### Control, Aggregates & Construction


| Surface                         | Category   | Notes                                                      |
| ------------------------------- | ---------- | ---------------------------------------------------------- |
| `scrutinee =>`                  | control    | Starts a `MatchExpr`; see match productions.               |
| `?? expr`                       | control    | Pattern guard following `MatchArm`.                        |
| `expr :> Type`                  | conversion | Checked cast; see semantics for errors.                    |
| `Type := %{ field :Type, ... }` | type       | Aggregate shape declaration.                               |
| `Type%{ field = expr, ... }`    | construct  | Aggregate construction expression.                         |
| `seq[T]{ expr, ... }`           | construct  | Growable sequence literal.                                 |



## Operator Tokens & Precedence

The `BinaryExpr` production is intentionally flat. Precedence and associativity are defined in the table below, and enforced in parsing.

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
SEMICOLON      = ';'
LINE_COMMENT   = '--'

MOD_PATH_SEP   = '::'
MOD_PATH_OPEN  = '=['
MOD_PATH_CLOSE = ']='
EXPORT_OPEN    = '<<'
EXPORT_CLOSE   = '>>'
ARROW          = '=>'
LAMBDA_OPEN    = '|'
LAMBDA_CLOSE   = '|>'
LAMBDA_EMPTY   = '||>'
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
TUPLE_PATTERN  = '#('
STRUCT_PATTERN = '%('
UNION_PATTERN  = '|('
FLAGS_PATTERN  = '&('
SERVICE_PATTERN= '@('

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
TERM                     : {NEWLINE | SEMICOLON}+
CommaList<T>             : TERM? T [',' TERM? T]* ','? TERM?
CommaListOpt<T>          : CommaList<T>?

AggregateShape<H, T>     : H CommaList<T> '}'

TypedConstruct<H, T>     : { TypeRef AggregateShape<H, T> }
LinearConstruct<H, A, T> : { H A? '{' } CommaListOpt<T> '}'

ParenList<T>             : '(' CommaList<T> ')'
ParenListOpt<T>          : '(' CommaListOpt<T> ')'
BlockListOpt<T>          : '{' CommaListOpt<T> '}'
BracketListOpt<T>        : '[' CommaListOpt<T> ']'
AngleListOpt<T>          : '<' CommaListOpt<T> '>'

Terminated<T>            : T TERM

TerminatedList<T>        : TERM? Terminated<T>+
TerminatedListOpt<T>     : TerminatedList<T>?

ModuleRef                : IDENT [MOD_PATH_SEP IDENT]*
```

## Trivia

```hgf
LineCommentTrivia : LINE_COMMENT [^TERM]* TERM
```

## Module Structure

```hgf
Module       : ModuleHeader [TERM ModuleBody]?
ModuleHeader : MOD_PATH_OPEN ModuleRef MOD_PATH_CLOSE
ModuleBody   : TerminatedListOpt<Declaration>
```

## Block Expression

```hgf
BlockExpr      : '{' TerminatedListOpt<BlockEntry> Expr '}'
BlockEntry     : BindingDecl
               | Expr
```

## Types

```hgf
TypeExpr           : TypeCore TypeSuffix?

TypeCore           : TypeRef
                   | FunctionTypeExpr
                   | SeqTypeExpr
                   | BufTypeExpr
                   | AggregateTypeExpr

TypeSuffix         : OPTIONAL
                   | FALLIBLE

TypeRef            : QualifiedIdent GenericArgs?
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

QualifiedIdent     : IDENT ['.' IDENT]*

FunctionTypeExpr   : ParenListOpt<TypeExpr> TypeExpr
SeqTypeExpr        : { SEQ_KW '[' } TypeExpr ']'
BufTypeExpr        : { BUF_KW '[' } TypeExpr ['*' INTEGER]? ']'

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

BufTypeArgs             : '[' TypeExpr ['*' INTEGER]? ']'

GenericArgs             : BracketListOpt<TypeExpr>
GenericParams           : BracketListOpt<GenericParam>
GenericParam            : IDENT [':' ConstraintList]?
ConstraintList          : TypeRef ['+' TypeRef]*
```

## Declarations

```hgf
Declaration     : ImportDecl
                | TypeDecl
                | FunctionDecl
                | ExportDecl
                | ServiceDecl
                | BindingDecl

ImportDecl      : IDENT BIND_MODULE ModuleRef

TypeDecl        : IDENT GenericParams? BIND_TYPE TypeExpr
                  -- Examples:
                  --   adder := (i32, i32) i32
                  --   Result[T, E] := |{ Ok :T, Err :E }

FunctionDecl    : IDENT GenericParams? ':' ParamList TypeExpr FunctionBody
                  -- Examples:
                  --   add :(a :i32, b :i32) i32 { a + b }
                  --   get[T] :(x :T) T => x
                  -- NOTE: This form is NOT YET IMPLEMENTED

ParamList       : ParenListOpt<ParamDecl>
ParamDecl       : IDENT ':' TypeExpr

ExportDecl      : EXPORT_OPEN CommaList<IDENT> EXPORT_CLOSE

BindingDecl     : LocalBinding
                | MutAssign

LocalBinding    : IDENT ':' TypeExpr BIND_VALUE Expr
                  -- Examples:
                  --   count :i32 = 42
                  --   add :(i32, i32) i32 = |a, b|> a + b
                  --   ident[T] :(T) T = |x|> x
                | MUTABLE IDENT ':' TypeExpr BIND_VALUE Expr
                  -- Example:
                  --   ^counter :i32 = 0
                | IDENT ':' TypeExpr BIND_SERVICE Expr
                  -- Example:
                  --   db :DbService ~= connect("localhost")

MutAssign       : AssignTarget BIND_VALUE Expr
AssignTarget    : IDENT ['.' IDENT]*

-- Service Declarations
ServiceDecl     : ProtocolDecl
                | LifecycleDecl

ProtocolDecl    : { TypeRef AngleListOpt<TypeRef> Receiver? } MethodBody
Receiver        : '(' IDENT ':' SERVICE_HANDLE ')'
MethodBody      : BlockListOpt<MethodDecl>
MethodDecl      : IDENT ':' ParamList TypeExpr BlockExpr

LifecycleDecl   : TypeRef LifecycleBody
LifecycleBody   : BlockListOpt<LifecycleMember>
LifecycleMember : ServiceCtorDecl
                | ServiceDtorDecl

ServiceCtorDecl : ParamList '@' '!' BlockExpr
ServiceDtorDecl : '~' ParamList TypeExpr BlockExpr
```

## Expressions

```hgf
Expr             : BinaryExpr
                 | MatchExpr
                 | FunctionLiteral
                 | BlockExpr

FunctionLiteral  : LAMBDA_EMPTY LambdaBody
                 | LAMBDA_OPEN LambdaParams LAMBDA_CLOSE LambdaBody
LambdaParams     : IDENT (',' IDENT)*
LambdaBody       : Expr
                 | BlockExpr
FunctionBody     : ARROW Expr
                 | BlockExpr

MatchExpr        : BinaryExpr ARROW MatchArmList
MatchArmList     : TerminatedList<MatchArm>
MatchArm         : Pattern GuardExpr? ARROW MatchTarget
MatchTarget      : Expr
GuardExpr        : GUARD BinaryExpr

UnaryExpr        : PrefixOp* CallOrAccessExpr
BinaryExpr       : UnaryExpr [InfixOp UnaryExpr]*

PrefixOp         : OP_NEG | OP_NOT
InfixOp          : OP_MULT | OP_DIV | OP_MOD
                 | OP_ADD | OP_SUB
                 | OP_LT | OP_GT | OP_LEQ | OP_GEQ
                 | OP_EQ | OP_NEQ
                 | OP_AND
                 | OP_OR

CallOrAccessExpr : PrimaryExpr [AccessTail | PropagationOp]*
AccessTail       : '.' IDENT
                 | CallArgs
                 | CAST TypeExpr

PropagationOp    : OPTIONAL | FALLIBLE
CallArgs         : ParenListOpt<Expr>

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
                 | TypedConstruct<FLAGS_SHAPE, IDENT>
                 | UnionExpr
                 | ServiceExpr
                 | OPT_SHAPE Expr? '}'
                 | OK_SHAPE Expr '}'
                 | ERR_SHAPE Expr '}'
                 | UNIT_KW '{}'
                 | LinearConstruct<SEQ_KW, GenericArgs, Expr>
                 | LinearConstruct<BUF_KW, BufTypeArgs, Expr>


UnionExpr        : { TypeRef UNION_SHAPE } VariantInit '}'

ServiceExpr      : TypedConstruct<SERVICE_SHAPE, FieldInit>
                 | AggregateShape<SERVICE_SHAPE, FieldInit>

FieldInit        : IDENT BIND_VALUE Expr
VariantInit      : IDENT [BIND_VALUE Expr]?
```

## Pattern Grammar

```hgf
Pattern             : WildcardPattern
                    | BindingPattern
                    | LiteralPattern
                    | TuplePattern
                    | StructPattern
                    | VariantPattern
                    | FlagsPattern
                    | ServicePattern
                    | ListPattern
                    | OptionalPattern
                    | FalliblePattern

WildcardPattern     : '_'
BindingPattern      : IDENT
LiteralPattern      : Literal
TuplePattern        : TUPLE_PATTERN CommaListOpt<Pattern> ')'
StructPattern       : STRUCT_PATTERN CommaListOpt<FieldPattern> ')'
VariantPattern      : UNION_PATTERN IDENT VariantPatternTail? ')'
FlagsPattern        : FLAGS_PATTERN CommaListOpt<FlagsPatternEntry> ')'
ServicePattern      : SERVICE_PATTERN CommaListOpt<ServicePatternEntry> ')'
ListPattern         : '(' ListElements? ')'
OptionalPattern     : '?(' Pattern? ')'
FalliblePattern     : '!(' Pattern  ')'
                    | '!!(' Pattern? ')'

FieldPattern        : IDENT BIND_VALUE Pattern
ListElements        : Pattern [',' Pattern]* [',' RestPattern]?
                    | RestPattern
RestPattern         : REST IDENT?
SignedFlag          : '+' IDENT
                    | '-' IDENT

VariantPatternTail  : ParenListOpt<Pattern>
FlagsPatternEntry   : SignedFlag
                    | IDENT

ServicePatternEntry : IDENT ':' TypeRef
```
