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
  List<RawToken>? _trailing;
  bool _emittedEob;

  RawToken _look; // single token look ahead slot
  bool _haveLook;

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

  static bool IsTrivia(RawTokenKind k) => k is RawTokenKind.WhitespaceTrivia or RawTokenKind.CommentTrivia;

  static bool IsTerminator(RawTokenKind k) => k == RawTokenKind.Terminator;

  // Handles logic for the Gathering state. Returns true if a token is emitted.
  bool HandleGatheringState(out SignificantToken tok)
  {
    tok = default;
    RawToken next = PeekRaw();
    if (next.Kind == RawTokenKind.Eob)
    {
      if (_coreToken.Kind > RawTokenKind.Default)
      {
        tok = EmitCore();
        return true;
      }

      _emittedEob = true;
      tok = new SignificantToken(
        new RawToken(
          RawTokenKind.Eob,
          next.Offset,
          Length: 0,
          next.Line,
          next.Column),
        LeadingTrivia: [],
        TrailingTrivia: []);

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
      // Always attach accumulated trivia to this terminator (leading on first emission of that token)
      StructuralArray<RawToken> leading = _leading is { Count: > 0 }
        ? _leading.ToArray()
        : [];

      _leading = null;
      tok = new SignificantToken(next, leading, []);
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
    tok = default;
    RawToken look = PeekRaw();
    if (look.Kind == RawTokenKind.Eob)
    {
      // EOB: finalize current core including trailing; then EOB pass handled in Gathering
      tok = EmitCore();
      return true;
    }

    if (IsTrivia(look.Kind))
    {
      _ = Read(out look);
      (_trailing ??= []).Add(look);
      return false;
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
      : new RawToken(RawTokenKind.Eob, 0, 0, 0, 0);
  }

  SignificantToken EmitCore()
  {
    // Leading trivia attaches to the token if any was collected prior (always attach; never discard).
    StructuralArray<RawToken> leadingArr = _leading is { Count: > 0 }
      ? _leading.ToArray()
      : [];

    _leading = null;

    StructuralArray<RawToken> trailingArr = _trailing is { Count: > 0 }
      ? _trailing.ToArray()
      : [];

    SignificantToken sig = new(_coreToken, leadingArr, trailingArr);

    _coreToken = default;
    _trailing = null;
    _state = State.Gathering;
    return sig;
  }
}
