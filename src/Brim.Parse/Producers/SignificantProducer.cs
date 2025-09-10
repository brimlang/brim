namespace Brim.Parse.Producers;

/// <summary>
/// Transforms RawTokens into SignificantToken stream, preserving Terminator tokens and synthesizing EOB.
/// </summary>
public sealed class SignificantProducer<TProducer>(in TProducer inner) :
  ITokenProducer<SignificantToken>
  where TProducer : ITokenProducer<RawToken>
{
  enum State { Gathering, HaveCore }

  readonly TProducer _inner = inner;

  State _state = State.Gathering;
  RawToken _coreToken;
  List<RawToken>? _leading;
  bool _emittedEob;

  RawToken _look; // single token look ahead slot
  bool _haveLook;

  public bool IsEndOfSource(in SignificantToken item) => Tokens.IsEob(item);

  public bool TryRead(out SignificantToken tok)
  {
    tok = default;
    if (_emittedEob)
      return false;

    while (true)
    {
      switch (_state)
      {
        case State.Gathering:
          if (HandleGatheringState(out tok))
            return true;
          break;
        case State.HaveCore:
          if (HandleHaveCoreState(out tok))
            return true;
          break;
        default:
          tok = default;
          return false; // Should not hit under current states
      }
    }
  }

  static bool IsTrivia(RawKind k) => k is RawKind.WhitespaceTrivia or RawKind.CommentTrivia;

  static bool IsTerminator(RawKind k) => k == RawKind.Terminator;

  // Handles logic for the Gathering state. Returns true if a token is emitted.
  bool HandleGatheringState(out SignificantToken tok)
  {
    tok = default;
    RawToken next = PeekRaw();
    if (next.Kind == RawKind.Eob)
    {
      if (_coreToken.Kind > RawKind.Default)
      {
        tok = EmitCore();
        return true;
      }

      _emittedEob = true;
      tok = new SignificantToken(
        new RawToken(
          RawKind.Eob,
          next.Offset,
          Length: 0,
          next.Line,
          next.Column),
        LeadingTrivia: []);

      return true;
    }

    _ = Read(out next); // consume
    if (IsTrivia(next.Kind))
    {
      (_leading ??= []).Add(next);
      return false;
    }

    if (IsTerminator(next.Kind))
    {
      StructuralArray<RawToken> leading = _leading is { Count: > 0 } ? _leading.ToArray() : [];
      _leading = null;
      tok = new SignificantToken(next, leading);
      return true;
    }

    // start core
    _coreToken = next;
    _state = State.HaveCore;
    return false;
  }

  // Handles logic for the HaveCore state. Returns true if a token is emitted.
  bool HandleHaveCoreState(out SignificantToken tok)
  {
    RawToken look = PeekRaw();
    if (look.Kind == RawKind.Eob)
    {
      tok = EmitCore();
      return true;
    }

    if (IsTrivia(look.Kind))
    {
      // Emit the current core first (with its existing leading only), then start collecting
      // trivia as leading for the NEXT token.
      tok = EmitCore();
      _ = Read(out look); // consume trivia after emitting core
      (_leading ??= []).Add(look);
      return true;
    }

    if (IsTerminator(look.Kind))
    {
      tok = EmitCore();
      // terminator will be processed next loop pass as standalone
      return true;
    }

    // boundary: next significant starts, emit current
    tok = EmitCore();
    return true;
  }

  bool Read(out RawToken tok)
  {
    if (_haveLook)
    {
      tok = _look;
      _haveLook = false;
      return true;
    }

    if (_inner.TryRead(out tok)) return true;
    tok = default;
    return false;
  }

  RawToken PeekRaw()
  {
    if (!_haveLook && Read(out _look)) _haveLook = true;
    return _haveLook
      ? _look
      : new RawToken(RawKind.Eob, 0, 0, 0, 0);
  }

  SignificantToken EmitCore()
  {
    // Leading trivia attaches to the token if any was collected prior (always attach; never discard).
    StructuralArray<RawToken> leadingArr = _leading is { Count: > 0 }
      ? _leading.ToArray()
      : [];

    _leading = null;

    SignificantToken sig = new(_coreToken, leadingArr);

    _coreToken = default;
    // _leading may already hold trivia for the next token (if we just transitioned)
    _state = State.Gathering;
    return sig;
  }
}
