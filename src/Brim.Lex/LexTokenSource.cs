using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Brim.Lex;

/// <summary>
/// Streaming lexer producing <see cref="LexToken"/> instances from raw source text.
/// </summary>
public sealed class LexTokenSource(SourceText source, DiagnosticList diagnostics) : ITokenSource<LexToken>
{
  readonly SourceText _source = source;
  DiagnosticList _diagnostics = diagnostics;

  int _pos; // 0..Length
  int _line = 1;
  int _col = 1;
  LexToken? _eob;

  public bool IsEndOfSource(in LexToken item) => item.TokenKind == TokenKind.Eob;

  public bool TryRead(out LexToken tok)
  {
    if (_eob.HasValue)
    {
      tok = _eob.Value;
      return false;
    }

    tok = LexNext();
    if (tok.TokenKind == TokenKind.Eob)
      _eob = tok;

    return true;
  }

  LexToken LexNext()
  {
    ReadOnlySpan<char> span = _source.Span;
    while (_pos < span.Length)
    {
      char c = span[_pos];
      int startOffset = _pos;
      int startLine = _line;
      int startCol = _col;

      if (BrimChars.IsTerminator(c))
        return LexTerminator(startOffset, startLine, startCol);

      if (BrimChars.IsAllowedWhitespace(c) && !BrimChars.IsTerminator(c))
        return LexWhitespace(startOffset, startLine, startCol);

      if (char.IsWhiteSpace(c) && !BrimChars.IsAllowedWhitespace(c))
      {
        _diagnostics.Add(Diagnostic.Lex.UnsupportedWhitespace(startOffset, startLine, startCol, c));
        AdvanceChar();
        continue;
      }

      if (c == '-' && PeekChar(1) == '-')
        return LexLineComment(startOffset, startLine, startCol);

      if (c == '"')
        return LexString(startOffset, startLine, startCol, span);

      if (c == '\'')
        return LexRuneLiteral(startOffset, startLine, startCol);

      if (char.IsDigit(c))
        return LexNumber(startOffset, startLine, startCol);

      if (BrimChars.IsIdentifierStart(c))
        return LexIdentifier(startOffset, startLine, startCol);

      if (CharacterTable.TryMatch(span[_pos..], out TokenKind kind, out int matchedLength))
        return MakeToken(kind, matchedLength, startOffset, startLine, startCol);

      _diagnostics.Add(Diagnostic.Lex.InvalidChar(startOffset, startLine, startCol, c));
      AdvanceChar();
      return new LexToken(TokenKind.Error, startOffset, 1, startLine, startCol);
    }

    return new LexToken(TokenKind.Eob, _source.Length, 0, _line, _col);
  }

  LexToken MakeToken(TokenKind kind, int length, int startOffset, int startLine, int startCol)
  {
    ReadOnlySpan<char> span = _source.Span;
    for (int i = 0; i < length && _pos < span.Length; i++)
      AdvanceChar();
    return new LexToken(kind, startOffset, length, startLine, startCol);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  LexToken LexWhitespace(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => BrimChars.IsAllowedWhitespace(c) && !BrimChars.IsTerminator(c));
    return new LexToken(TokenKind.WhitespaceTrivia, startOffset, _pos - startOffset, line, col);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  LexToken LexIdentifier(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => BrimChars.IsIdentifierContinue(c));
    int length = _pos - startOffset;
    return new LexToken(TokenKind.Identifier, startOffset, length, line, col);
  }

  LexToken LexNumber(int startOffset, int line, int col)
  {
    bool isDecimal = false;

    if (PeekChar(0) == '0' && (PeekChar(1) == 'x' || PeekChar(1) == 'X'))
    {
      AdvanceChar();
      AdvanceChar();
      while (true)
      {
        char c = PeekChar(0);
        if (IsHexDigit(c))
        {
          AdvanceChar();
        }
        else if (c == '_' && IsHexDigit(PeekChar(1)))
        {
          AdvanceChar();
          AdvanceChar();
        }
        else
        {
          break;
        }
      }
    }
    else if (PeekChar(0) == '0' && (PeekChar(1) == 'b' || PeekChar(1) == 'B'))
    {
      AdvanceChar();
      AdvanceChar();
      while (true)
      {
        char c = PeekChar(0);
        if (c is '0' or '1')
        {
          AdvanceChar();
        }
        else if (c == '_' && (PeekChar(1) is '0' or '1'))
        {
          AdvanceChar();
          AdvanceChar();
        }
        else
        {
          break;
        }
      }
    }
    else
    {
      while (true)
      {
        char c = PeekChar(0);
        if (char.IsDigit(c))
        {
          AdvanceChar();
        }
        else if (c == '_' && char.IsDigit(PeekChar(1))) { AdvanceChar(); AdvanceChar(); }
        else
        {
          break;
        }
      }

      if (PeekChar(0) == '.' && char.IsDigit(PeekChar(1)))
      {
        isDecimal = true;
        AdvanceChar();
        while (true)
        {
          char c = PeekChar(0);
          if (char.IsDigit(c))
          {
            AdvanceChar();
          }
          else if (c == '_' && char.IsDigit(PeekChar(1))) { AdvanceChar(); AdvanceChar(); }
          else
          {
            break;
          }
        }
      }
    }

    if (isDecimal)
    {
      char c = PeekChar(0);
      if (c is 'f') AdvanceChar();
      if (PeekChar(0) == '3' && PeekChar(1) == '2') { AdvanceChar(); AdvanceChar(); }
      else if (PeekChar(0) == '6' && PeekChar(1) == '4') { AdvanceChar(); AdvanceChar(); }
    }
    else
    {
      char c = PeekChar(0);
      if (c is 'i' or 'u')
      {
        AdvanceChar();
        if (PeekChar(0) == '1' && PeekChar(1) == '6') { AdvanceChar(); AdvanceChar(); }
        else if (PeekChar(0) == '3' && PeekChar(1) == '2') { AdvanceChar(); AdvanceChar(); }
        else if (PeekChar(0) == '6' && PeekChar(1) == '4') { AdvanceChar(); AdvanceChar(); }
        else if (PeekChar(0) == '8') { AdvanceChar(); }
      }
    }

    TokenKind kind = isDecimal ? TokenKind.DecimalLiteral : TokenKind.IntegerLiteral;
    return new LexToken(kind, startOffset, _pos - startOffset, line, col);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  LexToken LexLineComment(int startOffset, int line, int col)
  {
    AdvanceChar();
    AdvanceChar();
    AdvanceCharWhile(static c => !BrimChars.IsTerminator(c));
    return new LexToken(TokenKind.CommentTrivia, startOffset, _pos - startOffset, line, col);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  LexToken LexTerminator(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => BrimChars.IsTerminator(c));
    return new LexToken(TokenKind.Terminator, startOffset, _pos - startOffset, line, col);
  }

  LexToken LexString(int startOffset, int line, int col, ReadOnlySpan<char> span)
  {
    AdvanceChar(); // consume opening "
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
      _diagnostics.Add(Diagnostic.Lex.UnterminatedString(startOffset, len == 0 ? 1 : len, line, col));

    return new LexToken(TokenKind.StringLiteral, startOffset, len, line, col);
  }

  LexToken LexRuneLiteral(int startOffset, int line, int col)
  {
    ReadOnlySpan<char> span = _source.Span;
    AdvanceChar(); // opening '

    List<char> contentChars = [];
    bool foundClosing = false;

    while (_pos < span.Length)
    {
      char c = span[_pos];
      if (c == '\'')
      {
        foundClosing = true;
        break;
      }

      if (c == '\\')
      {
        AdvanceChar();
        if (_pos >= span.Length) break;
        char escaped = span[_pos];
        char actualChar = escaped switch
        {
          'n' => '\n',
          't' => '\t',
          'r' => '\r',
          '\\' => '\\',
          '\'' => '\'',
          '"' => '"',
          '0' => '\0',
          _ => escaped
        };
        contentChars.Add(actualChar);
        AdvanceChar();
      }
      else
      {
        contentChars.Add(c);
        AdvanceChar();
      }
    }

    if (foundClosing)
      AdvanceChar();

    int len = _pos - startOffset;
    if (!foundClosing)
    {
      _diagnostics.Add(Diagnostic.Lex.UnterminatedRune(startOffset, len == 0 ? 1 : len, line, col));
      return new LexToken(TokenKind.RuneLiteral, startOffset, len == 0 ? 1 : len, line, col);
    }

    string contentString = new([.. contentChars]);
    if (string.IsNullOrEmpty(contentString))
      return new LexToken(TokenKind.RuneLiteral, startOffset, len, line, col);

    byte[] utf8Bytes = Encoding.UTF8.GetBytes(contentString);
    OperationStatus status = Rune.DecodeFromUtf8(utf8Bytes, out _, out int bytesConsumed);

    if (status == OperationStatus.Done && bytesConsumed == utf8Bytes.Length)
      return new LexToken(TokenKind.RuneLiteral, startOffset, len, line, col);

    if (status == OperationStatus.Done && bytesConsumed < utf8Bytes.Length)
      _diagnostics.Add(Diagnostic.Lex.MultipleRunesInLiteral(startOffset, len, line, col));
    else
      _diagnostics.Add(Diagnostic.Lex.InvalidRuneScalar(startOffset, len, line, col));

    return new LexToken(TokenKind.RuneLiteral, startOffset, len, line, col);
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
    else
    {
      _col++;
    }

    _pos++;
  }

  void AdvanceCharWhile(Predicate<char> predicate)
  {
    ReadOnlySpan<char> span = _source.Span;
    while (_pos < span.Length && predicate(span[_pos]))
      AdvanceChar();
  }

  static bool IsHexDigit(char c) =>
    char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
}
