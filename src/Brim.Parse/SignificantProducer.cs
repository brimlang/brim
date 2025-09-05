namespace Brim.Parse;

/// <summary>
/// Transforms RawTokens into SignificantToken stream, preserving Terminator tokens and synthesizing EOF.
/// Leading/trailing trivia attaches to first significant in block or preceding token (including terminators) as specified.
/// </summary>
public struct SignificantProducer<TInner>(
  TInner inner,
  SignificantProducer<TInner>.ReadDelegate reader) :
  ITokenProducer<SignificantToken>
  where TInner : struct
{
  public delegate bool ReadDelegate(ref TInner inner, out RawToken tok);

  enum State { Gathering, HaveCore, EmitPending, Done }

  State _state = State.Gathering;
  RawToken _core = default;
  List<RawToken>? _leading = null;
  List<RawToken>? _trailing = null;
  bool _emittedEof = false;

  RawToken _look = default; // single token look slot
  bool _haveLook = false;

  public bool TryRead(out SignificantToken tok)
  {
    tok = default;
    if (_emittedEof) return false;

    while (true)
    {
      if (_state == State.Gathering)
      {
        RawToken next = PeekRaw();
        if (next.Kind == RawTokenKind.Eof)
        {
          if (_core.Kind != 0)
          {
            tok = EmitCore();
            return true;
          }

          // emit EOF significant container
          _emittedEof = true;
          tok = new SignificantToken(
            new RawToken(
              RawTokenKind.Eof,
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
          continue;
        }

        if (IsTerm(next.Kind))
        {
          // Always attach accumulated trivia to this terminator (leading on first emission of that token)
          StructuralArray<RawToken> leading = _leading is { Count: > 0 }
            ? _leading.ToArray() 
            : StructuralArray.Empty<RawToken>();

          _leading = null;
          tok = new SignificantToken(next, leading, []);

          return true;
        }

        // start core
        _core = next;
        _state = State.HaveCore;
        continue;
      }
      else if (_state == State.HaveCore)
      {
        RawToken look = PeekRaw();
        if (look.Kind == RawTokenKind.Eof)
        {
          // EOF: finalize current core including trailing; then EOF pass handled in Gathering
          tok = EmitCore();
          return true;
        }

        if (IsTrivia(look.Kind))
        {
          _ = Read(out look);
          (_trailing ??= []).Add(look);
          continue;
        }

        if (IsTerm(look.Kind))
        {
          tok = EmitCore();
          // terminator will be processed next loop pass as standalone
          return true;
        }

        // boundary: next significant starts, emit current
        tok = EmitCore();
        return true;
      }
      else
      {
        tok = default;
        return false; // Should not hit under current states
      }
    }
  }

  bool Read(out RawToken tok)
  {
    if (_haveLook)
    {
      tok = _look;
      _haveLook = false;
      return true;
    }

    if (reader(ref inner, out tok)) return true;
    tok = default;
    return false;
  }

  RawToken PeekRaw()
  {
    if (!_haveLook && Read(out _look)) _haveLook = true;
    return _haveLook ? _look : new RawToken(RawTokenKind.Eof, 0, 0, 0, 0);
  }

  static bool IsTrivia(RawTokenKind k) => k is RawTokenKind.WhitespaceTrivia or RawTokenKind.CommentTrivia;

  static bool IsTerm(RawTokenKind k) => k == RawTokenKind.Terminator;

  SignificantToken EmitCore()
  {
    // Leading trivia attaches to the token if any was collected prior (always attach; never discard).
    StructuralArray<RawToken> leadingArr = _leading is { Count: > 0 }
      ? _leading.ToArray() 
      : StructuralArray.Empty<RawToken>();

    _leading = null;

    StructuralArray<RawToken> trailingArr = _trailing is { Count: > 0 }
      ? _trailing.ToArray()
      : StructuralArray.Empty<RawToken>();

    SignificantToken sig = new(_core, leadingArr, trailingArr);

    _core = default;
    _trailing = null;
    _state = State.Gathering;
    return sig;
  }
}
