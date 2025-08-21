using System.Globalization;

namespace Brim.Parse;

public sealed class Lexer(string source)
{
    readonly string _text = source ?? string.Empty;
    int _pos;      // offset in chars
    int _line = 1; // 1-based
    int _col = 1; // 1-based

    public Token Next()
    {
        SkipWhitespaceExceptNewline();

        if (Eof) return Make(TokenKind.Eof, 0);

        // Newlines as tokens
        if (Peek() is '\n')
        {
            Advance();
            return Make(TokenKind.Newline, 1);
        }
        if (Peek() is '\r')
        {
            Advance();
            if (!Eof && Peek() == '\n') Advance();
            return Make(TokenKind.Newline, 1);
        }

        // Multi-char lookaheads first
        if (Look("[[[")) return MakeAndAdvance(TokenKind.TripleLBracket, 3);
        if (Look("]]]")) return MakeAndAdvance(TokenKind.TripleRBracket, 3);
        if (Look("<<")) return MakeAndAdvance(TokenKind.ExportMarker, 2);
        if (Look("--- toml")) return MakeAndAdvance(TokenKind.FenceTomlStart, 8);
        if (Look("---") && !Look("--- toml")) return MakeAndAdvance(TokenKind.FenceTomlEnd, 3);
        if (Peek() == ':' && Peek(1) == '=') return MakeAndAdvance(TokenKind.VarDecl, 2);
        if (TryString(out Token sTok)) return sTok;
        if (TryInteger(out Token iTok)) return iTok;
        if (TryIdent(out Token idTok)) return idTok;

        // Single-char tokens
        return Peek() switch
        {
            '(' => MakeAndAdvance(TokenKind.LParen),
            ')' => MakeAndAdvance(TokenKind.RParen),
            '{' => MakeAndAdvance(TokenKind.LBrace),
            '}' => MakeAndAdvance(TokenKind.RBrace),
            '[' => MakeAndAdvance(TokenKind.LBracket),
            ']' => MakeAndAdvance(TokenKind.RBracket),
            ',' => MakeAndAdvance(TokenKind.Comma),
            ':' => MakeAndAdvance(TokenKind.Colon),
            '=' => MakeAndAdvance(TokenKind.Equal),
            '^' => MakeAndAdvance(TokenKind.Hat),
            '~' => MakeAndAdvance(TokenKind.Tilde),
            '|' => MakeAndAdvance(TokenKind.Pipe),
            '#' => MakeAndAdvance(TokenKind.Hash),
            '*' => MakeAndAdvance(TokenKind.Star),
            '&' => MakeAndAdvance(TokenKind.Ampersand),
            '<' => MakeAndAdvance(TokenKind.Less),
            '>' => MakeAndAdvance(TokenKind.Greater),
            '-' => MakeAndAdvance(TokenKind.Minus),
            _ => MakeAndAdvance(TokenKind.Ident), // fallback
        };
    }

    bool TryString(out Token t)
    {
        if (Peek() != '"') { t = default; return false; }
        int start = _pos; int startCol = _col;
        Advance(); // opening "
        while (!Eof)
        {
            char c = Peek();
            if (c == '\\') { Advance(); if (!Eof) Advance(); continue; }
            if (c == '"') { Advance(); t = Make(TokenKind.BrString, _pos - start, start, startCol); return true; }
            if (c is '\n' or '\r') break;
            Advance();
        }
        t = Make(TokenKind.BrString, _pos - start, start, startCol);
        return true;
    }

    bool TryInteger(out Token t)
    {
        if (!char.IsAsciiDigit(Peek())) { t = default; return false; }
        int start = _pos; int startCol = _col;
        while (!Eof && char.IsAsciiDigit(Peek())) Advance();
        t = Make(TokenKind.BrInteger, _pos - start, start, startCol);
        return true;
    }

    static bool IsIdentStart(char c)
    {
        if (c == '_' || char.IsLetter(c)) return true;
        UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
        return cat is UnicodeCategory.LetterNumber;
    }

    static bool IsIdentPart(char c)
    {
        if (c == '_' || char.IsLetterOrDigit(c)) return true;
        UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
        return cat is UnicodeCategory.LetterNumber or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.ConnectorPunctuation;
    }

    bool TryIdent(out Token t)
    {
        if (!IsIdentStart(Peek())) { t = default; return false; }
        int start = _pos; int startCol = _col;
        Advance();
        while (!Eof && IsIdentPart(Peek())) Advance();
        t = Make(TokenKind.Ident, _pos - start, start, startCol);
        return true;
    }

    void SkipWhitespaceExceptNewline()
    {
        while (!Eof)
        {
            char c = Peek();
            if (c is ' ' or '\t' or '\f') Advance();
            else break;
        }
    }

    bool Look(string s)
    {
        if (_pos + s.Length > _text.Length) return false;
        for (int i = 0; i < s.Length; i++)
            if (_text[_pos + i] != s[i]) return false;
        return true;
    }

    char Peek(int lookahead = 0) => _text[_pos + lookahead];
    bool Eof => _pos >= _text.Length;

    void Advance()
    {
        char c = _text[_pos++];
        if (c == '\n') { _line++; _col = 1; } else { _col++; }
    }

    Token MakeAndAdvance(TokenKind kind, int n = 1)
    {
        int start = _pos; int startCol = _col;
        for (int i = 0; i < n; i++) Advance();
        return Make(kind, n, start, startCol);
    }

    Token Make(TokenKind kind, int len, int? startOverride = null, int? colOverride = null)
    {
        int start = startOverride ?? (_pos - len);
        int col = colOverride ?? (_col - len);
        return new Token(kind, start, len, _line, col, _text.AsMemory(start, len));
    }
}

