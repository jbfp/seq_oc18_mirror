using System.Collections.Immutable;

namespace Sequence.Core
{
    public interface IBot
    {
        Move Decide(GameView gameView, IImmutableList<Move> moves);
    }
}
