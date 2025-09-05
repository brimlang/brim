using System.Collections.Generic;
using SymbolEntry = (Brim.Parse.RawTokenKind singleKind, (string symbol, Brim.Parse.RawTokenKind kind)[] multiSyms);

namespace Brim.Parse;

public static class RawSymbolTable
{
  public static readonly Dictionary<char, SymbolEntry> SymbolTable = new()
  {
    ['@'] = (RawTokenKind.Atmark, [
        ("@{", RawTokenKind.AtmarkLBrace),
        ("@}", RawTokenKind.RBrace),
        ("@>", RawTokenKind.AtmarkGreater)
    ]),
    [':'] = (RawTokenKind.Colon, [
        (":*", RawTokenKind.ColonStar),
        (":=", RawTokenKind.ColonEqual),
        ("::", RawTokenKind.ColonColon)
    ]),
    ['<'] = (RawTokenKind.Less, [
        ("<<", RawTokenKind.LessLess),
        ("<@", RawTokenKind.LessAt)
    ]),
    ['='] = (RawTokenKind.Equal, [("=>", RawTokenKind.EqualGreater)]),
    ['*'] = (RawTokenKind.Star, [("*{", RawTokenKind.StarLBrace)]),
    ['~'] = (RawTokenKind.Tilde, [("~=", RawTokenKind.TildeEqual)]),
    ['|'] = (RawTokenKind.Pipe, [("|{", RawTokenKind.PipeLBrace)]),
    ['#'] = (RawTokenKind.Hash, [("#(", RawTokenKind.HashLParen)]),
    ['%'] = (RawTokenKind.Percent, [("%{", RawTokenKind.PercentLBrace)]),
    ['['] = (RawTokenKind.LBracket, [("[[", RawTokenKind.LBracketLBracket)]),
    [']'] = (RawTokenKind.RBracket, [("]]", RawTokenKind.RBracketRBracket)]),
    ['?'] = (RawTokenKind.Question, [("?(", RawTokenKind.QuestionLParen)]),
    ['-'] = (RawTokenKind.Minus, []),
    ['&'] = (RawTokenKind.Ampersand, []),
    ['('] = (RawTokenKind.LParen, []),
    [')'] = (RawTokenKind.RParen, []),
    ['{'] = (RawTokenKind.LBrace, []),
    ['}'] = (RawTokenKind.RBrace, []),
    [','] = (RawTokenKind.Comma, []),
    ['^'] = (RawTokenKind.Hat, []),
    ['+'] = (RawTokenKind.Plus, []),
    ['>'] = (RawTokenKind.Greater, []),
    ['/'] = (RawTokenKind.Slash, []),
    ['\\'] = (RawTokenKind.Backslash, []),
    ['.'] = (RawTokenKind.Stop, []),
  };
}
