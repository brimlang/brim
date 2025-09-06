using Brim.Parse.Green;

namespace Brim.Parse;

internal delegate GreenNode ParseAction(Parser parser);

/// <summary>
/// Ordered prediction entry. The <see cref="TokenSequence"/> is matched against the token stream; first matching entry wins.
/// </summary>
internal readonly record struct Prediction(TokenSequence Sequence, ParseAction Action)
{
  public TokenSequence Look => Sequence;
}

/// <summary>
/// Fixed-size (≤4) token sequence used for prediction lookahead (explicit length, trailing Any).
/// </summary>
internal readonly struct TokenSequence
{
  readonly RawTokenKind _k1;
  readonly RawTokenKind _k2;
  readonly RawTokenKind _k3;
  readonly RawTokenKind _k4;
  public byte Length { get; }

  public TokenSequence(RawTokenKind k1) { _k1 = k1; _k2 = RawTokenKind.Any; _k3 = RawTokenKind.Any; _k4 = RawTokenKind.Any; Length = 1; }
  public TokenSequence(RawTokenKind k1, RawTokenKind k2) { _k1 = k1; _k2 = k2; _k3 = RawTokenKind.Any; _k4 = RawTokenKind.Any; Length = 2; }
  public TokenSequence(RawTokenKind k1, RawTokenKind k2, RawTokenKind k3) { _k1 = k1; _k2 = k2; _k3 = k3; _k4 = RawTokenKind.Any; Length = 3; }
  public TokenSequence(RawTokenKind k1, RawTokenKind k2, RawTokenKind k3, RawTokenKind k4) { _k1 = k1; _k2 = k2; _k3 = k3; _k4 = k4; Length = 4; }

  public RawTokenKind this[int i] => i switch
  {
    0 => _k1,
    1 => _k2,
    2 => _k3,
    3 => _k4,
    _ => throw new ArgumentOutOfRangeException(nameof(i))
  };

  public static implicit operator TokenSequence(RawTokenKind one) => new(one);
  public static implicit operator TokenSequence((RawTokenKind one, RawTokenKind two) t) => new(t.one, t.two);
  public static implicit operator TokenSequence((RawTokenKind one, RawTokenKind two, RawTokenKind three) t) => new(t.one, t.two, t.three);
  public static implicit operator TokenSequence((RawTokenKind one, RawTokenKind two, RawTokenKind three, RawTokenKind four) t) => new(t.one, t.two, t.three, t.four);
}

/// <summary>
/// Read-only grouped prediction table. Provides O(1) access to subset sharing the same first token.
/// </summary>
readonly ref struct PredictionTable
{
  readonly ReadOnlySpan<Prediction> _entries;
  readonly ReadOnlySpan<int> _groupStart; // -1 if none
  readonly ReadOnlySpan<byte> _groupCount; // 0 if none
  public ReadOnlySpan<Prediction> Entries => _entries;

  public PredictionTable(ReadOnlySpan<Prediction> entries, ReadOnlySpan<int> starts, ReadOnlySpan<byte> counts)
  { _entries = entries; _groupStart = starts; _groupCount = counts; }

  public bool TryGetGroup(RawTokenKind kind, out ReadOnlySpan<Prediction> group)
  {
    int idx = (int)kind;
    if ((uint)idx >= (uint)_groupStart.Length || _groupStart[idx] < 0)
    { group = ReadOnlySpan<Prediction>.Empty; return false; }
    int start = _groupStart[idx];
    int count = _groupCount[idx];
    group = _entries.Slice(start, count);
    return true;
  }
}

public sealed partial class Parser
{
  internal static RawTokenKind ToRawTokenKind(SyntaxKind kind) => kind switch
  {
    SyntaxKind.TerminatorToken => RawTokenKind.Terminator,
    SyntaxKind.ExportMarkerToken => RawTokenKind.LessLess,
    SyntaxKind.ModulePathOpenToken => RawTokenKind.LBracketLBracket,
    SyntaxKind.ModulePathCloseToken => RawTokenKind.RBracketRBracket,
    SyntaxKind.ModulePathSepToken => RawTokenKind.ColonColon,
    SyntaxKind.GenericOpenToken => RawTokenKind.LBracket,
    SyntaxKind.GenericCloseToken => RawTokenKind.RBracket,
    SyntaxKind.IdentifierToken => RawTokenKind.Identifier,
    SyntaxKind.NumberToken => RawTokenKind.NumberLiteral,
    SyntaxKind.StrToken => RawTokenKind.StringLiteral,
    SyntaxKind.EqualToken => RawTokenKind.Equal,
    SyntaxKind.StructToken => RawTokenKind.PercentLBrace,
    SyntaxKind.UnionToken => RawTokenKind.PipeLBrace,
    SyntaxKind.CloseBraceToken => RawTokenKind.RBrace,
    SyntaxKind.CommentToken => RawTokenKind.CommentTrivia,
    SyntaxKind.EobToken => RawTokenKind.Eob,
    SyntaxKind.ColonToken => RawTokenKind.Colon,
    SyntaxKind.ErrorToken => RawTokenKind.Error,
    _ => RawTokenKind.Error
  };

  // Directive (header / export) predictions.
  internal static readonly Prediction[] ModuleDirectivePredictions =
  [
  new(RawTokenKind.LessLess, ExportDirective.Parse),
  ];

  // Member / declaration predictions.
  internal static readonly Prediction[] ModuleMemberPredictions =
  [
    new((RawTokenKind.Identifier, RawTokenKind.Equal, RawTokenKind.LBracketLBracket), ImportDeclaration.Parse),
    new((RawTokenKind.Identifier, RawTokenKind.LBracket, RawTokenKind.Identifier, RawTokenKind.Comma), GenericDeclaration.Parse),
    new((RawTokenKind.Identifier, RawTokenKind.LBracket, RawTokenKind.Identifier, RawTokenKind.Identifier), GenericDeclaration.Parse),
    new((RawTokenKind.Identifier, RawTokenKind.LBracket, RawTokenKind.Identifier, RawTokenKind.RBracket), GenericDeclaration.Parse),
    new((RawTokenKind.Identifier, RawTokenKind.Equal, RawTokenKind.PercentLBrace), StructDeclaration.Parse),
    new((RawTokenKind.Identifier, RawTokenKind.Equal, RawTokenKind.PipeLBrace), UnionDeclaration.Parse),
    new((RawTokenKind.Identifier, RawTokenKind.LBracket, RawTokenKind.RBracket), GenericDeclaration.Parse), // empty generic param list
  ];

  internal static PredictionTable ModuleDirectivesTable => BuildTable(ModuleDirectivePredictions);
  internal static PredictionTable ModuleMembersTable => BuildTable(ModuleMemberPredictions);

  static PredictionTable BuildTable(ReadOnlySpan<Prediction> preds)
  {
    if (preds.Length == 0)
      return new PredictionTable(preds, ReadOnlySpan<int>.Empty, ReadOnlySpan<byte>.Empty);

    int maxKind = 0;
    foreach (Prediction p in preds)
    {
      int k = (int)p.Look[0];
      if (k > maxKind) maxKind = k;
    }
    int[] starts = new int[maxKind + 1];
    byte[] counts = new byte[maxKind + 1];
    Array.Fill(starts, -1);

    // We rely on original order; first occurrence sets start, every occurrence bumps count.
    for (int i = 0; i < preds.Length; i++)
    {
      int k = (int)preds[i].Look[0];
      if (starts[k] == -1)
      {
        starts[k] = i;
        counts[k] = 1;
      }
      else
      {
        counts[k]++;
      }
    }
    return new PredictionTable(preds, starts, counts);
  }

  static Parser()
  {
#if DEBUG
    void Validate(ReadOnlySpan<Prediction> preds, string name)
    {
      HashSet<string> seen = [];
      foreach (Prediction e in preds)
      {
        TokenSequence ls = e.Look;
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < ls.Length; i++)
        {
          if (i > 0) sb.Append(',');
          sb.Append((int)ls[i]);
        }
        string key = sb.ToString();
        if (!seen.Add(key))
          throw new InvalidOperationException($"Duplicate TokenSequence in prediction table '{name}': {key}");
      }
    }
    Validate(ModuleDirectivePredictions, nameof(ModuleDirectivePredictions));
    Validate(ModuleMemberPredictions, nameof(ModuleMemberPredictions));
#endif
  }

  internal static ParseAction MakeTrace(string name, ParseAction action) => p =>
  {
    Console.WriteLine($"Entering {name} at token {p.CurrentRaw} (offset {p.CurrentRaw.Offset})");
    GreenNode result = action(p);
    Console.WriteLine($"Exiting {name} at token {p.CurrentRaw} (offset {p.CurrentRaw.Offset})");
    return result;
  };
}
