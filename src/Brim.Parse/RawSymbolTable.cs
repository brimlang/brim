using SymbolEntry = (Brim.Parse.RawKind singleKind, (string symbol, Brim.Parse.RawKind kind)[] multiSyms);

namespace Brim.Parse;

public static class RawSymbolTable
{
  public static readonly Dictionary<char, SymbolEntry> SymbolTable = new()
  {
    ['@'] = (RawKind.Atmark, [
        ("@{", RawKind.AtmarkLBrace),
        ("@}", RawKind.RBrace),
        ("@>", RawKind.AtmarkGreater)
    ]),
    [':'] = (RawKind.Colon, [
        (":*", RawKind.ColonStar),
        (":=", RawKind.ColonEqual),
        ("::", RawKind.ColonColon)
    ]),
    ['<'] = (RawKind.Less, [
        ("<<", RawKind.LessLess),
        ("<@", RawKind.LessAt)
    ]),
    ['='] = (RawKind.Equal, [("=>", RawKind.EqualGreater)]),
    ['*'] = (RawKind.Star, [("*{", RawKind.StarLBrace)]),
    ['~'] = (RawKind.Tilde, [("~=", RawKind.TildeEqual)]),
    ['|'] = (RawKind.Pipe, [("|{", RawKind.PipeLBrace)]),
    ['#'] = (RawKind.Hash, [("#(", RawKind.HashLParen)]),
    ['%'] = (RawKind.Percent, [("%{", RawKind.PercentLBrace)]),
    ['['] = (RawKind.LBracket, [("[[", RawKind.LBracketLBracket)]),
    [']'] = (RawKind.RBracket, [("]]", RawKind.RBracketRBracket)]),
    ['?'] = (RawKind.Question, [("?(", RawKind.QuestionLParen)]),
    ['-'] = (RawKind.Minus, []),
    ['&'] = (RawKind.Ampersand, []),
    ['('] = (RawKind.LParen, []),
    [')'] = (RawKind.RParen, []),
    ['{'] = (RawKind.LBrace, []),
    ['}'] = (RawKind.RBrace, []),
    [','] = (RawKind.Comma, []),
    ['^'] = (RawKind.Hat, []),
    ['+'] = (RawKind.Plus, []),
    ['>'] = (RawKind.Greater, []),
    ['/'] = (RawKind.Slash, []),
    ['\\'] = (RawKind.Backslash, []),
    ['.'] = (RawKind.Stop, []),
  };
}
