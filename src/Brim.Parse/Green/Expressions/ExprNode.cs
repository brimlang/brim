namespace Brim.Parse.Green;

public abstract record ExprNode(SyntaxKind Kind, int Offset) : GreenNode(Kind, Offset)
{
}
