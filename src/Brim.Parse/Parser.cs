using System.Collections.Immutable;
using System;
using System.Diagnostics;
using Brim.Parse.Green;

namespace Brim.Parse;

public partial struct Parser
{
  LookAheadWindow<SignificantToken, SignificantProducer<RawTokenProducer>> _look;
  readonly List<Diag> _diags;

  public Parser(SourceText source)
  {
  _diags = new();
  RawTokenProducer raw = new(source, _diags);

    SignificantProducer<RawTokenProducer> sig = new(
      raw,
      static (ref prod, out tok) => prod.TryRead(out tok));

    _look = new(
      sig,
      static (ref sp, out st) => sp.TryRead(out st),
      capacity: 4);
  }

  readonly SignificantToken CurrentSig => _look.Current;
  readonly RawToken Current => CurrentSig.Token;

  public BrimModule ParseModule()
  {
    PredictEntry[] table = _moduleTable; // ordered prediction entries

    ModuleDirective header = ModuleDirective.Parse(ref this);

    ImmutableArray<GreenNode>.Builder members = ImmutableArray.CreateBuilder<GreenNode>();
    int iterations = 0;
  // Reusable buffer for expected K1 kinds (single stackalloc)
  Span<RawTokenKind> expectedBuf = stackalloc RawTokenKind[4];
  while (!IsEof(Current))
    {
      bool progressed = false;
      // Standalone syntax (terminators/comments) handled directly.
      if (IsStandaloneSyntax(Current.Kind))
      {
        members.Add(new GreenToken(MapStandaloneSyntaxKind(Current.Kind), CurrentSig));
        _ = Advance();
        continue;
      }

      bool matched = false;
      foreach (PredictEntry entry in table)
      {
        if (Match(entry.LookAhead))
        {
          matched = true;
          members.Add(entry.Action(ref this));
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
    _diags.Add(DiagFactory.Unexpected(Current, expectedBuf[..ec]));
  // Always advance at least one token to guarantee progress
  members.Add(new GreenToken(SyntaxKind.ErrorToken, CurrentSig));

        _ = Advance();
        progressed = true;
      }

      Debug.Assert(progressed, $"Parser did not make progress at token {Current}.");
      if (!progressed)
        break; // hard escape to avoid hang in release

      iterations++;
      if (iterations > 200_000)
        throw new InvalidOperationException("Parser iteration guard exceeded (possible infinite loop)");
    }

    // Consume EOF token explicitly
  GreenToken eof = new(SyntaxKind.EofToken, CurrentSig);
    return new BrimModule(header, members, eof);
  }

  internal readonly bool Match(RawTokenKind kind, int offset = 0) =>
    kind == RawTokenKind.Any || PeekKind(offset) == kind;

  internal RawToken Expect(RawTokenKind kind)
  {
    if (Current.Kind == kind)
    {
      RawToken token = Current;
      _ = Advance();
      return token;
    }

    return GetErrorToken(kind);
  }

  internal GreenToken ExpectSyntax(SyntaxKind kind)
  {
    if (_tokenMap.TryGetValue(kind, out RawTokenKind tokenKind))
    {
      if (Match(tokenKind))
      {
        SignificantToken sig = CurrentSig;
        _ = Advance();
        if (sig.Token.Kind != RawTokenKind.Error)
          return new GreenToken(kind, sig);
      }
      else
      {
        _diags.Add(DiagFactory.Missing(tokenKind, Current));
        RawToken missing = GetErrorToken(tokenKind);
        SignificantToken fabricated = new(missing,
          StructuralArray.Empty<RawToken>(),
          StructuralArray.Empty<RawToken>());
        return new GreenToken(kind, fabricated);
      }
    }

    return new GreenToken(kind, CurrentSig);
  }

  bool Advance() => _look.Advance();

  readonly bool Match(LookAhead lookAhead) =>
    Match(lookAhead.K1, 0)
    && Match(lookAhead.K2, 1)
    && Match(lookAhead.K3, 2)
    && Match(lookAhead.K4, 3);

  readonly RawTokenKind PeekKind(int offset)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(offset);
    if (offset == 0) return Current.Kind;
    ref readonly SignificantToken st = ref _look.Peek(offset); // throws if > capacity
    return st.Token.Kind;
  }

  static bool IsStandaloneSyntax(RawTokenKind kind) => kind is RawTokenKind.Terminator or RawTokenKind.CommentTrivia;

  static bool IsEof(RawToken tok) => tok.Kind == RawTokenKind.Eof;

  static SyntaxKind MapStandaloneSyntaxKind(RawTokenKind kind) => kind switch
  {
    RawTokenKind.Terminator => SyntaxKind.TerminatorToken,
    RawTokenKind.CommentTrivia => SyntaxKind.CommentToken,
    _ => SyntaxKind.ErrorToken
  };

  readonly RawToken GetErrorToken(RawTokenKind expected) => new(
    RawTokenKind.Error,
    Current.Offset,
    Length: 0,
    Current.Line,
    Current.Column,
    RawToken.ErrorKind.UnexpectedToken,
    [expected, Current.Kind]);

  public IReadOnlyList<Diag> Diagnostics => _diags;
}
