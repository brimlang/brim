using System.Text;

namespace Brim.Parse;

public static class BrimParser
{
  public static ParsedUnit ParseString(string source)
  {
    Lexer lx = new(source);
    string? module = null;
    string? toml = null;
    List<string> exports = [];
    bool hasTopLevelVar = false;

    Token tok;
    while ((tok = lx.Next()).Kind != TokenKind.Eof)
    {
      switch (tok.Kind)
      {
        case TokenKind.TripleLBracket:
          module = ReadHeaderContent(lx);
          break;

        case TokenKind.FenceTomlStart:
          toml = ReadTomlBlock(lx);
          break;

        case TokenKind.ExportMarker:
          // Expect: << Ident
          Token t = lx.Next();
          if (t.Kind == TokenKind.Ident)
            exports.Add(t.Slice.ToString().Trim());
          // Consume rest of line for cleanliness
          SkipRestOfLine(lx);
          break;

        case TokenKind.VarDecl:
          // Heuristic: consider top-level := as “module state present”
          hasTopLevelVar = true;
          break;
        case TokenKind.Eof:
          break;
        case TokenKind.LParen:
          break;
        case TokenKind.RParen:
          break;
        case TokenKind.LBrace:
          break;
        case TokenKind.RBrace:
          break;
        case TokenKind.Comma:
          break;
        case TokenKind.Colon:
          break;
        case TokenKind.Equal:
          break;
        case TokenKind.Hat:
          break;
        case TokenKind.Tilde:
          break;
        case TokenKind.Pipe:
          break;
        case TokenKind.Hash:
          break;
        case TokenKind.Star:
          break;
        case TokenKind.Ampersand:
          break;
        case TokenKind.LBracket:
          break;
        case TokenKind.RBracket:
          break;
        case TokenKind.Less:
          break;
        case TokenKind.Greater:
          break;
        case TokenKind.Minus:
          break;
        case TokenKind.TripleRBracket:
          break;
        case TokenKind.FenceTomlEnd:
          break;
        case TokenKind.Ident:
          break;
        case TokenKind.BrInteger:
          break;
        case TokenKind.BrString:
          break;
        case TokenKind.Newline:
          break;
        default:
          break;
      }
    }

    return new ParsedUnit(module?.Trim(), toml, exports, hasTopLevelVar);
  }

  static string? ReadHeaderContent(Lexer lx)
  {
    StringBuilder sb = new();
    Token t;
    while ((t = lx.Next()).Kind != TokenKind.Eof)
    {
      if (t.Kind == TokenKind.TripleRBracket) break;
      _ = sb.Append(t.Slice);
    }
    return sb.ToString();
  }

  static string? ReadTomlBlock(Lexer lx)
  {
    StringBuilder sb = new();
    Token t;
    while ((t = lx.Next()).Kind != TokenKind.Eof)
    {
      if (t.Kind == TokenKind.FenceTomlEnd) break;
      _ = sb.Append(t.Slice);
    }
    return sb.ToString().Trim();
  }

  static void SkipRestOfLine(Lexer lx)
  {
    Token t;
    while ((t = lx.Next()).Kind != TokenKind.Eof && t.Kind != TokenKind.Newline) { }
  }
}

