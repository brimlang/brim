using Brim.Parse.Collections;

namespace Brim.Parse.Producers;

/// <summary>
/// Simple pull-based token producer. Each call to <see cref="TryRead"/>
/// yields the next token. Must emit a single EOB token (RawKind.Eob)
/// exactly once, then return false thereafter.
/// </summary>
public interface ITokenProducer<T> : IBufferSource<T> where T : struct
{
}
