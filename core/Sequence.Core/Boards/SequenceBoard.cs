using System.Collections.Immutable;
using System.Linq;

namespace Sequence.Core.Boards
{
    internal sealed class SequenceBoard : IBoardType
    {
        public ImmutableArray<ImmutableArray<Tile>> Board => _board;

        private static readonly ImmutableArray<ImmutableArray<Tile>> _board;

        static SequenceBoard()
        {
            var b = new[]
            {
                new Tile[]
                {
                    null,
                    new Tile(Suit.Diamonds, Rank.Six),
                    new Tile(Suit.Diamonds, Rank.Seven),
                    new Tile(Suit.Diamonds, Rank.Eight),
                    new Tile(Suit.Diamonds, Rank.Nine),
                    new Tile(Suit.Diamonds, Rank.Ten),
                    new Tile(Suit.Diamonds, Rank.Queen),
                    new Tile(Suit.Diamonds, Rank.King),
                    new Tile(Suit.Diamonds, Rank.Ace),
                    null,
                },
                new Tile[]
                {
                    new Tile(Suit.Diamonds, Rank.Five),
                    new Tile(Suit.Hearts, Rank.Three),
                    new Tile(Suit.Hearts, Rank.Two),
                    new Tile(Suit.Spades, Rank.Two),
                    new Tile(Suit.Spades, Rank.Three),
                    new Tile(Suit.Spades, Rank.Four),
                    new Tile(Suit.Spades, Rank.Five),
                    new Tile(Suit.Spades, Rank.Six),
                    new Tile(Suit.Spades, Rank.Seven),
                    new Tile(Suit.Clubs, Rank.Ace),
                },
                new Tile[]
                {
                    new Tile(Suit.Diamonds, Rank.Four),
                    new Tile(Suit.Hearts, Rank.Four),
                    new Tile(Suit.Diamonds, Rank.King),
                    new Tile(Suit.Diamonds, Rank.Ace),
                    new Tile(Suit.Clubs, Rank.Ace),
                    new Tile(Suit.Clubs, Rank.King),
                    new Tile(Suit.Clubs, Rank.Queen),
                    new Tile(Suit.Clubs, Rank.Ten),
                    new Tile(Suit.Spades, Rank.Eight),
                    new Tile(Suit.Clubs, Rank.King),
                },
                new Tile[]
                {
                    new Tile(Suit.Diamonds, Rank.Three),
                    new Tile(Suit.Hearts, Rank.Five),
                    new Tile(Suit.Diamonds, Rank.Queen),
                    new Tile(Suit.Hearts, Rank.Queen),
                    new Tile(Suit.Hearts, Rank.Ten),
                    new Tile(Suit.Hearts, Rank.Nine),
                    new Tile(Suit.Hearts, Rank.Eight),
                    new Tile(Suit.Clubs, Rank.Nine),
                    new Tile(Suit.Spades, Rank.Nine),
                    new Tile(Suit.Clubs, Rank.Queen),
                },
                new Tile[]
                {
                    new Tile(Suit.Diamonds, Rank.Two),
                    new Tile(Suit.Hearts, Rank.Six),
                    new Tile(Suit.Diamonds, Rank.Ten),
                    new Tile(Suit.Hearts, Rank.King),
                    new Tile(Suit.Hearts, Rank.Three),
                    new Tile(Suit.Hearts, Rank.Two),
                    new Tile(Suit.Hearts, Rank.Seven),
                    new Tile(Suit.Clubs, Rank.Eight),
                    new Tile(Suit.Spades, Rank.Ten),
                    new Tile(Suit.Clubs, Rank.Ten),
                },
                new Tile[]
                {
                    new Tile(Suit.Spades, Rank.Ace),
                    new Tile(Suit.Hearts, Rank.Seven),
                    new Tile(Suit.Diamonds, Rank.Nine),
                    new Tile(Suit.Hearts, Rank.Ace),
                    new Tile(Suit.Hearts, Rank.Four),
                    new Tile(Suit.Hearts, Rank.Five),
                    new Tile(Suit.Hearts, Rank.Six),
                    new Tile(Suit.Clubs, Rank.Seven),
                    new Tile(Suit.Spades, Rank.Queen),
                    new Tile(Suit.Clubs, Rank.Nine),
                },
                new Tile[]
                {
                    new Tile(Suit.Spades, Rank.King),
                    new Tile(Suit.Hearts, Rank.Eight),
                    new Tile(Suit.Diamonds, Rank.Eight),
                    new Tile(Suit.Clubs, Rank.Two),
                    new Tile(Suit.Clubs, Rank.Three),
                    new Tile(Suit.Clubs, Rank.Four),
                    new Tile(Suit.Clubs, Rank.Five),
                    new Tile(Suit.Clubs, Rank.Six),
                    new Tile(Suit.Spades, Rank.King),
                    new Tile(Suit.Clubs, Rank.Eight),
                },
                new Tile[]
                {
                    new Tile(Suit.Spades, Rank.Queen),
                    new Tile(Suit.Hearts, Rank.Nine),
                    new Tile(Suit.Diamonds, Rank.Seven),
                    new Tile(Suit.Diamonds, Rank.Six),
                    new Tile(Suit.Diamonds, Rank.Five),
                    new Tile(Suit.Diamonds, Rank.Four),
                    new Tile(Suit.Diamonds, Rank.Three),
                    new Tile(Suit.Diamonds, Rank.Two),
                    new Tile(Suit.Spades, Rank.Ace),
                    new Tile(Suit.Clubs, Rank.Seven),
                },
                new Tile[]
                {
                    new Tile(Suit.Spades, Rank.Ten),
                    new Tile(Suit.Hearts, Rank.Ten),
                    new Tile(Suit.Hearts, Rank.Queen),
                    new Tile(Suit.Hearts, Rank.King),
                    new Tile(Suit.Hearts, Rank.Ace),
                    new Tile(Suit.Clubs, Rank.Two),
                    new Tile(Suit.Clubs, Rank.Three),
                    new Tile(Suit.Clubs, Rank.Four),
                    new Tile(Suit.Clubs, Rank.Five),
                    new Tile(Suit.Clubs, Rank.Six),
                },
                new Tile[]
                {
                    null,
                    new Tile(Suit.Spades, Rank.Nine),
                    new Tile(Suit.Spades, Rank.Eight),
                    new Tile(Suit.Spades, Rank.Seven),
                    new Tile(Suit.Spades, Rank.Six),
                    new Tile(Suit.Spades, Rank.Five),
                    new Tile(Suit.Spades, Rank.Four),
                    new Tile(Suit.Spades, Rank.Three),
                    new Tile(Suit.Spades, Rank.Two),
                    null,
                },
            };

            _board = b
                .Select(r => r.ToImmutableArray())
                .ToImmutableArray();
        }
    }
}
