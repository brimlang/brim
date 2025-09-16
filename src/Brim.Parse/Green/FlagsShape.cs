using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record FlagsShape(
  GreenToken Ampersand,
  GreenToken UnderlyingType,
  GreenToken OpenBrace,
  StructuralArray<FlagMemberDeclaration> Members,
  GreenToken CloseBrace) :
GreenNode(SyntaxKind.FlagsShape, Ampersand.Offset),
IParsable<FlagsShape>
{
  public override int FullWidth => CloseBrace.EndOffset - Ampersand.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Ampersand;
    yield return UnderlyingType;
    yield return OpenBrace;
    foreach (FlagMemberDeclaration m in Members) yield return m;
    yield return CloseBrace;
  }

  public static FlagsShape Parse(Parser p)
  {
    GreenToken amp = p.ExpectSyntax(SyntaxKind.AmpersandToken);
    if (p.MatchRaw(RawKind.LBrace))
    {
      // Invalid: &{ ... }
      p.AddDiagUnexpectedGenericBody();
      // fabricate missing underlying type to continue
      _ = p.FabricateMissing(SyntaxKind.IdentifierToken, RawKind.Identifier);
    }

    GreenToken underlying = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    ImmutableArray<FlagMemberDeclaration>.Builder members = ImmutableArray.CreateBuilder<FlagMemberDeclaration>();
    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        Parser.StallGuard sg = p.GetStallGuard();
        FlagMemberDeclaration m = FlagMemberDeclaration.Parse(p);
        members.Add(m);

        if (m.TrailingComma is null)
          break;

        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob))
          break;

        if (sg.Stalled)
          break; // progress guard
      }
    }

    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new FlagsShape(amp, underlying, open, members, close);
  }
}
