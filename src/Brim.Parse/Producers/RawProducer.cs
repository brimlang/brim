using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using Brim.Parse.Collections;

namespace Brim.Parse.Producers;

/// <summary>
/// Streaming lexer producing RawTokens and a synthesized EOB token.
/// </summary>
public sealed class RawProducer(
    in SourceText source,
    in DiagnosticList sink) : ITokenProducer<RawToken>
{
  readonly SourceText _source = source;
  readonly DiagnosticList _sink = sink;

  int _pos; // 0.._span.Length
  int _line = 1;
  int _col = 1;
  bool _emittedEob;

  /// <summary>
  /// Keywords that should be recognized as specific tokens instead of identifiers.
  /// </summary>
  static bool TryGetKeyword(ReadOnlySpan<char> span, out RawKind kind)
  {
    // Keywords are all ASCII, so we can do a quick check before allocating a string
    if (span.Length == 0 || !BrimChars.IsAsciiLetter(span[0]))
    {
      kind = RawKind._SentinelDefault;
      return false;
    }

    (RawKind, bool) result = span switch
    {
      "true" => (RawKind.True, true),
      "false" => (RawKind.False, true),
      "void" => (RawKind.Void, true),
      "unit" => (RawKind.Unit, true),
      "bool" => (RawKind.Bool, true),
      "str" => (RawKind.Str, true),
      "rune" => (RawKind.Rune, true),
      "err" => (RawKind.Err, true),
      "seq" => (RawKind.Seq, true),
      "buf" => (RawKind.Buf, true),
      "i8" => (RawKind.I8, true),
      "i16" => (RawKind.I16, true),
      "i32" => (RawKind.I32, true),
      "i64" => (RawKind.I64, true),
      "u8" => (RawKind.U8, true),
      "u16" => (RawKind.U16, true),
      "u32" => (RawKind.U32, true),
      "u64" => (RawKind.U64, true),
      "f32" => (RawKind.F32, true),
      "f64" => (RawKind.F64, true),
      _ => (RawKind._SentinelDefault, false)
    };

    kind = result.Item1;
    return result.Item2;
  }

  public bool IsEndOfSource(in RawToken item) => Tokens.IsEob(item);

  public bool TryRead(out RawToken tok)
  {
    if (_emittedEob)
    {
      tok = default;
      return false;
    }

    if (_pos >= _source.Length)
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

      if (BrimChars.IsTerminator(c))
        return LexTerminator(startOffset, startLine, startCol);

      if (BrimChars.IsAllowedWhitespace(c) && !BrimChars.IsTerminator(c))
        return LexWhitespace(startOffset, startLine, startCol);

      // Reject unsupported Unicode whitespace
      if (char.IsWhiteSpace(c) && !BrimChars.IsAllowedWhitespace(c))
      {
        AdvanceChar();
        _sink.Add(Diagnostic.UnsupportedWhitespace(startOffset, startLine, startCol, c));
        return MakeToken(RawKind.Error, 0, startOffset, startLine, startCol);
      }

      if (c == '-' && PeekChar(1) == '-')
        return LexLineComment(startOffset, startLine, startCol);

      if (c == '"')
        return LexString(startOffset, startLine, startCol, _source.Span);

      if (c == '\'')
        return LexRuneLiteral(startOffset, startLine, startCol);

      if (char.IsDigit(c))
        return LexNumber(startOffset, startLine, startCol);

      if (BrimChars.IsIdentifierStart(c))
        return LexIdentifier(startOffset, startLine, startCol);

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
    AdvanceCharWhile(static c => BrimChars.IsAllowedWhitespace(c) && !BrimChars.IsTerminator(c));
    return new RawToken(RawKind.WhitespaceTrivia, startOffset, _pos - startOffset, line, col);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  RawToken LexIdentifier(int startOffset, int line, int col)
  {
    AdvanceCharWhile(static c => BrimChars.IsIdentifierContinue(c));
    int length = _pos - startOffset;

    // Get the identifier text and normalize it
    ReadOnlySpan<char> identifierSpan = _source.Span.Slice(startOffset, length);

    // Normalize the identifier and check if it's a keyword using TryGetKeyword
    string normalized = BrimChars.NormalizeIdentifier(identifierSpan);
    RawKind kind = TryGetKeyword(normalized, out RawKind keywordKind)
      ? keywordKind
      : RawKind.Identifier;

    return new RawToken(kind, startOffset, length, line, col);
  }

  RawToken LexNumber(int startOffset, int line, int col)
  {
    bool isDecimal = false;

    // Hexadecimal
    if (PeekChar(0) == '0' && (PeekChar(1) == 'x' || PeekChar(1) == 'X'))
    {
      AdvanceChar(); // 0
      AdvanceChar(); // x
      while (true)
      {
        char c = PeekChar(0);
        if (IsHexDigit(c)) { AdvanceChar(); }
        else if (c == '_' && IsHexDigit(PeekChar(1))) { AdvanceChar(); AdvanceChar(); }
        else { break; }
      }
    }
    // Binary
    else if (PeekChar(0) == '0' && (PeekChar(1) == 'b' || PeekChar(1) == 'B'))
    {
      AdvanceChar(); // 0
      AdvanceChar(); // b
      while (true)
      {
        char c = PeekChar(0);
        if (c is '0' or '1') { AdvanceChar(); }
        else if (c == '_' && (PeekChar(1) is '0' or '1')) { AdvanceChar(); AdvanceChar(); }
        else { break; }
      }
    }
    else
    {
      // Decimal or fractional
      while (true)
      {
        char c = PeekChar(0);
        if (char.IsDigit(c)) { AdvanceChar(); }
        else if (c == '_' && char.IsDigit(PeekChar(1))) { AdvanceChar(); AdvanceChar(); }
        else { break; }
      }

      if (PeekChar(0) == '.' && char.IsDigit(PeekChar(1)))
      {
        isDecimal = true;
        AdvanceChar(); // '.'
        while (true)
        {
          char c = PeekChar(0);
          if (char.IsDigit(c)) { AdvanceChar(); }
          else if (c == '_' && char.IsDigit(PeekChar(1))) { AdvanceChar(); AdvanceChar(); }
          else { break; }
        }
      }
    }

    if (isDecimal)
    {
      char c = PeekChar(0);
      if (c is 'f')
        AdvanceChar(); // consume f
      if (PeekChar(0) == '3' && PeekChar(1) == '2') { AdvanceChar(); AdvanceChar(); }
      else if (PeekChar(0) == '6' && PeekChar(1) == '4') { AdvanceChar(); AdvanceChar(); }
    }
    else
    {
      char c = PeekChar(0);
      if (c is 'i' or 'u')
      {
        AdvanceChar(); // consume i/u
        if (PeekChar(0) == '1' && PeekChar(1) == '6') { AdvanceChar(); AdvanceChar(); }
        else if (PeekChar(0) == '3' && PeekChar(1) == '2') { AdvanceChar(); AdvanceChar(); }
        else if (PeekChar(0) == '6' && PeekChar(1) == '4') { AdvanceChar(); AdvanceChar(); }
        else if (PeekChar(0) == '8') { AdvanceChar(); }
      }
    }

    RawKind kind = isDecimal ? RawKind.DecimalLiteral : RawKind.IntegerLiteral;
    return new RawToken(kind, startOffset, _pos - startOffset, line, col);

    static bool IsHexDigit(char c) =>
      char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  RawToken LexLineComment(int startOffset, int line, int col)
  {
    AdvanceChar(); // first -
    AdvanceChar(); // second -
    AdvanceCharWhile(static c => !BrimChars.IsTerminator(c));
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
    {
      RawToken errTok = new(RawKind.Error, startOffset, len == 0 ? 1 : len, line, col);
      _sink.Add(Diagnostic.UnterminatedString(errTok));
      return errTok;
    }

    return new RawToken(RawKind.StringLiteral, startOffset, len, line, col);
  }

  RawToken LexRuneLiteral(int startOffset, int line, int col)
  {
    ReadOnlySpan<char> span = _source.Span;
    AdvanceChar(); // consume opening '

    List<char> contentChars = []; // Collect chars between quotes for UTF-8 conversion
    bool foundClosing = false;

    // Parse content between quotes
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
        // Handle escape sequence
        AdvanceChar(); // consume \
        if (_pos >= span.Length) break;

        char escaped = span[_pos];
        char actualChar = escaped switch
        {
          'n' => '\n',
          't' => '\t',
          'r' => '\r',
          '\\' => '\\',
          '\'' => '\'',
          '\"' => '\"',
          '0' => '\0',
          _ => escaped // Unknown escape, keep as-is
        };
        contentChars.Add(actualChar);
        AdvanceChar(); // consume escaped character
      }
      else
      {
        contentChars.Add(c);
        AdvanceChar();
      }
    }

    if (foundClosing)
      AdvanceChar(); // consume closing '

    int len = _pos - startOffset;
    if (!foundClosing)
    {
      RawToken errTok = new(RawKind.Error, startOffset, len == 0 ? 1 : len, line, col);
      _sink.Add(Diagnostic.UnterminatedRune(errTok));
      return errTok;
    }

    // Convert collected chars to string, then to UTF-8 bytes for proper Rune validation
    string contentString = new([.. contentChars]);
    if (string.IsNullOrEmpty(contentString))
    {
      // Empty rune literal is lexically valid but semantically questionable
      return new RawToken(RawKind.RuneLiteral, startOffset, len, line, col);
    }

    // Use UTF-8 bytes to properly decode Unicode scalars
    byte[] utf8Bytes = Encoding.UTF8.GetBytes(contentString);
    OperationStatus status = Rune.DecodeFromUtf8(utf8Bytes, out _, out int bytesConsumed);

    if (status == OperationStatus.Done && bytesConsumed == utf8Bytes.Length)
    {
      // Successfully decoded exactly one Unicode scalar value
      return new RawToken(RawKind.RuneLiteral, startOffset, len, line, col);
    }
    else if (status == OperationStatus.Done && bytesConsumed < utf8Bytes.Length)
    {
      // Multiple Unicode scalars in rune literal
      RawToken errTok = new(RawKind.Error, startOffset, len, line, col);
      _sink.Add(Diagnostic.MultipleRunesInLiteral(errTok));
      return errTok;
    }
    else
    {
      // Invalid UTF-8 or malformed Unicode
      RawToken errTok = new(RawKind.Error, startOffset, len, line, col);
      _sink.Add(Diagnostic.InvalidRuneScalar(errTok));
      return errTok;
    }
  }
}
