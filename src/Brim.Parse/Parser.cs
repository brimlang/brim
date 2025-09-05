using System.Diagnostics;
using Brim.Parse.Green;
using Brim.Parse.Producers;

namespace Brim.Parse;

public sealed partial class Parser(
  in LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> look,
  in DiagSink sink)
{
  readonly LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> _look = look;
  readonly DiagSink _sink = sink;

  public SignificantToken Current => _look.Current;
  RawToken CurrentRaw => Current.CoreToken;

  public IReadOnlyList<Diagnostic> Diagnostics => _sink.Items;

  public BrimModule ParseModule()
  {
    PredictEntry[] table = ModuleTable; // ordered prediction entries

    ModuleDirective header = ModuleDirective.Parse(this);

    ImmutableArray<GreenNode>.Builder members = ImmutableArray.CreateBuilder<GreenNode>();
    int iterations = 0;

    // Reusable buffer for expected K1 kinds (single stackalloc)
    Span<RawTokenKind> expectedBuf = stackalloc RawTokenKind[4];
    while (!IsEof(CurrentRaw))
    {
      int startOffset = CurrentRaw.Offset;
      bool progressed = false;

      // Standalone syntax (terminators/comments) handled directly.
      if (IsStandaloneSyntax(CurrentRaw.Kind))
      {
        members.Add(new GreenToken(MapStandaloneSyntaxKind(CurrentRaw.Kind), Current));
        _ = Advance();
        continue;
      }

      bool matched = false;
      foreach (PredictEntry entry in table)
      {
        if (Match(entry.LookAhead))
        {
          matched = true;
          members.Add(entry.Action(this));
          progressed = true;
          break;
        }
      }

      if (!matched && !progressed)
      {
        // Collect expected K1 tokens from prediction table (unique, up to 4)
        int ec = 0;
        foreach (PredictEntry pe in table)
        {
          RawTokenKind k = pe.LookAhead.K1;
          bool dup = false;
          for (int i = 0; i < ec; i++) if (expectedBuf[i] == k) { dup = true; break; }
          if (!dup) { expectedBuf[ec++] = k; if (ec == 4) break; }
        }
        _sink.Add(DiagFactory.Unexpected(CurrentRaw, expectedBuf[..ec]));

        // Always advance at least one token to guarantee progress
        members.Add(new GreenToken(SyntaxKind.ErrorToken, Current));

        _ = Advance();
        progressed = true;
      }

      Debug.Assert(progressed, $"Parser did not make progress at token {CurrentRaw}.");
      if (!progressed)
        break; // hard escape to avoid hang in release

      // Additional runtime guard: ensure the raw token offset advances; if not, break to avoid infinite loop.
      if (CurrentRaw.Offset == startOffset)
      {
        _sink.Add(DiagFactory.Unexpected(CurrentRaw, []));
        break;
      }

      iterations++;
      if (iterations > 200_000)
        throw new InvalidOperationException("Parser iteration guard exceeded (possible infinite loop)");
    }

    // Consume EOB token explicitly
    GreenToken eob = new(SyntaxKind.EobToken, Current);
    return new BrimModule(header, members, eob);
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
    if (TokenMap.TryGetValue(kind, out RawTokenKind tokenKind))
    {
      if (Match(tokenKind))
      {
        SignificantToken sig = Current;
        _ = Advance();
        if (sig.CoreToken.Kind != RawTokenKind.Error)
          return new GreenToken(kind, sig);
      }
      else
      {
        _sink.Add(DiagFactory.Missing(tokenKind, CurrentRaw));
        RawToken missing = GetErrorToken();
        SignificantToken fabricated = new(
          missing,
          LeadingTrivia: [],
          TrailingTrivia: []);

        return new GreenToken(kind, fabricated);
      }
    }

    return new GreenToken(kind, Current);
  }

  bool Advance() => _look.Advance();

  bool Match(LookAhead lookAhead) =>
    Match(lookAhead.K1, 0)
    && Match(lookAhead.K2, 1)
    && Match(lookAhead.K3, 2)
    && Match(lookAhead.K4, 3);

  RawTokenKind PeekKind(int offset)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(offset);
    if (offset == 0) return CurrentRaw.Kind;
    ref readonly SignificantToken st = ref _look.Peek(offset); // throws if > capacity
    return st.CoreToken.Kind;
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
}
