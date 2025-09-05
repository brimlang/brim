namespace Brim.Parse;

/// <summary>
/// Streaming lexer producing RawTokens and a synthesized EOF token.
/// </summary>
public struct RawTokenProducer(SourceText source, List<Diag>? diags = null) :
  ITokenProducer<RawToken>
{
  int _pos = 0; // 0.._span.Length
  int _line = 1;
  int _col = 1;
  bool _emittedEof = false;

  public bool TryRead(out RawToken tok)
  {
    if (_emittedEof)
    {
      tok = default;
      return false;
    }

    ReadOnlySpan<char> span = source.Span;
    if (_pos >= span.Length)
    {
      tok = new RawToken(RawTokenKind.Eof, source.Length, 0, _line, _col);
      _emittedEof = true;
      return true;
    }

    tok = LexNext();
    if (tok is { Kind: RawTokenKind.Eof })
    {
      _emittedEof = true;
    }

    return true;
  }

  RawToken LexNext()
  {
    ReadOnlySpan<char> span = source.Span;
    while (_pos < span.Length)
    {
      char c = span[_pos];
      int startOffset = _pos;
      int startLine = _line;
      int startCol = _col;

      if (c == '-' && PeekChar(1) == '-')
        return LexLineComment(startOffset, startLine, startCol);

      if (Utilities.IsNonTerminalWhitespace(c))
        return LexWhitespace(startOffset, startLine, startCol);

      if (c is '\n' or ';')
        return LexTerminator(startOffset, startLine, startCol);

      if (c == '"')
        return LexString(startOffset, startLine, startCol);

      if (Utilities.IsIdentifierStart(c))
        return LexIdentifier(startOffset, startLine, startCol);

      if (char.IsDigit(c))
        return LexNumber(startOffset, startLine, startCol);

      (RawTokenKind singleKind, (string symbol, RawTokenKind kind)[] multiSyms) entry;
      if (RawSymbolTable.SymbolTable.TryGetValue(c, out entry))
      {
        // multi first
        foreach ((string symbol, RawTokenKind kind) in entry.multiSyms)
        {
          if (Matches(symbol))
            return MakeToken(kind, symbol.Length, startOffset, startLine, startCol);
        }

        return MakeToken(entry.singleKind, 1, startOffset, startLine, startCol);
      }

      // unexpected
  diags?.Add(DiagFactory.InvalidChar(startOffset, startLine, startCol, c));
      return MakeToken(RawTokenKind.Error, 1, startOffset, startLine, startCol, RawToken.ErrorKind.UnexpectedChar, new object[] { c });
    }

    return new RawToken(RawTokenKind.Eof, source.Length, 0, _line, _col);
  }

  readonly bool Matches(string symbol) => source.Length - _pos < symbol.Length
    ? false
    : source.Span.Slice(_pos, symbol.Length).SequenceEqual(symbol);

  readonly char PeekChar(int k)
  {
    int idx = _pos + k;
    ReadOnlySpan<char> span = source.Span;
    return idx < span.Length ? span[idx] : '\0';
  }

  RawToken MakeToken(RawTokenKind kind, int length, int startOffset, int startLine, int startCol, RawToken.ErrorKind errorKind = RawToken.ErrorKind.None, object[]? args = null)
  {
    ReadOnlySpan<char> span = source.Span; // for bounds
    for (int i = 0; i < length && _pos < span.Length; i++)
      AdvanceChar();

    return new RawToken(kind, startOffset, length, startLine, startCol, errorKind, args);
  }

  void AdvanceChar()
  {
    ReadOnlySpan<char> span = source.Span;
    if (_pos >= span.Length)
      return;

    char c = span[_pos];
    if (c == '\n') { _line++; _col = 1; }
    else { _col++; }
    _pos++;
  }

  void AdvanceCharWhile(Predicate<char> pred)
  {
    ReadOnlySpan<char> span = source.Span;
    while (_pos < span.Length && pred(span[_pos]))
      AdvanceChar();
  }

  RawToken LexWhitespace(int startOffset, int line, int col)
  {
    ReadOnlySpan<char> span = source.Span;
    AdvanceCharWhile(c => Utilities.IsNonTerminalWhitespace(c));
    return new RawToken(RawTokenKind.WhitespaceTrivia, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexIdentifier(int startOffset, int line, int col)
  {
    ReadOnlySpan<char> span = source.Span;
    AdvanceCharWhile(c => Utilities.IsIdentifierPart(c));
    return new RawToken(RawTokenKind.Identifier, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexNumber(int startOffset, int line, int col)
  {
    ReadOnlySpan<char> span = source.Span;
    AdvanceCharWhile(c => char.IsDigit(c));
    return new RawToken(RawTokenKind.NumberLiteral, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexLineComment(int startOffset, int line, int col)
  {
    AdvanceChar(); // -
    AdvanceChar(); // -
    ReadOnlySpan<char> span = source.Span;
    AdvanceCharWhile(c => c != '\n');
    return new RawToken(RawTokenKind.CommentTrivia, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexTerminator(int startOffset, int line, int col)
  {
    ReadOnlySpan<char> span = source.Span;
    AdvanceCharWhile(c => c is '\n' or ';');
    return new RawToken(RawTokenKind.Terminator, startOffset, _pos - startOffset, line, col);
  }

  RawToken LexString(int startOffset, int line, int col)
  {
    AdvanceChar(); // opening quote

    ReadOnlySpan<char> span = source.Span;
    while (_pos < span.Length)
    {
      char c = span[_pos];
      if (c == '"')
      {
        AdvanceChar();
        break;
      }

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

    bool terminated = len > 0 && source.Span[startOffset] == '"' && source.Span[startOffset + len - 1] == '"';
    if (!terminated)
    {
      RawToken errTok = new(RawTokenKind.Error, startOffset, len == 0 ? 1 : len, line, col, RawToken.ErrorKind.UnterminatedString, null);
  diags?.Add(DiagFactory.UnterminatedString(errTok));
      return errTok;
    }
    return new RawToken(RawTokenKind.StringLiteral, startOffset, len, line, col);
  }
}
