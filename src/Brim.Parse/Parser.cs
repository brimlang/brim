using System.Diagnostics;
using Brim.Parse.Green;
using Brim.Parse.Producers;

namespace Brim.Parse;

public sealed partial class Parser(
  in LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> look,
  in DiagSink sink)
{
  internal const int StallLimit = 512; // max consecutive non-advancing iterations allowed
  readonly LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> _look = look;
  readonly DiagSink _sink = sink;

  public SignificantToken Current => _look.Current;
  RawToken CurrentRaw => Current.CoreToken;

  public IReadOnlyList<Diagnostic> Diagnostics => _sink.Items;

  public BrimModule ParseModule()
  {
    // Required first construct: ModuleHeader ( [[ ... ]] ) optionally followed by export directives.
    ModuleDirective? headerNode = null;
    if (CurrentRaw.Kind == RawTokenKind.LBracketLBracket)
    {
      // Parse header directly (not via generic directives table to enforce position rule).
      ModuleHeader mh = ModuleHeader.Parse(this);
      GreenToken term = ExpectSyntax(SyntaxKind.TerminatorToken);
      headerNode = new ModuleDirective(mh, term);
    }
    else if (!IsEof(CurrentRaw))
    {
      // Missing required header: emit diagnostic and fabricate minimal empty header using error tokens.
      _sink.Add(DiagFactory.MissingModuleHeader(CurrentRaw));
      // Fabricate tokens
      RawToken errorTok = GetErrorToken();
      GreenToken open = new(SyntaxKind.ModulePathOpenToken, new SignificantToken(errorTok, []));
      GreenToken close = new(SyntaxKind.ModulePathCloseToken, new SignificantToken(errorTok, []));
      ModulePath path = new([new GreenToken(SyntaxKind.IdentifierToken, new SignificantToken(errorTok, []))]);
      ModuleHeader fabricatedHeader = new(open, path, close);
      GreenToken fabricatedTerm = new(SyntaxKind.TerminatorToken, new SignificantToken(errorTok, []));
      headerNode = new ModuleDirective(fabricatedHeader, fabricatedTerm);
    }

    // After header, parse any export directives ( '<< Identifier' ) in sequence.
    List<GreenNode> directiveList = new();
    if (headerNode is not null) directiveList.Add(headerNode);
    while (!IsEof(CurrentRaw) && CurrentRaw.Kind == RawTokenKind.LessLess)
    {
      directiveList.Add(ExportDirective.Parse(this));
    }
    ImmutableArray<GreenNode>.Builder directiveNodes = ImmutableArray.CreateBuilder<GreenNode>(directiveList.Count);
    directiveNodes.AddRange(directiveList);
    ModuleDirective header = (ModuleDirective)directiveNodes[0];

  ImmutableArray<GreenNode>.Builder members = ImmutableArray.CreateBuilder<GreenNode>();
  ParserProgress progress = new(CurrentRaw.Offset);
    ExpectedSet expectedSet = default; // reused accumulator
    while (!IsEof(CurrentRaw))
    {
      // Standalone syntax (terminators/comments) handled directly.
      if (IsStandaloneSyntax(CurrentRaw.Kind))
      {
        members.Add(new GreenToken(MapStandaloneSyntaxKind(CurrentRaw.Kind), Current));
        _ = Advance();
        progress = progress.Update(CurrentRaw.Offset);
        if (progress.StallCount > StallLimit)
        {
          _sink.Add(DiagFactory.Unexpected(CurrentRaw, ReadOnlySpan<RawTokenKind>.Empty));
          break;
        }
        continue;
      }

      bool matched = false;
      if (ModuleMembersTable.TryGetGroup(CurrentRaw.Kind, out ReadOnlySpan<Prediction> group))
      {
        foreach (Prediction entry in group)
        {
          if (Match(entry.Look))
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
          expectedSet = expectedSet.Add(pe.Look[0]);
        ReportUnexpected(expectedSet);
        members.Add(new GreenToken(SyntaxKind.ErrorToken, Current));
        _ = Advance();
      }

      int prevOffset = progress.LastOffset;
      progress = progress.Update(CurrentRaw.Offset);
#if DEBUG
      if (CurrentRaw.Offset == prevOffset)
        Debug.Assert(progress.StallCount > 0, "Offset did not advance but stall count failed to increment.");
      else
        Debug.Assert(progress.StallCount == 0, "Offset advanced but stall count not reset to zero.");
#endif
      if (progress.StallCount > StallLimit)
      {
        _sink.Add(DiagFactory.Unexpected(CurrentRaw, ReadOnlySpan<RawTokenKind>.Empty));
        break;
      }
    }

    // Consume EOB token explicitly
    GreenToken eob = new(SyntaxKind.EobToken, Current);
    // Stable sort diagnostics (already in emission order; ensure order by offset then insertion)
    if (_sink.Items is List<Diagnostic> list && list.Count > 1)
    {
      list.Sort(static (a, b) =>
      {
        int cmp = a.Offset.CompareTo(b.Offset);
        if (cmp != 0) return cmp;
        // tie-breaker: line, column to maintain determinism
        cmp = a.Line.CompareTo(b.Line);
        if (cmp != 0) return cmp;
        // fallback: code (semi-stable; list.Sort is not stable in .NET so we emulate minimal extra ordering)
        return ((int)a.Code).CompareTo((int)b.Code);
      });
    }

    StructuralArray<Diagnostic> diags = _sink.Items is List<Diagnostic> list2 && list2.Count > 0
      ? new StructuralArray<Diagnostic>(list2)
      : StructuralArray.Empty<Diagnostic>();
  return new BrimModule(header, members, eob, diags);
  }

  internal bool Match(RawTokenKind kind, int offset = 0) =>
    kind == RawTokenKind.Any || PeekKind(offset) == kind;

  internal RawToken Expect(RawTokenKind kind)
  {
    if (CurrentRaw.Kind == kind)
    {
      RawToken token = CurrentRaw;
      _ = Advance();
      return token;
    }

    return GetErrorToken();
  }

  internal GreenToken ExpectSyntax(SyntaxKind kind)
  {
    RawTokenKind tokenKind = Parser.ToRawTokenKind(kind);
    if (tokenKind == RawTokenKind.Error)
      return new GreenToken(kind, Current); // non-mapped or already error

    if (Match(tokenKind))
    {
      SignificantToken sig = Current;
      _ = Advance();
      if (sig.CoreToken.Kind != RawTokenKind.Error)
        return new GreenToken(kind, sig);
      return new GreenToken(kind, Current); // fallthrough if underlying raw is error
    }
    return FabricateMissing(kind, tokenKind);
  }

  internal void ReportUnexpected(ExpectedSet expected) =>
    _sink.Add(DiagFactory.Unexpected(CurrentRaw, expected.AsSpan()));

  internal GreenToken FabricateMissing(SyntaxKind syntaxKind, RawTokenKind expectedRaw)
  {
    _sink.Add(DiagFactory.Missing(expectedRaw, CurrentRaw));
    RawToken missing = GetErrorToken();
    SignificantToken fabricated = new(
      missing,
      LeadingTrivia: []);
    return new GreenToken(syntaxKind, fabricated);
  }

  bool Advance() => _look.Advance();

  [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
  bool Match(TokenSequence seq)
  {
    // Fast path: inline kind comparison to avoid repeated method dispatch.
    for (int i = 0; i < seq.Length; i++)
    {
      RawTokenKind expect = seq[i];
      if (expect == RawTokenKind.Any) continue; // wildcard slot
      RawTokenKind actual = i == 0
        ? CurrentRaw.Kind
        : _look.Peek(i).CoreToken.Kind; // direct peek; safe within capacity by table construction
      if (actual != expect) return false;
    }
    return true;
  }

  RawTokenKind PeekKind(int offset)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(offset);
    if (offset == 0) return CurrentRaw.Kind;
    ref readonly SignificantToken st = ref _look.Peek(offset); // throws if > capacity
  return st.CoreToken.Kind; // This line remains unchanged
  }

  static bool IsStandaloneSyntax(RawTokenKind kind) => kind is RawTokenKind.Terminator or RawTokenKind.CommentTrivia;

  static bool IsEof(RawToken tok) => tok.Kind == RawTokenKind.Eob;

  static SyntaxKind MapStandaloneSyntaxKind(RawTokenKind kind) => kind switch
  {
    RawTokenKind.Terminator => SyntaxKind.TerminatorToken,
    RawTokenKind.CommentTrivia => SyntaxKind.CommentToken,
    _ => SyntaxKind.ErrorToken
  };

  RawToken GetErrorToken() => new(
    RawTokenKind.Error,
    CurrentRaw.Offset,
    Length: 0,
    CurrentRaw.Line,
    CurrentRaw.Column);

  internal void AddDiagEmptyGeneric(GreenToken open)
  {
    Diagnostic d = DiagFactory.EmptyGenericParamList(open.Token);
    _sink.Add(d);
  }

  internal void AddDiagUnexpectedGenericBody() => _sink.Add(DiagFactory.UnexpectedGenericBody(CurrentRaw));
  internal void AddDiagEmptyGenericArgList(GreenToken open)
  {
    Diagnostic d = DiagFactory.EmptyGenericArgList(open.Token);
    _sink.Add(d);
  }

  // Convenience static entry points (replace former ParseFacade)
  public static BrimModule ParseModule(string source)
  {
    SourceText st = SourceText.From(source);
    return ParseModule(st);
  }

  public static BrimModule ParseModule(SourceText st)
  {
    DiagSink sink = DiagSink.Create();
    RawProducer raw = new(st, sink);
    SignificantProducer<RawProducer> sig = new(raw);
    LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> la = new(sig, capacity: 4);
    Parser p = new(la, sink);
    return p.ParseModule();
  }
}

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
