using System.Collections.Immutable;
using System.Diagnostics;
using Brim.Parse.Green;

namespace Brim.Parse;

public partial struct Parser
{
  LookAheadWindow<SignificantToken, SignificantProducer<RawTokenProducer>> _look;

  public Parser(SourceText source)
  {
    RawTokenProducer raw = new(source);

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
    while (!IsEof(Current))
    {
      bool progressed = false;
      // Standalone syntax (terminators/comments) handled directly.
      if (IsStandaloneSyntax(Current.Kind))
      {
        members.Add(new GreenToken(MapStandaloneSyntaxKind(Current.Kind), Current));
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
        // Always advance at least one token to guarantee progress
        members.Add(new GreenToken(SyntaxKind.ErrorToken, Current)
        {
          Diagnostics = [Diagnostic.UnexpectedToken(Current.Kind)]
        });

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
    GreenToken eof = new(SyntaxKind.EofToken, Current);
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
      RawToken token;
      if (Match(tokenKind))
      {
        token = Current;
        _ = Advance();
      }
      else
      {
        token = GetErrorToken(tokenKind);
      }

      if (token.Kind != RawTokenKind.Error)
        return new GreenToken(kind, token);
    }

    return new GreenToken(kind, Current)
    {
      Diagnostics = [Diagnostic.UnexpectedToken(Current.Kind)]
    };
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
}
