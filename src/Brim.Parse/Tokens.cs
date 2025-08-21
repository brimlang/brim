namespace Brim.Parse;

public enum TokenKind
{
  Eof,

  // Punctuation / symbols
  LParen, RParen, LBrace, RBrace, Comma, Colon, Equal, VarDecl, // :=

  Hat, Tilde, Pipe, Hash, Star, Ampersand,
  LBracket, RBracket, Less, Greater, Minus,

  // Multi-char symbols
  TripleLBracket,    // [[[
  TripleRBracket,    // ]]]
  ExportMarker,      // <<

  // Front-matter (still allowed)
  FenceTomlStart,    // --- toml
  FenceTomlEnd,      // ---

  // Literals / ids
  Ident,
  BrInteger,
  BrString,

  // Helpers
  Newline,
}

public readonly record struct Token(
  TokenKind Kind,
  int Offset,
  int Length,
  int Line,
  int Column,
  ReadOnlyMemory<char> Slice)
{
  public override string ToString() => $"{Kind}@{Line}:{Column} \"{Slice}\"";
}

