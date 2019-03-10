using System;

namespace Sequence.GetGame
{
    public abstract class GameEventBase
    {
        protected GameEventBase(int version)
        {
            Version = version;
        }

        public int Version { get; }
    }

    public sealed class CardDiscarded : GameEventBase
    {
        public CardDiscarded(PlayerId byPlayerId, Card card, int version) : base(version)
        {
            ByPlayerId = byPlayerId ?? throw new ArgumentNullException(nameof(byPlayerId));
            Card = card ?? throw new ArgumentNullException(nameof(card));
        }

        public PlayerId ByPlayerId { get; }
        public Card Card { get; }
    }

    public sealed class ChipAdded : GameEventBase
    {
        public ChipAdded(Coord coord, Team team, int version) : base(version)
        {
            Coord = coord;
            Team = team;
        }

        public Coord Coord { get; }
        public Team Team { get; }
    }

    public sealed class ChipRemoved : GameEventBase
    {
        public ChipRemoved(Coord coord, int version) : base(version)
        {
            Coord = coord;
        }

        public Coord Coord { get; }
    }

    public sealed class CardDrawn : GameEventBase
    {
        public CardDrawn(PlayerId byPlayerId, Card card, int version) : base(version)
        {
            ByPlayerId = byPlayerId ?? throw new ArgumentNullException(nameof(byPlayerId));
            Card = card;
        }

        public PlayerId ByPlayerId { get; }
        public Card Card { get; }
    }

    public sealed class DeckShuffled : GameEventBase
    {
        public DeckShuffled(int numCardsInDeck, int version) : base(version)
        {
            NumCardsInDeck = numCardsInDeck;
        }

        public int NumCardsInDeck { get; }
    }

    public sealed class CardDied : GameEventBase
    {
        public CardDied(Card card, int version) : base(version)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
        }

        public Card Card { get; }
    }

    public sealed class CardRevived : GameEventBase
    {
        public CardRevived(Card card, int version) : base(version)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
        }

        public Card Card { get; }
    }

    public sealed class SequenceCreated : GameEventBase
    {
        public SequenceCreated(Seq sequence, int version) : base(version)
        {
            Sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        }

        public Seq Sequence { get; set; }
    }

    public sealed class TurnEnded : GameEventBase
    {
        public TurnEnded(PlayerId nextPlayerId, int version) : base(version)
        {
            NextPlayerId = nextPlayerId ?? throw new ArgumentNullException(nameof(nextPlayerId));
        }

        public PlayerId NextPlayerId { get; }
    }

    public sealed class GameEnded : GameEventBase
    {
        public GameEnded(Team winnerTeam, int version) : base(version)
        {
            WinnerTeam = winnerTeam;
        }

        public Team WinnerTeam { get; }
    }
}
