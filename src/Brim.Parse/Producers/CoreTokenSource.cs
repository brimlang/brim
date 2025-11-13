using Brim.Lex;

namespace Brim.Parse.Producers;

/// <summary>
/// Adapts <see cref="LexToken"/>s into <see cref="CoreToken"/>s by attaching accumulated leading trivia.
/// </summary>
public sealed class CoreTokenSource(in ITokenSource<LexToken> source) : ITokenSource<CoreToken>
{
  readonly ITokenSource<LexToken> _inner = source;
  CoreToken? _eob;

  public bool IsEndOfSource(in CoreToken item) => item.TokenKind == TokenKind.Eob;

  public bool TryRead(out CoreToken item)
  {
    if (_eob.HasValue)
    {
      item = _eob.Value;
      return false;
    }

    ArrayBuilder<TriviaToken> leadingTrivia = [];

    while (_inner.TryRead(out LexToken lex))
    {
      if (lex.TokenKind.IsTrivia)
      {
        leadingTrivia.Add(TriviaToken.FromLexToken(lex));
        continue;
      }

      StructuralArray<TriviaToken> leading = leadingTrivia;
      leadingTrivia.Clear();

      if (lex.TokenKind is TokenKind.Eob)
      {
        _eob ??= CoreToken.FromLexToken(leading, lex);
        item = _eob.Value;
        return true;
      }

      item = CoreToken.FromLexToken(leading, lex);
      return true;
    }

    throw new InvalidOperationException("Unreachable code reached in CoreTokenSource.TryRead");
  }
}
