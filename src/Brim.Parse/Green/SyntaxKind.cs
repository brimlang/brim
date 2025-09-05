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
  UnionToken,
  CloseBraceToken,
  ExportMarkerToken,
  ColonToken,
  GenericOpenToken,
  GenericCloseToken,
  WhiteSpaceToken,
  CommentToken,
  TerminatorToken,
  EobToken,

  // Nodes
  Module,
  FunctionDeclaration,
  StructDeclaration,
  UnionDeclaration,
  ExportDeclaration,
  ImportDeclaration,
  ParameterList,
  GenericParameterList,
  GenericArgumentList,
  FieldList,
  Block,
  ModuleHeader,
  ModulePath,
  ModuleDirective,
  FieldDeclaration,
  UnionVariantDeclaration,
  GenericType,
}
