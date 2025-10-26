using Brim.Parse.Green;

namespace Brim.Parse.Tests;

/// <summary>
/// Comprehensive tests for pattern parsing across all pattern types.
/// Covers literal, binding, wildcard, tuple, struct, variant, flags, service,
/// list, optional, fallible patterns, and nested pattern combinations.
/// </summary>
public class PatternParsingTests
{
    const string Header = "=[test::module]=\n";

    [Fact]
    public void LiteralPattern_Integer_ParsesCorrectly()
    {
        // Arrange: Create a simple match expression with a literal integer pattern
        string src = Header + "test :(i32) i32 = |x|> x => 42 => 99\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a LiteralPattern
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        LiteralPattern pattern = Assert.IsType<LiteralPattern>(arm.Pattern);
        Assert.Equal(SyntaxKind.IntToken, pattern.Literal.Kind);
    }

    [Fact]
    public void BindingPattern_Identifier_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a binding pattern (identifier)
        string src = Header + "test :(i32) i32 = |x|> x => n => n\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a BindingPattern
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        BindingPattern pattern = Assert.IsType<BindingPattern>(arm.Pattern);
        Assert.Equal(SyntaxKind.IdentifierToken, pattern.Identifier.Kind);
    }

    [Fact]
    public void LiteralPattern_String_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a string literal pattern
        string src = Header + "test :(str) i32 = |s|> s => \"hello\" => 1\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a LiteralPattern with string token
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        LiteralPattern pattern = Assert.IsType<LiteralPattern>(arm.Pattern);
        Assert.Equal(SyntaxKind.StrToken, pattern.Literal.Kind);
    }

    [Fact]
    public void VariantPattern_NoPayload_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a variant pattern without payload
        string src = Header + "test :(Option[i32]) i32 = |opt|> opt => |(None) => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a VariantPattern
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        VariantPattern pattern = Assert.IsType<VariantPattern>(arm.Pattern);
        Assert.Equal(SyntaxKind.IdentifierToken, pattern.VariantName.Kind);
        Assert.Null(pattern.Tail);
    }

    [Fact]
    public void TuplePattern_Empty_ParsesCorrectly()
    {
        // Arrange: Create a match expression with an empty tuple pattern
        string src = Header + "test :(unit) i32 = |u|> u => #() => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a TuplePattern with no elements
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        TuplePattern pattern = Assert.IsType<TuplePattern>(arm.Pattern);
        Assert.Empty(pattern.Patterns.Elements);
    }

    [Fact]
    public void TuplePattern_OneElement_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a single-element tuple pattern
        string src = Header + "test :((i32,)) i32 = |t|> t => #(n) => n\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a TuplePattern with one element
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        TuplePattern pattern = Assert.IsType<TuplePattern>(arm.Pattern);
        Assert.Single(pattern.Patterns.Elements);
        Assert.IsType<BindingPattern>(pattern.Patterns.Elements[0].Node);
    }

    [Fact]
    public void TuplePattern_TwoElements_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a two-element tuple pattern
        string src = Header + "test :((i32, i32)) i32 = |t|> t => #(a, b) => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a TuplePattern with two elements
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        TuplePattern pattern = Assert.IsType<TuplePattern>(arm.Pattern);
        Assert.Equal(2, pattern.Patterns.Elements.Length);
        Assert.IsType<BindingPattern>(pattern.Patterns.Elements[0].Node);
        Assert.IsType<BindingPattern>(pattern.Patterns.Elements[1].Node);
    }

    [Fact]
    public void TuplePattern_Nested_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a nested tuple pattern
        string src = Header + "test :(((i32, i32), i32)) i32 = |t|> t => #(#(a, b), c) => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as nested TuplePatterns
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        TuplePattern outer = Assert.IsType<TuplePattern>(arm.Pattern);
        Assert.Equal(2, outer.Patterns.Elements.Length);

        TuplePattern inner = Assert.IsType<TuplePattern>(outer.Patterns.Elements[0].Node);
        Assert.Equal(2, inner.Patterns.Elements.Length);
        Assert.IsType<BindingPattern>(inner.Patterns.Elements[0].Node);
        Assert.IsType<BindingPattern>(inner.Patterns.Elements[1].Node);

        Assert.IsType<BindingPattern>(outer.Patterns.Elements[1].Node);
    }

    [Fact]
    public void WildcardPattern_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a wildcard pattern
        // Note: _ is lexed as an identifier; semantic analysis distinguishes wildcards from bindings
        string src = Header + "test :(i32) i32 = |x|> x => _ => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a BindingPattern (wildcard semantics come later)
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        BindingPattern pattern = Assert.IsType<BindingPattern>(arm.Pattern);
        Assert.Equal(SyntaxKind.IdentifierToken, pattern.Identifier.Kind);
    }

    [Fact]
    public void VariantPattern_WithPayload_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a variant pattern with tuple payload
        // Syntax is |(VariantName(pattern)) with nested parens for the payload
        string src = Header + "test :(Option[i32]) i32 = |opt|> opt => |(Some(n)) => n\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a VariantPattern with payload
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        VariantPattern pattern = Assert.IsType<VariantPattern>(arm.Pattern);
        Assert.Equal(SyntaxKind.IdentifierToken, pattern.VariantName.Kind);
        Assert.NotNull(pattern.Tail);
        Assert.Single(pattern.Tail.Patterns.Elements);
        Assert.IsType<BindingPattern>(pattern.Tail.Patterns.Elements[0].Node);
    }

    [Fact]
    public void StructPattern_Empty_ParsesCorrectly()
    {
        // Arrange: Create a match expression with an empty struct pattern
        // Struct pattern uses %( token, not .{
        string src = Header + "test :(Point) i32 = |p|> p => %() => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a StructPattern with no fields
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        StructPattern pattern = Assert.IsType<StructPattern>(arm.Pattern);
        Assert.Empty(pattern.FieldPatterns.Elements);
    }

    [Fact]
    public void StructPattern_OneField_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a struct pattern with one field
        // Struct pattern uses %( token, not .{
        // Field patterns use = not :
        string src = Header + "test :(Point) i32 = |p|> p => %(x = n) => n\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a StructPattern with one field
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        StructPattern pattern = Assert.IsType<StructPattern>(arm.Pattern);
        Assert.Single(pattern.FieldPatterns.Elements);

        FieldPattern field = pattern.FieldPatterns.Elements[0].Node;
        Assert.Equal(SyntaxKind.IdentifierToken, field.FieldName.Kind);
        Assert.IsType<BindingPattern>(field.Pattern);
    }

    [Fact]
    public void StructPattern_TwoFields_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a struct pattern with two fields
        // Struct pattern uses %( token, not .{
        // Field patterns use = not :
        string src = Header + "test :(Point) i32 = |p|> p => %(x = a, y = b) => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a StructPattern with two fields
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        StructPattern pattern = Assert.IsType<StructPattern>(arm.Pattern);
        Assert.Equal(2, pattern.FieldPatterns.Elements.Length);

        FieldPattern field1 = pattern.FieldPatterns.Elements[0].Node;
        Assert.Equal(SyntaxKind.IdentifierToken, field1.FieldName.Kind);
        Assert.IsType<BindingPattern>(field1.Pattern);

        FieldPattern field2 = pattern.FieldPatterns.Elements[1].Node;
        Assert.Equal(SyntaxKind.IdentifierToken, field2.FieldName.Kind);
        Assert.IsType<BindingPattern>(field2.Pattern);
    }

    [Fact]
    public void ListPattern_Empty_ParsesCorrectly()
    {
        // Arrange: Create a match expression with an empty list pattern
        string src = Header + "test :(list[i32]) i32 = |lst|> lst => () => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a ListPattern with no elements
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        ListPattern pattern = Assert.IsType<ListPattern>(arm.Pattern);
        Assert.Null(pattern.Elements);
    }

    [Fact]
    public void ListPattern_OneElement_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a single-element list pattern
        string src = Header + "test :(list[i32]) i32 = |lst|> lst => (x) => x\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a ListPattern with one element
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        ListPattern pattern = Assert.IsType<ListPattern>(arm.Pattern);
        Assert.NotNull(pattern.Elements);
        // ListElements.Elements contains patterns interspersed with commas
        // For a single element, we expect just the pattern
        Assert.Single(pattern.Elements.Elements);
        Assert.IsType<BindingPattern>(pattern.Elements.Elements[0]);
    }

    [Fact]
    public void ListPattern_TwoElements_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a two-element list pattern
        string src = Header + "test :(list[i32]) i32 = |lst|> lst => (x, y) => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a ListPattern with two elements
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        ListPattern pattern = Assert.IsType<ListPattern>(arm.Pattern);
        Assert.NotNull(pattern.Elements);
        // ListElements.Elements contains: pattern, comma, pattern
        Assert.Equal(3, pattern.Elements.Elements.Length);
        Assert.IsType<BindingPattern>(pattern.Elements.Elements[0]);
        Assert.IsType<BindingPattern>(pattern.Elements.Elements[2]);
    }

    [Fact]
    public void OptionalPattern_Empty_ParsesCorrectly()
    {
        // Arrange: Create a match expression with an empty optional pattern (None case)
        string src = Header + "test :(option[i32]) i32 = |opt|> opt => ?() => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as an OptionalPattern with no inner pattern
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        OptionalPattern pattern = Assert.IsType<OptionalPattern>(arm.Pattern);
        Assert.Null(pattern.Pattern);
    }

    [Fact]
    public void OptionalPattern_WithValue_ParsesCorrectly()
    {
        // Arrange: Create a match expression with an optional pattern containing a value (Some case)
        string src = Header + "test :(option[i32]) i32 = |opt|> opt => ?(n) => n\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as an OptionalPattern with an inner pattern
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        OptionalPattern pattern = Assert.IsType<OptionalPattern>(arm.Pattern);
        Assert.NotNull(pattern.Pattern);
        Assert.IsType<BindingPattern>(pattern.Pattern);
    }

    [Fact]
    public void FalliblePattern_SingleBang_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a fallible pattern !(pattern) for success case
        string src = Header + "test :(result[i32]) i32 = |res|> res => !(n) => n\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a FalliblePattern with inner pattern
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        FalliblePattern pattern = Assert.IsType<FalliblePattern>(arm.Pattern);
        Assert.Null(pattern.SecondBangToken);
        Assert.NotNull(pattern.Pattern);
        Assert.IsType<BindingPattern>(pattern.Pattern);
    }

    [Fact]
    public void FalliblePattern_DoubleBang_Empty_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a double-bang fallible pattern !!() for error case
        string src = Header + "test :(result[i32]) i32 = |res|> res => !!() => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a FalliblePattern with double bang and no inner
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        FalliblePattern pattern = Assert.IsType<FalliblePattern>(arm.Pattern);
        Assert.NotNull(pattern.SecondBangToken);
        Assert.Null(pattern.Pattern);
    }

    [Fact]
    public void FalliblePattern_DoubleBang_WithValue_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a double-bang fallible pattern !!(e) for error case
        string src = Header + "test :(result[i32]) i32 = |res|> res => !!(e) => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a FalliblePattern with double bang and inner pattern
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        FalliblePattern pattern = Assert.IsType<FalliblePattern>(arm.Pattern);
        Assert.NotNull(pattern.SecondBangToken);
        Assert.NotNull(pattern.Pattern);
        Assert.IsType<BindingPattern>(pattern.Pattern);
    }

    [Fact]
    public void NestedPattern_StructWithTupleFields_ParsesCorrectly()
    {
        // Arrange: Create a match with struct pattern containing nested tuple patterns
        string src = Header + "test :(Data) i32 = |d|> d => %(pos = #(x, y), val = n) => x\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify nested structure
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        StructPattern pattern = Assert.IsType<StructPattern>(arm.Pattern);
        Assert.Equal(2, pattern.FieldPatterns.Elements.Length);

        // First field: pos = #(x, y)
        FieldPattern field1 = pattern.FieldPatterns.Elements[0].Node;
        TuplePattern tuplePattern = Assert.IsType<TuplePattern>(field1.Pattern);
        Assert.Equal(2, tuplePattern.Patterns.Elements.Length);

        // Second field: val = n
        FieldPattern field2 = pattern.FieldPatterns.Elements[1].Node;
        Assert.IsType<BindingPattern>(field2.Pattern);
    }

    [Fact]
    public void NestedPattern_VariantWithStructPayload_ParsesCorrectly()
    {
        // Arrange: Create a match with variant pattern containing a struct pattern
        string src = Header + "test :(Result) i32 = |r|> r => |(Ok(%(value = n))) => n\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify nested structure
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        VariantPattern variantPattern = Assert.IsType<VariantPattern>(arm.Pattern);
        Assert.NotNull(variantPattern.Tail);
        Assert.Single(variantPattern.Tail.Patterns.Elements);

        // Check that the payload is a struct pattern
        StructPattern structPattern = Assert.IsType<StructPattern>(variantPattern.Tail.Patterns.Elements[0].Node);
        Assert.Single(structPattern.FieldPatterns.Elements);
    }

    [Fact]
    public void NestedPattern_ListWithTupleElements_ParsesCorrectly()
    {
        // Arrange: Create a match with list pattern containing tuple patterns
        string src = Header + "test :(list[(i32, i32)]) i32 = |lst|> lst => (#(a, b), #(c, d)) => a\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify nested structure
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        ListPattern listPattern = Assert.IsType<ListPattern>(arm.Pattern);
        Assert.NotNull(listPattern.Elements);
        // ListElements.Elements contains: pattern, comma, pattern (3 elements)
        Assert.Equal(3, listPattern.Elements.Elements.Length);

        // First element is a tuple pattern
        TuplePattern tuple1 = Assert.IsType<TuplePattern>(listPattern.Elements.Elements[0]);
        Assert.Equal(2, tuple1.Patterns.Elements.Length);

        // Third element (after comma) is also a tuple pattern
        TuplePattern tuple2 = Assert.IsType<TuplePattern>(listPattern.Elements.Elements[2]);
        Assert.Equal(2, tuple2.Patterns.Elements.Length);
    }

    [Fact]
    public void NestedPattern_OptionalWithStructContent_ParsesCorrectly()
    {
        // Arrange: Create a match with optional pattern containing a struct pattern
        string src = Header + "test :(option[Point]) i32 = |opt|> opt => ?(%(x = a, y = b)) => a\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify nested structure
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        OptionalPattern optPattern = Assert.IsType<OptionalPattern>(arm.Pattern);
        Assert.NotNull(optPattern.Pattern);

        // Check that the inner pattern is a struct pattern
        StructPattern structPattern = Assert.IsType<StructPattern>(optPattern.Pattern);
        Assert.Equal(2, structPattern.FieldPatterns.Elements.Length);
    }

    [Fact]
    public void NestedPattern_DeeplyNested_ParsesCorrectly()
    {
        // Arrange: Create a deeply nested pattern: variant(optional(tuple(struct)))
        string src = Header + "test :(Complex) i32 = |c|> c => |(Data(?(#(%(val = n))))) => n\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify deeply nested structure
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        // Layer 1: Variant pattern
        VariantPattern variantPattern = Assert.IsType<VariantPattern>(arm.Pattern);
        Assert.NotNull(variantPattern.Tail);

        // Layer 2: Optional pattern
        OptionalPattern optPattern = Assert.IsType<OptionalPattern>(variantPattern.Tail.Patterns.Elements[0].Node);
        Assert.NotNull(optPattern.Pattern);

        // Layer 3: Tuple pattern
        TuplePattern tuplePattern = Assert.IsType<TuplePattern>(optPattern.Pattern);
        Assert.Single(tuplePattern.Patterns.Elements);

        // Layer 4: Struct pattern
        StructPattern structPattern = Assert.IsType<StructPattern>(tuplePattern.Patterns.Elements[0].Node);
        Assert.Single(structPattern.FieldPatterns.Elements);

        // Final layer: Binding pattern
        FieldPattern field = structPattern.FieldPatterns.Elements[0].Node;
        Assert.IsType<BindingPattern>(field.Pattern);
    }

    [Fact]
    public void FlagsPattern_Empty_ParsesCorrectly()
    {
        // Arrange: Create a match expression with an empty flags pattern
        string src = Header + "test :(Permissions) i32 = |perms|> perms => &() => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a FlagsPattern with no entries
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        FlagsPattern pattern = Assert.IsType<FlagsPattern>(arm.Pattern);
        Assert.Empty(pattern.Entries.Elements);
    }

    [Fact]
    public void FlagsPattern_OneFlag_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a flags pattern with one flag
        string src = Header + "test :(Permissions) i32 = |perms|> perms => &(+Read) => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a FlagsPattern with one entry
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        FlagsPattern pattern = Assert.IsType<FlagsPattern>(arm.Pattern);
        Assert.Single(pattern.Entries.Elements);
    }

    [Fact]
    public void FlagsPattern_TwoFlags_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a flags pattern with two flags
        string src = Header + "test :(Permissions) i32 = |perms|> perms => &(+Read, +Write) => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a FlagsPattern with two entries
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        FlagsPattern pattern = Assert.IsType<FlagsPattern>(arm.Pattern);
        Assert.Equal(2, pattern.Entries.Elements.Length);
    }

    [Fact]
    public void ServicePattern_Empty_ParsesCorrectly()
    {
        // Arrange: Create a match expression with an empty service pattern
        string src = Header + "test :(DbService) i32 = |svc|> svc => @() => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a ServicePattern with no entries
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        ServicePattern pattern = Assert.IsType<ServicePattern>(arm.Pattern);
        Assert.Empty(pattern.Entries.Elements);
    }

    [Fact]
    public void ServicePattern_OneEntry_ParsesCorrectly()
    {
        // Arrange: Create a match expression with a service pattern with one entry
        string src = Header + "test :(DbService) i32 = |svc|> svc => @(conn: Connection) => 0\n";

        // Act: Parse the module
        BrimModule module = ParserTestHelpers.ParseModule(src);

        // Assert: Verify the pattern was parsed as a ServicePattern with one entry
        ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
        FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
        MatchExpr match = Assert.IsType<MatchExpr>(func.Body);

        Assert.Single(match.Arms.Arms);
        MatchArm arm = match.Arms.Arms[0];

        ServicePattern pattern = Assert.IsType<ServicePattern>(arm.Pattern);
        Assert.Single(pattern.Entries.Elements);

        ServicePatternEntry entry = pattern.Entries.Elements[0].Node;
        Assert.Equal(SyntaxKind.IdentifierToken, entry.Name.Kind);
    }
}
