namespace Sequence
{
    public interface IGameEvent
    {
    }

    public sealed class CardDiscarded : IGameEvent
    {
        public CardDiscarded(PlayerId byPlayerId, Card card)
        {
            ByPlayerId = byPlayerId;
            Card = card;
        }

        public PlayerId ByPlayerId { get; }
        public Card Card { get; }
    }

    public sealed class ChipAdded : IGameEvent
    {
        public ChipAdded(Coord coord, Team team)
        {
            Coord = coord;
            Team = team;
        }

        public Coord Coord { get; }
        public Team Team { get; }
    }

    public sealed class ChipRemoved : IGameEvent
    {
        public ChipRemoved(Coord coord)
        {
            Coord = coord;
        }

        public Coord Coord { get; }
    }

    public sealed class CardDrawn : IGameEvent
    {
        public CardDrawn(PlayerId byPlayerId, Card? card)
        {
            ByPlayerId = byPlayerId;
            Card = card;
        }

        public PlayerId ByPlayerId { get; }
        public Card? Card { get; }
    }

    public sealed class DeckShuffled : IGameEvent
    {
        public DeckShuffled(int numCardsInDeck)
        {
            NumCardsInDeck = numCardsInDeck;
        }

        public int NumCardsInDeck { get; }
    }

    public sealed class CardDied : IGameEvent
    {
        public CardDied(Card card)
        {
            Card = card;
        }

        public Card Card { get; }
    }

    public sealed class CardRevived : IGameEvent
    {
        public CardRevived(Card card)
        {
            Card = card;
        }

        public Card Card { get; }
    }

    public sealed class SequenceCreated : IGameEvent
    {
        public SequenceCreated(Seq sequence)
        {
            Sequence = sequence;
        }

        public Seq Sequence { get; set; }
    }

    public sealed class TurnEnded : IGameEvent
    {
        public TurnEnded(PlayerId nextPlayerId)
        {
            NextPlayerId = nextPlayerId;
        }

        public PlayerId NextPlayerId { get; }
    }

    public sealed class GameEnded : IGameEvent
    {
        public GameEnded(Team winnerTeam)
        {
            WinnerTeam = winnerTeam;
        }

        public Team WinnerTeam { get; }
    }
}
