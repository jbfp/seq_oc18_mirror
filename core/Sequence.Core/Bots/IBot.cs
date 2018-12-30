namespace Sequence.Core
{
    public interface IBot
    {
        (Card, Coord) Decide(GameView game);
    }
}
