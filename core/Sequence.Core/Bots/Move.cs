using System;

namespace Sequence.Core
{
    public sealed class Move : IEquatable<Move>
    {
        public Move(Card card, Coord coord)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
            Coord = coord;
        }

        public Card Card { get; }
        public Coord Coord { get; }

        public void Deconstruct(out Card card, out Coord coord)
        {
            card = Card;
            coord = Coord;
        }

        public bool Equals(Move other)
        {
            return other != null
                && Card.Equals(other.Card)
                && Coord.Equals(other.Coord);
        }

        public override bool Equals(object obj) => Equals(obj as Move);

        public override int GetHashCode() => new { Card, Coord }.GetHashCode();

        public override string ToString() => $"{Card} @ {Coord}";
    }
}
