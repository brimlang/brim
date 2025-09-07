using System.Runtime.CompilerServices;

namespace Brim.Parse.Producers;

/// <summary>
/// Streaming lexer producing RawTokens and a synthesized EOB token.
/// </summary>
public sealed class RawProducer(
    in SourceText source,
    in DiagnosticList sink) :
ITokenProducer<RawToken>
{
  readonly SourceText _source = source;
  readonly DiagnosticList _sink = sink;

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
      tok = new(RawKind.Eob, _source.Length, Length: 0, _line, _col);
      _emittedEob = true;
      return true;
    }

    tok = LexNext();
    if (tok.Kind == RawKind.Eob)
    {
      _emittedEob = true;
    }

    return true;
  }

  char PeekChar(int k)
  {
    int idx = _pos + k;
    ReadOnlySpan<char> span = _source.Span;
    return idx < span.Length ? span[idx] : BrimChars.EOB;
  }

  void AdvanceChar()
  {
    ReadOnlySpan<char> span = _source.Span;
    if (_pos >= span.Length)
      return;

    char c = span[_pos];
    if (c == BrimChars.NewLine)
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

      if (BrimChars.IsNonTerminalWhitespace(c))
        return LexWhitespace(startOffset, startLine, startCol);

      if (BrimChars.IsTerminator(c))
        return LexTerminator(startOffset, startLine, startCol);

      if (c == '"')
        return LexString(startOffset, startLine, startCol, _source.Span);

      if (BrimChars.IsIdentifierStart(c))
        return LexIdentifier(startOffset, startLine, startCol);

      if (char.IsDigit(c))
        return LexNumber(startOffset, startLine, startCol);

      if (RawKindTable.TryMatch(span[_pos..], out RawKind kind, out int matchedLength))
        return MakeToken(kind, matchedLength, startOffset, startLine, startCol);

      _sink.Add(Diagnostic.InvalidChar(startOffset, startLine, startCol, c));
      return MakeToken(RawKind.Error, 1, startOffset, startLine, startCol);
    }

    return new RawToken(RawKind.Eob, _source.Length, 0, _line, _col);
  }

  RawToken MakeToken(RawKind kind, int length, int startOffset, int startLine, int startCol)
  {
    ReadOnlySpan<char> span = _source.Span;
    for (int i = 0; i < length && _pos < span.Length; i++)
      AdvanceChar();

    return new(kind, startOffset, length, startLine, startCol);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  RawToken LexWhitespace(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => BrimChars.IsNonTerminalWhitespace(c));
    return new RawToken(RawKind.WhitespaceTrivia, startOffset, _pos - startOffset, line, col);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  RawToken LexIdentifier(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => BrimChars.IsIdentifierPart(c));
    return new RawToken(RawKind.Identifier, startOffset, _pos - startOffset, line, col);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  RawToken LexNumber(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => char.IsDigit(c));
    return new RawToken(RawKind.NumberLiteral, startOffset, _pos - startOffset, line, col);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  RawToken LexLineComment(int startOffset, int line, int col)
  {
    AdvanceChar();
    AdvanceChar();
    AdvanceCharWhile(static c => c is not BrimChars.NewLine);
    return new RawToken(RawKind.CommentTrivia, startOffset, _pos - startOffset, line, col);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  RawToken LexTerminator(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => BrimChars.IsTerminator(c));
    return new RawToken(RawKind.Terminator, startOffset, _pos - startOffset, line, col);
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
      RawToken errTok = new(RawKind.Error, startOffset, len == 0 ? 1 : len, line, col);
      _sink.Add(Diagnostic.UnterminatedString(errTok));
      return errTok;
    }

    return new RawToken(RawKind.StringLiteral, startOffset, len, line, col);
  }
}
