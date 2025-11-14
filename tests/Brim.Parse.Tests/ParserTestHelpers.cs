using Brim.Core;
using Brim.Core.Collections;
using Brim.Lex;
using Brim.Parse;
using Brim.Parse.Green;
using Brim.Parse.Producers;
using Xunit;

namespace Brim.Parse.Tests;

static class ParserTestHelpers
{
  public static BrimModule ParseModule(string source)
    => Parser.ParseModule(source);

  public static TMember GetMember<TMember>(BrimModule module, int index)
    where TMember : GreenNode
    => Assert.IsType<TMember>(module.Members[index]);

  public static Parser CreateParser(string source, out DiagnosticList diagnostics)
  {
    SourceText text = SourceText.From(source);
    diagnostics = DiagnosticList.Create();
    LexTokenSource lex = new(text, diagnostics);
    CoreTokenSource core = new(lex);
    RingBuffer<CoreToken> buffer = new(core, capacity: 4);
    return new Parser(buffer, diagnostics);
  }
}
