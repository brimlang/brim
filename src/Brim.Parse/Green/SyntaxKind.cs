namespace Brim.Parse.Green;

public enum SyntaxKind
{
  // Tokens
  ErrorToken,
  ModulePathOpenToken,
  ModulePathCloseToken,
  ModulePathSepToken,
  IdentifierToken,
  NumberToken,
  StrToken,
  EqualToken,
  StructToken,
  CloseBraceToken,
  ExportMarkerToken,
  ColonToken,
  WhiteSpaceToken,
  CommentToken,
  TerminatorToken,
  EofToken,

  // Nodes
  Module,
  FunctionDeclaration,
  StructDeclaration,
  ExportDeclaration,
  ImportDeclaration,
  ParameterList,
  FieldList,
  Block,
  ModuleHeader,
  ModulePath,
  ModuleDirective,
  FieldDeclaration,
}
