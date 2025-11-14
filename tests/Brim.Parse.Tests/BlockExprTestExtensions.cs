using System.Collections.Generic;
using System.Linq;
using Brim.Parse.Green;

namespace Brim.Parse.Tests;

internal static class BlockExprTestExtensions
{
  public static IEnumerable<GreenNode> StatementNodes(this BlockExpr block) =>
    block.StatementList.Elements.Select(e => e.Node);

  public static GreenNode LastStatement(this BlockExpr block) =>
    block.StatementNodes().Last();
}
