---
id: canon.functions
layer: canon
title: Function Declaration Forms
authors: ['trippwill', 'assistant']
updated: 2025-01-25
status: accepted
version: 0.1.0
canon:
  - spec/functions.md
  - spec/grammar.md
  - spec/sample.brim
---

# Function Declaration Forms

Brim distinguishes between three distinct forms for working with functions. Each has its own syntax and use case.

## 1. Function Type Declaration

Declares a **named type** that represents a function signature. This creates a type alias that can be used anywhere a type is expected.

**Syntax:**
```
Name := (ParamType, ...) ReturnType
```

**Examples:**
```brim
adder := (i32, i32) i32
handler := (str) unit
transform[T] := (T) T
```

**Key characteristics:**
- Uses type bind operator `:=`
- Parameters are **types only** (no parameter names)
- Creates a reusable type alias
- Can have generic parameters on the type name
- Right-hand side is a **function type expression**

## 2. Function Value Declaration

Declares a **named value** with a function type and binds it to an implementation.

**Syntax:**
```
name :FunctionType = implementation
name :(ParamType, ...) ReturnType = implementation
```

**Examples:**
```brim
-- Using a type alias
add_a :adder = |a, b|> a + b

-- Inline function type
add_b :(i32, i32) i32 = |a, b|> a + b

-- With generic type parameter
ident[T] :(T) T = |x|> x
```

**Key characteristics:**
- Uses value bind operator `=`
- Left side: `name :Type` (type can be alias or inline function type)
- Right side: function literal (`|params|> body`) or other expression
- Generic parameters go on the **name**, not the function type
- This is a **value binding**, same category as `count :i32 = 42`

## 3. Function Combined Declaration (Shorthand)

Declares and defines a function in one statement with **named parameters**. This is syntactic sugar that combines type and implementation. **Block body is required.**

**Syntax:**
```
name :(param :Type, ...) ReturnType { body }
```

**Examples:**
```brim
-- Block body (required form)
add_d :(a :i32, b :i32) i32 {
  fold(ints, a + b, |x, y|> x + y)
}

-- Single expression (still needs braces)
square :(x :i32) i32 { x * x }

-- With generics
apply[T] :(f :(T) T, x :T) T { f(x) }
```

**Key characteristics:**
- Uses colon `:` followed by parameter list with **named** parameters
- Parameters are `name :Type` pairs
- No bind operator between signature and body
- Body **must be** a block `{ ... }` (no arrow form)
- Generic parameters go on the **function name**
- This is **NOT** a value binding—it's its own declaration form

**Rationale for block-only:**
- Same token count as arrow form (`{ expr }` vs `=> expr`)
- Keeps `=>` exclusively for match expressions
- Clear visual distinction from value declarations with lambdas
- Simpler grammar and implementation
- Forces consistency—all combined declarations look the same

## Comparison Table

| Form | Bind Op | Params | Right Side | Use Case |
|------|---------|--------|------------|----------|
| Type Decl | `:=` | Types only | Type expression | Define reusable function type |
| Value Decl | `=` | In lambda | Expression (often lambda) | Bind function value to name |
| Combined | none | Named in sig | Block (required) | Define function with body |

## Complete Example

```brim
-- 1. Type declaration
BinaryOp := (i32, i32) i32

-- 2. Value declaration using type alias
add :BinaryOp = |a, b|> a + b

-- 2. Value declaration with inline type
sub :(i32, i32) i32 = |a, b|> a - b

-- 3. Combined declaration with block (required form)
mul :(a :i32, b :i32) i32 {
  a * b
}

-- 3. Combined declaration with single expression (still needs braces)
div :(a :i32, b :i32) i32 { a / b }
```

## Grammar Rules

```hgf
-- Type declaration (creates type alias)
TypeDecl        : IDENT GenericParams? BIND_TYPE TypeExpr

-- Value declaration (binds function value)
ValueDecl       : IDENT GenericParams? ':' TypeExpr BIND_VALUE Expr

-- Combined declaration (shorthand with named params)
FunctionDecl    : IDENT GenericParams? ':' ParamList ReturnType FunctionBody

-- Supporting productions
ParamList       : ParenListOpt<ParamDecl>
ParamDecl       : IDENT ':' TypeExpr
FunctionBody    : BlockExpr                    -- Block is required (no arrow form)
FunctionType    : ParenListOpt<TypeExpr> TypeExpr
```

## Parsing Disambiguation

The parser must distinguish between forms 2 and 3 after seeing `name :`. The key is what follows the colon:

- `name :(Type, ...)` → If next is `=`, it's a **value declaration** (form 2)
- `name :(param :Type, ...)` → If params have names, it's a **combined declaration** (form 3)
- `name :` followed by non-paren → value declaration with non-function type

Current implementation status:
- ✅ Form 1 (Type declarations) - fully implemented as TypeDeclaration
- ✅ Form 2 (Value declarations) - fully implemented as ValueDeclaration  
- ⚠️ Form 3 (Combined declarations) - **NOT YET IMPLEMENTED**

Form 3 requires a new FunctionDeclaration node type and parser logic to distinguish it from form 2.
