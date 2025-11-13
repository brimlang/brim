using Brim.Core.Collections;

namespace Brim.Core.Tests;

public class DiagnosticListTests
{
  [Fact]
  public void Add_CapsAtMaxDiagnostics()
  {
    DiagnosticList list = DiagnosticList.Create();

    for (int i = 0; i < DiagnosticList.MaxDiagnostics; i++)
      list.Add(new Diagnostic(DiagCode.UnexpectedToken, i, 1, i, 1));

    list.Add(new Diagnostic(DiagCode.UnexpectedToken, 999, 1, 999, 1));

    Assert.True(list.IsCapped);
    Assert.Equal(DiagnosticList.MaxDiagnostics, list.Count);

    ImmutableArray<Diagnostic> diagnostics = list.GetSortedDiagnostics();
    Diagnostic last = diagnostics[^1];

    Assert.Equal(DiagCode.TooManyErrors, last.Code);
    Assert.Equal(DiagnosticList.MaxDiagnostics - 1, last.Offset);
  }

  [Fact]
  public void GetSortedDiagnostics_OrdersByOffsetLineCode()
  {
    DiagnosticList list = DiagnosticList.Create();

    list.Add(new Diagnostic(DiagCode.InvalidIdentifierChar, 5, 1, 2, 2));
    list.Add(new Diagnostic(DiagCode.InvalidIdentifierStart, 0, 1, 0, 0));
    list.Add(new Diagnostic(DiagCode.InvalidIdentifierChar, 5, 1, 1, 1));

    ImmutableArray<Diagnostic> diagnostics = list.GetSortedDiagnostics();

    Assert.Equal(3, diagnostics.Length);
    Assert.Equal(DiagCode.InvalidIdentifierStart, diagnostics[0].Code);
    Assert.Equal(1, diagnostics[1].Line);
    Assert.Equal(2, diagnostics[2].Line);
    Assert.Equal(diagnostics[1].Offset, diagnostics[2].Offset);
  }
}
