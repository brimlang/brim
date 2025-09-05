namespace Brim.Parse.Producers;

/// <summary>
/// Streaming lexer producing RawTokens and a synthesized EOB token.
/// </summary>
public sealed class RawProducer(
    in SourceText source,
    in DiagSink sink)
: ITokenProducer<RawToken>
{
  readonly SourceText _source = source;
  readonly DiagSink _sink = sink;

  int _pos; // 0.._span.Length
  int _line = 1;
  int _col = 1;
  bool _emittedEob;

  public bool TryRead(out RawToken tok)
  {
    if (_emittedEob)
    {
      tok = default;
      return false;
    }

    ReadOnlySpan<char> span = _source.Span;
    if (_pos >= span.Length)
    {
      tok = new(RawTokenKind.Eob, _source.Length, Length: 0, _line, _col);
      _emittedEob = true;
      return true;
    }

    tok = LexNext();
    if (tok.Kind == RawTokenKind.Eob)
    {
      _emittedEob = true;
    }

    return true;
  }

  bool Matches(string symbol) =>
    _source.Length - _pos >= symbol.Length &&
    _source.Span.Slice(_pos, symbol.Length).SequenceEqual(symbol);

  char PeekChar(int k)
  {
    int idx = _pos + k;
    ReadOnlySpan<char> span = _source.Span;
    return idx < span.Length ? span[idx] : Chars.EOB;
  }

  void AdvanceChar()
  {
    ReadOnlySpan<char> span = _source.Span;
    if (_pos >= span.Length)
      return;

    char c = span[_pos];
    if (c == Chars.NewLine)
    {
      _line++;
      _col = 1;
    }
    else { _col++; }

    _pos++;
  }

  void AdvanceCharWhile(Predicate<char> pred)
  {
    ReadOnlySpan<char> span = _source.Span;
    while (_pos < span.Length && pred(span[_pos]))
      AdvanceChar();
  }

  RawToken LexNext()
  {
    ReadOnlySpan<char> span = _source.Span;
    while (_pos < span.Length)
    {
      char c = span[_pos];
      int startOffset = _pos;
      int startLine = _line;
      int startCol = _col;

      if (c == '-' && PeekChar(1) == '-')
        return LexLineComment(startOffset, startLine, startCol);
      if (Chars.IsNonTerminalWhitespace(c))
        return LexWhitespace(startOffset, startLine, startCol);
      if (Chars.IsTerminator(c))
        return LexTerminator(startOffset, startLine, startCol);
      if (c == '"')
        return LexString(startOffset, startLine, startCol, _source.Span);
      if (Chars.IsIdentifierStart(c))
        return LexIdentifier(startOffset, startLine, startCol);
      if (char.IsDigit(c))
        return LexNumber(startOffset, startLine, startCol);

      if (RawSymbolTable.SymbolTable.TryGetValue(c, out (RawTokenKind singleKind, (string symbol, RawTokenKind kind)[] multiSyms) entry))
      {
        foreach ((string symbol, RawTokenKind kind) in entry.multiSyms)
        {
          if (Matches(symbol))
            return MakeToken(kind, symbol.Length, startOffset, startLine, startCol);
        }
        return MakeToken(entry.singleKind, 1, startOffset, startLine, startCol);
      }

      _sink.Add(DiagFactory.InvalidChar(startOffset, startLine, startCol, c));
      return MakeToken(RawTokenKind.Error, 1, startOffset, startLine, startCol);
    }
    return new RawToken(RawTokenKind.Eob, _source.Length, 0, _line, _col);
  }

  RawToken MakeToken(RawTokenKind kind, int length, int startOffset, int startLine, int startCol)
  {
    ReadOnlySpan<char> span = _source.Span;
    for (int i = 0; i < length && _pos < span.Length; i++)
      AdvanceChar();
    return new(kind, startOffset, length, startLine, startCol);
  }

  RawToken LexWhitespace(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => Chars.IsNonTerminalWhitespace(c));
    return new RawToken(RawTokenKind.WhitespaceTrivia, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexIdentifier(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => Chars.IsIdentifierPart(c));
    return new RawToken(RawTokenKind.Identifier, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexNumber(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => char.IsDigit(c));
    return new RawToken(RawTokenKind.NumberLiteral, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexLineComment(int startOffset, int line, int col)
  {
    AdvanceChar();
    AdvanceChar();
    AdvanceCharWhile(static c => c is not Chars.NewLine);
    return new RawToken(RawTokenKind.CommentTrivia, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexTerminator(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => Chars.IsTerminator(c));
    return new RawToken(RawTokenKind.Terminator, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexString(int startOffset, int line, int col, ReadOnlySpan<char> span)
  {
    AdvanceChar();
    while (_pos < span.Length)
    {
      char c = span[_pos];
      if (c == '"') { AdvanceChar(); break; }
      if (c == '\\')
      {
        AdvanceChar();
        if (_pos >= span.Length) break;
        AdvanceChar();
        continue;
      }
      AdvanceChar();
    }

    int len = _pos - startOffset;
    bool terminated = len > 0 && span[startOffset] == '"' && span[startOffset + len - 1] == '"';
    if (!terminated)
    {
      RawToken errTok = new(RawTokenKind.Error, startOffset, len == 0 ? 1 : len, line, col);
      _sink.Add(DiagFactory.UnterminatedString(errTok));
      return errTok;
    }

    return new RawToken(RawTokenKind.StringLiteral, startOffset, len, line, col);
  }
}
