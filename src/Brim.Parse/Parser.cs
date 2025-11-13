using System.Runtime.CompilerServices;
using Brim.Lex;
using Brim.Parse.Collections;
using Brim.Parse.Green;
using Brim.Parse.Producers;

namespace Brim.Parse;

public sealed partial class Parser(
  in RingBuffer<CoreToken> look,
  in DiagnosticList sink)
{
  readonly RingBuffer<CoreToken> _look = look;
  readonly DiagnosticList _diags = sink;

  /// <summary>
  /// Parses a BrimModule from the given SourceText.
  /// </summary>
  /// <param name="source">The source text to parse.</param>
  /// <returns>A parsed BrimModule instance.</returns>
  public static BrimModule ModuleFrom(SourceText source)
  {
    DiagnosticList diags = DiagnosticList.Create();
    LexTokenSource lexSource = new(source, diags);
    return new Parser(
      new RingBuffer<CoreToken>(
        new CoreTokenSource(
          lexSource),
        capacity: 4),
      diags)
    .ParseModule();
  }

  internal static BrimModule ParseModule(string src) => ModuleFrom(SourceText.From(src));

  public CoreToken Current => _look.Current;

  public BrimModule ParseModule()
  {
    ModuleDirective header = ModuleDirective.Parse(this);

    ArrayBuilder<GreenNode> members = [];
    ExpectedSet expectedSet = default; // reused accumulator

    while (!Tokens.IsEob(Current))
    {
      Trace.WriteLine($"[Parser] {Current}");
      StallGuard stallGuard = GetStallGuard();
      // terminators handled directly.
      if (Match(TokenKind.Terminator))
      {
        members.Add(SyntaxKind.TerminatorToken.MakeGreen(Current));
        Advance();
        continue;
      }

      bool matched = false;
      if (ModuleMembersTable.TryGetGroup(Current.TokenKind, out ReadOnlySpan<Prediction> group))
      {
        foreach (Prediction entry in group)
        {
          if (MatchSequence(entry.Sequence))
          {
            matched = true;
            members.Add(entry.Action(this));
            break;
          }
        }
      }

      if (!matched)
      {
        // Build expected set lazily (cheap ≤4). Reset accumulator each failure.
        expectedSet = default;
        foreach (Prediction pe in ModuleMembersTable.Entries)
          expectedSet = expectedSet.Add(pe.Sequence[0]);

        _diags.Add(Diagnostic.Parse.Unexpected(Current, expectedSet.AsSpan()));
        members.Add(SyntaxKind.ErrorToken.MakeGreen(Current));
        Advance();
      }

      if (stallGuard.Stalled)
      {
        Trace.WriteLine("[Parser] Stalled detected, skipping unexpected token.");
        _diags.Add(Diagnostic.Parse.Unexpected(Current, []));
        break;
      }
    }

    // Consume EOB token explicitly
    GreenToken eob = SyntaxKind.EobToken.MakeGreen(Current);
    return new BrimModule(header, members, eob)
    {
      Diagnostics = _diags.GetSortedDiagnostics()
    };
  }

  internal bool Match(TokenKind kind, int offset = 0) =>
    PeekTokenKind(offset) != TokenKind.Eob && (kind == TokenKind.Any || PeekTokenKind(offset) == kind);

  internal GreenToken Expect(SyntaxKind kind)
  {
    TokenKind tokenKind = MapTokenKind(kind);
    if (tokenKind == TokenKind.Error)
    {
      Advance();
      return SyntaxKind.ErrorToken.MakeGreen(Current); // non-mapped or already error
    }

    if (Match(tokenKind))
    {
      CoreToken core = Current;
      Advance();

      return core.TokenKind != TokenKind.Error
        ? kind.MakeGreen(core)
        : SyntaxKind.ErrorToken.MakeGreen(core);
    }

    return FabricateMissing(kind);
  }

  internal StructuralArray<GreenToken> CollectSyntaxKind(SyntaxKind kind)
  {
    ArrayBuilder<GreenToken> tokens = [];
    while (Match(MapTokenKind(kind)))
    {
      CoreToken sig = Current;
      tokens.Add(kind.MakeGreen(sig));
      Advance();
    }

    return tokens;
  }

  internal GreenToken FabricateMissing(SyntaxKind syntaxKind)
  {
    TokenKind expectedRaw = MapTokenKind(syntaxKind);
    _diags.Add(Diagnostic.Parse.Missing(expectedRaw, Current));

    LexToken missing = GetMissingToken();
    CoreToken fabricated = CoreToken.FromLexToken(leading: [], lex: missing);
    return new GreenToken(syntaxKind, fabricated);
  }

  internal GreenToken UnexpectedTokenAsError()
  {
    _diags.Add(Diagnostic.Parse.Unexpected(Current, []));
    CoreToken curr = Current;
    Advance();
    return SyntaxKind.ErrorToken.MakeGreen(curr);
  }

  internal void AddDiagEmptyGeneric(GreenToken open)
  {
    CoreToken token = open.CoreToken;
    _diags.Add(Diagnostic.Parse.EmptyGenericArgList(token));
  }

  internal void AddDiagEmptyGenericParam(GreenToken open)
  {
    CoreToken token = open.CoreToken;
    _diags.Add(Diagnostic.Parse.EmptyGenericParamList(token));
  }

  internal void AddDiagUnexpectedGenericBody() => _diags.Add(Diagnostic.Parse.UnexpectedGenericBody(Current));

  internal void AddDiagEmptyNamedTupleElementList() =>
    _diags.Add(Diagnostic.Parse.Unexpected(Current, [TokenKind.Identifier]));

  internal void AddDiagInvalidGenericConstraint() =>
    _diags.Add(Diagnostic.Parse.InvalidGenericConstraint(Current));

  internal void AddDiagUnsupportedModuleMember(CoreToken token) =>
    _diags.Add(Diagnostic.Parse.UnsupportedModuleMember(token));

  internal StallGuard GetStallGuard() => new(this);

  void Advance() => _look.Advance();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool MatchSequence(TokenSequence seq)
  {
    // Fast path: inline kind comparison to avoid repeated method dispatch.
    for (int i = 0; i < seq.Length; i++)
    {
      TokenKind expect = seq[i];
      if (expect == TokenKind.Any) continue; // wildcard slot

      TokenKind actual = i == 0
        ? Current.TokenKind
        : _look.Peek(i).TokenKind; // direct peek; safe within capacity by table construction

      if (actual != expect) return false;
    }

    return true;
  }

  TokenKind PeekTokenKind(int offset)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(offset);
    if (offset == 0)
      return Current.TokenKind;

    ref readonly CoreToken st = ref _look.Peek(offset); // throws if > capacity
    return st.TokenKind;
  }

  LexToken GetErrorToken() => new(
    TokenKind.Error,
    Current.Offset,
    length: 0,
    Current.Line,
    Current.Column);

  LexToken GetMissingToken() => new(
    TokenKind.Missing,
    Current.Offset,
    length: 0,
    Current.Line,
    Current.Column);

  /// <summary>
  /// A guard that checks if the parser's position has stalled (not advanced).
  /// </summary>
  /// <remarks>
  /// Captures the parser's current token offset at construction and provides a property to check if the parser has advanced.
  /// </remarks>
  /// <param name="p">The parser whose position to monitor.</param>
  internal readonly ref struct StallGuard(Parser p)
  {
    private readonly int _before = p.Current.Offset;

    /// <summary>
    /// Gets a value indicating whether the parser has stalled (i.e., not advanced since construction).
    /// </summary>
    public bool Stalled => _before == p.Current.Offset;
  }

}
