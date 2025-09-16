---
id: core.grammar
layer: core
title: Core Grammar (Draft)
authors: ['trippwill', 'assistant']
updated: 2025-09-14
status: draft
---

# Core Grammar (Draft)

This file centralizes the draft EBNF for Brim’s core. It is authoritative for syntax shape; topical specs link here for details and semantics. The grammar is LL(k≤4) by construction.

## Conventions
- Literals in quotes; nonterminals in CamelCase.
- “Token” names (e.g., Terminator) follow lexer naming; glyphs shown where useful.
- Statement separators are Terminators (newline or `;`).
- At statement start inside blocks, the parser selects between Binding and Expr by a small token prefix (see Prediction).

## Module
```ebnf
Module          ::= ModuleDirective ModuleBody Eob
ModuleDirective ::= ModuleHeader Terminator+
ModuleHeader    ::= '[[ ' ModulePath ' ]]'         -- no spaces in source; shown with padding here
ModulePath      ::= Ident ('::' Ident)*
ModuleBody      ::= (Member | Terminator)+
Member          ::= ExportDirective
                  | ImportDecl
                  | TypeDecl
                  | FunctionDecl
                  | ServiceDecl
                  | BlockStandalone                    -- standalone terminators/comments
```

## Imports & Exports
```ebnf
ExportDirective ::= '<<' Ident Terminator
ImportDecl      ::= Ident '::=' ModulePath Terminator   -- top-level only
```

## Declarations & Bindings
```ebnf
TypeDecl        ::= Ident GenericParams? ':=' TypeExpr Terminator

FunctionDecl    ::= BindingHeader ( '=' | '.=' ) FunctionExpr
                  | CombinedFuncHeader BlockExpr          -- const-only ergonomic form

BindingHeader   ::= Ident ':' FunctionType
CombinedFuncHeader ::= Ident ':' ParamDeclList ReturnType

-- General bindings inside blocks
ConstBind       ::= Ident ':' TypeExpr '=' Expr
VarBind         ::= Ident ':' TypeExpr '.=' Expr
LifeBind        ::= Ident '~=' Expr

GenericParams   ::= '[' (GenericParam (',' GenericParam)* (',')?)? ']'
GenericParam    ::= Ident (':' ConstraintList)?
ConstraintList  ::= ProtocolRef ('+' ProtocolRef)*
```

## Expressions
```ebnf
Expr            ::= SimpleExpr | BlockExpr | MatchExpr | CastExpr

SimpleExpr      ::= Ident
                  | Literal
                  | CallExpr
                  | ConstructorExpr
                  | MemberExpr
                  | FunctionExpr

BlockExpr       ::= '{' BlockBody '}'
BlockBody       ::= (Statement StmtSep+)* Expr?      -- final expression optional
Statement       ::= ConstBind | VarBind | LifeBind | Expr
StmtSep         ::= Terminator

MemberExpr      ::= NonLiteralPrimary '.' Ident ( '(' ArgList? ')' )?   -- field or method; no literal receiver
NonLiteralPrimary ::= Ident | '(' Expr ')' | ConstructorExpr | ListCtor
CallExpr        ::= Expr '(' ArgList? ')'
ArgList         ::= Expr (',' Expr)* (',')?

CastExpr        ::= Expr ':>' TypeExpr                      -- compile-time cast
```

## Function Types & Values
```ebnf
FunctionType    ::= '(' TypeList? ')' TypeExpr
TypeList        ::= TypeExpr (',' TypeExpr)* (',')?

FunctionExpr    ::= '(' ParamList? ')' BlockExpr            -- no arrow; literals use names-only params
ParamList       ::= Ident (',' Ident)* (',')?               -- names only in literals
ParamDeclList   ::= '(' ParamDecl (',' ParamDecl)* (',')? ')'   -- for combined headers
ParamDecl       ::= Ident ':' TypeExpr
ReturnType      ::= TypeExpr
```

## Match
```ebnf
MatchExpr       ::= Expr '=>' MatchArm+
MatchArm        ::= Pattern GuardOpt '=>' (Expr | BlockExpr)
GuardOpt        ::= /* empty */ | '??' Expr
```

## Aggregates & Constructors
```ebnf
-- Types (see topical specs for structure)
TypeExpr        ::= BuiltinType
                  | Ident GenericArgs?
                  | FunctionType
                  | AggregateShape
                  | ListType
                  | TypeExpr '?' | TypeExpr '!'

GenericArgs     ::= '[' TypeList? ']'
ListType        ::= 'list' '[' TypeExpr ']'

AggregateShape  ::= StructShape | UnionShape | NamedTupleShape | FlagsShape | ProtocolShape | ServiceShape
StructShape     ::= '%{' FieldTypes? '}'
FieldTypes      ::= FieldType (',' FieldType)* (',')?
FieldType       ::= Ident ':' TypeExpr
UnionShape      ::= '|{' VariantTypes? '}'
VariantTypes    ::= VariantType (',' VariantType)* (',')?
VariantType     ::= Ident (':' TypeExpr)?
NamedTupleShape ::= '#{' TypeList '}'
FlagsShape      ::= '&' Ident '{' Ident (',' Ident)* (',')? '}'
ProtocolShape   ::= '.{' MethodSigList? '}'
ServiceShape    ::= '^' '{' ProtoRef (',' ProtoRef)* (',')? '}'
ProtoRef        ::= Ident GenericArgs?

ConstructorExpr ::= TypeName '%{' FieldInits? '}'
                  | TypeName '|{' VariantInit '}'
                  | TypeName '#{' ExprList? '}'
                  | 'list' '{' ExprList? '}'
FieldInits      ::= FieldInit (',' FieldInit)* (',')?
FieldInit       ::= Ident '=' Expr
VariantInit     ::= Ident | Ident '=' Expr
ExprList        ::= Expr (',' Expr)* (',')?
```

## Patterns
```ebnf
Pattern         ::= Wildcard | ListPat | StructPat | NamedTuplePat | UnionPat | FlagsPat | Ident
Wildcard        ::= '_'

ListPat         ::= '(' ( ListItems? ) ')'
ListItems       ::= Pattern (',' Pattern)* (',' RestPat)? | RestPat
RestPat         ::= '..' Ident?

StructPat       ::= '(' FieldPat (',' FieldPat)* (',')? ')'
FieldPat        ::= Ident '=' Pattern | Ident        -- shorthand binds by field name

NamedTuplePat   ::= TypeName '#(' PatList ')'
PatList         ::= Pattern (',' Pattern)* (',')?

UnionPat        ::= Ident '(' Pattern? ')'

FlagsPat        ::= '(' FlagsExact | FlagsReqForbid ')'
FlagsExact      ::= Ident (',' Ident)* (',')?
FlagsReqForbid  ::= SignedFlag (',' SignedFlag)* (',')?
SignedFlag      ::= ('+' Ident) | ('-' Ident)
```

## Services & Protocols
```ebnf
-- Protocols are aggregate types declared via TypeDecl and ProtocolShape:
--   Proto[T?] := .{ method :(TypeList?) TypeExpr (, ...)? }
MethodSigList   ::= MethodSig (',' MethodSig)* (',')?
MethodSig       ::= Ident GenericParams? ':(' TypeList? ')' TypeExpr
-- Services: type declaration uses ServiceShape (protocol refs only):
--   Svc[T?] := ^{ ProtoRef (, ProtoRef)* (',')? }

-- Implementation blocks (term space) — structure only (bodies elided here):
ImplBlock       ::= ServiceRef ReceiverBinder '{' StateBlock Member* '}'
ServiceRef      ::= Ident GenericArgs?
ReceiverBinder  ::= '<' (Ident | '_') '>'
StateBlock      ::= '<' FieldDecl (',' FieldDecl)* (',')? '>' StmtSep
FieldDecl       ::= Ident ':' TypeExpr
Member          ::= CtorImpl | MethodImpl | DtorImpl
CtorImpl        ::= '^(' ParamDeclList? ')' BlockExpr
MethodImpl      ::= Ident '(' ParamDeclList? ')' ReturnType BlockExpr
DtorImpl        ::= '~()' BlockExpr
```

## Prediction (Statement Start)
At statement start (immediately after `{` or any Terminator):
- If the next significant tokens are `Identifier ':'`, parse a binding header (const/var/function/type) according to the binding operator that follows (`=`, `.=` or `:=`).
- Otherwise, parse an expression statement. Member/field access in expression space is `expr.member(...)` and does not compete with `:`.
