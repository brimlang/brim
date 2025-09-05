using Brim.Parse.Producers;

namespace Brim.Parse;

/// <summary>
/// Heap-based facade to drive the ref struct parser pipeline and surface results for tests / callers
/// that want to store diagnostics & module together without ref struct lifetime constraints.
/// </summary>
public sealed class ParseFacade
{
  public static (Green.BrimModule module, IReadOnlyList<Diagnostic> diagnostics) ParseModule(string sourceText)
  {
    SourceText st = SourceText.From(sourceText);
    DiagSink sink = DiagSink.Create();
    RawProducer raw = new(st, sink);
    SignificantProducer<RawProducer> sig = new(raw);
    LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> negdw = new(sig, capacity: 4);
    Parser p = new(negdw, sink);
    Green.BrimModule mod = p.ParseModule();
    return (mod, p.Diagnostics);
  }

  public static (Green.BrimModule module, IReadOnlyList<Diagnostic> diagnostics) ParseModule(SourceText st)
  {
    DiagSink sink = DiagSink.Create();
    RawProducer raw = new(st, sink);
    SignificantProducer<RawProducer> sig = new(raw);
    LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> negdw = new(sig, capacity: 4);
    Parser p = new(negdw, sink);
    Green.BrimModule mod = p.ParseModule();
    return (mod, p.Diagnostics);
  }
}
