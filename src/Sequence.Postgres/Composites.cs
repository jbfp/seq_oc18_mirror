using Sequence.Core;

namespace Sequence.Postgres
{
#pragma warning disable CS0649
    internal sealed class CardComposite
    {
        public DeckNo deckno;
        public Suit suit;
        public Rank rank;

        public static CardComposite FromCard(Card card)
        {
            if (card == null)
            {
                return null;
            }

            return new CardComposite
            {
                deckno = card.DeckNo,
                rank = card.Rank,
                suit = card.Suit,
            };
        }

        public Card ToCard() => new Card(deckno, suit, rank);
    }

    internal sealed class CoordComposite
    {
        public short col;
        public short row;

        public static CoordComposite FromCoord(Coord coord) => new CoordComposite
        {
            col = (short)coord.Column,
            row = (short)coord.Row,
        };

        public Coord ToCoord() => new Coord(col, row);
    }
#pragma warning restore CS0649
}
