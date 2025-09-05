namespace Brim.Parse;

/// <summary>
/// Immutable backing store for source characters.
/// Holds a <see cref="ReadOnlyMemory{T}"/> so copies of this value type are cheap and share the same text.
/// Does not precompute line start indices; callers needing line/column mapping should build that separately.
/// Use <see cref="GetCursor"/> to obtain a lightweight forward-only character cursor suitable for lexing.
/// </summary>
public readonly partial struct SourceText
{
  readonly ReadOnlyMemory<char> _text;

  internal SourceText(ReadOnlyMemory<char> text) => _text = text;

  public static SourceText From(string text) => new(text.AsMemory());

  public static SourceText FromFile(string path)
  {
    return new(File.ReadAllText(path).AsMemory())
    {
      FilePath = path
    };
  }

  /// <summary>Optional originating file path (empty if not provided).</summary>
  public readonly string FilePath { get; init; } = string.Empty;

  /// <summary>Monotonic version for incremental scenarios (reserved; currently always 0 unless set by caller).</summary>
  public readonly int Version { get; init; } = 0;

  /// <summary>Total number of characters in the underlying text.</summary>
  public readonly int Length => _text.Length;

  /// <summary>Returns the underlying characters as a span (valid for the lifetime of this <see cref="SourceText"/> copy).</summary>
  public readonly ReadOnlySpan<char> Span => _text.Span;

  public readonly ReadOnlySpan<char>.Enumerator GetEnumerator() => _text.Span.GetEnumerator();
}
