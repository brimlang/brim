using System.Runtime.CompilerServices;
using Brim.Parse.Green;
using Brim.Parse.Producers;

namespace Brim.Parse;

public sealed partial class Parser(
  in LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> look,
  in DiagnosticList sink)
{
  internal const int StallLimit = 512; // max consecutive non-advancing iterations allowed

  readonly LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> _look = look;
  readonly DiagnosticList _diags = sink;

  public static BrimModule ParseModule(string source)
  {
    SourceText st = SourceText.From(source);
    return ParseModule(st);
  }

  public static BrimModule ParseModule(SourceText st)
  {
    DiagnosticList sink = DiagnosticList.Create();
    RawProducer raw = new(st, sink);
    SignificantProducer<RawProducer> sig = new(raw);
    LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> la = new(sig, capacity: 4);
    Parser p = new(la, sink);
    return p.ParseModule();
  }

  public SignificantToken Current => _look.Current;

  public BrimModule ParseModule()
  {
    // Required first construct: ModuleDirective
    ModuleDirective header = ModuleDirective.Parse(this);

    ImmutableArray<GreenNode>.Builder members = ImmutableArray.CreateBuilder<GreenNode>();
    ParserProgress progress = new(Current.CoreToken.Offset);
    ExpectedSet expectedSet = default; // reused accumulator

    while (Current.CoreToken.Kind is not RawKind.Eob)
    {
      // Standalone syntax (terminators/comments) handled directly.
      if (IsStandaloneSyntax(Current.Kind))
      {
        members.Add(new GreenToken(MapStandaloneSyntaxKind(Current.CoreToken.Kind), Current));

        Advance();
        progress = progress.Update(Current.CoreToken.Offset);

        if (progress.StallCount > StallLimit)
        {
          _diags.Add(Diagnostic.Unexpected(Current.CoreToken, []));
          break;
        }

        continue;
      }

      bool matched = false;
      if (ModuleMembersTable.TryGetGroup(Current.Kind, out ReadOnlySpan<Prediction> group))
      {
        foreach (Prediction entry in group)
        {
          if (Match(entry.Sequence))
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

        _diags.Add(Diagnostic.Unexpected(Current.CoreToken, expectedSet.AsSpan()));
        members.Add(new GreenToken(SyntaxKind.ErrorToken, Current));
        Advance();
      }

      progress = progress.Update(Current.CoreToken.Offset);
      if (progress.StallCount > StallLimit)
      {
        _diags.Add(Diagnostic.Unexpected(Current.CoreToken, []));
        break;
      }
    }

    // Consume EOB token explicitly
    GreenToken eob = new(SyntaxKind.EobToken, Current);
    return new BrimModule(header, members, eob)
    {
      Diagnostics = _diags.GetSortedDiagnostics()
    };
  }

  internal bool MatchRaw(RawKind kind, int offset = 0) =>
    kind == RawKind.Any || PeekKind(offset) == kind;

  internal RawToken ExpectRaw(RawKind kind)
  {
    if (Current.Kind == kind)
    {
      RawToken token = Current.CoreToken;
      Advance();
      return token;
    }

    return GetErrorToken();
  }

  internal bool MatchSyntax(SyntaxKind kind, int offset = 0)
  {
    RawKind tokenKind = MapRawKind(kind);
    if (tokenKind == RawKind.Error)
      return false; // non-mapped or already error

    return MatchRaw(tokenKind, offset);
  }

  internal GreenToken ExpectSyntax(SyntaxKind kind)
  {
    RawKind tokenKind = MapRawKind(kind);
    if (tokenKind == RawKind.Error)
      return new GreenToken(kind, Current); // non-mapped or already error

    if (MatchRaw(tokenKind))
    {
      SignificantToken sig = Current;
      Advance();

      if (sig.CoreToken.Kind != RawKind.Error)
        return new GreenToken(kind, sig);

      return new GreenToken(kind, Current); // fallthrough if underlying raw is error
    }

    return FabricateMissing(kind, tokenKind);
  }

  internal GreenToken FabricateMissing(SyntaxKind syntaxKind, RawKind expectedRaw)
  {
    _diags.Add(Diagnostic.Missing(expectedRaw, Current.CoreToken));

    RawToken missing = GetErrorToken();
    SignificantToken fabricated = new(
      missing,
      LeadingTrivia: []);

    return new GreenToken(syntaxKind, fabricated);
  }

  void Advance() => _look.Advance();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool Match(TokenSequence seq)
  {
    // Fast path: inline kind comparison to avoid repeated method dispatch.
    for (int i = 0; i < seq.Length; i++)
    {
      RawKind expect = seq[i];
      if (expect == RawKind.Any) continue; // wildcard slot

      RawKind actual = i == 0
        ? Current.Kind
        : _look.Peek(i).Kind; // direct peek; safe within capacity by table construction
      if (actual != expect) return false;
    }

    return true;
  }

  RawKind PeekKind(int offset)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(offset);
    if (offset == 0)
      return Current.Kind;

    ref readonly SignificantToken st = ref _look.Peek(offset); // throws if > capacity
    return st.CoreToken.Kind;
  }

  static bool IsStandaloneSyntax(RawKind kind) => kind is RawKind.Terminator or RawKind.CommentTrivia;

  RawToken GetErrorToken() => new(
    RawKind.Error,
    Current.CoreToken.Offset,
    Length: 0,
    Current.CoreToken.Line,
    Current.CoreToken.Column);

  internal void AddDiagEmptyGeneric(GreenToken open) =>
    _diags.Add(Diagnostic.EmptyGenericArgList(open.Token));
  internal void AddDiagUnexpectedGenericBody() =>
    _diags.Add(Diagnostic.UnexpectedGenericBody(Current.CoreToken));

  /// <summary>
  /// Tracks parser forward progress: last offset and number of consecutive non-advancing iterations.
  /// </summary>
  readonly record struct ParserProgress(int LastOffset, int StallCount = 0)
  {
    public ParserProgress Update(int currentOffset) =>
      currentOffset == LastOffset
        ? new ParserProgress(LastOffset, StallCount + 1)
        : new ParserProgress(currentOffset, 0);
  }
}

