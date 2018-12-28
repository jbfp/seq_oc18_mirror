using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.Core
{
    public sealed class Board
    {
        internal static readonly ImmutableArray<ImmutableArray<(Suit, Rank)?>> TheBoard;

        static Board()
        {
            var b = new[]
            {
                new (Suit, Rank)?[]
                {
                    null,
                    (Suit.Diamonds, Rank.Ten),
                    (Suit.Diamonds, Rank.Nine),
                    (Suit.Diamonds, Rank.Eight),
                    (Suit.Diamonds, Rank.Seven),
                    (Suit.Spades, Rank.Seven),
                    (Suit.Spades, Rank.Eight),
                    (Suit.Spades, Rank.Nine),
                    (Suit.Spades, Rank.Ten),
                    null,
                },
                new (Suit, Rank)?[]
                {
                    (Suit.Clubs, Rank.Ten),
                    (Suit.Clubs, Rank.King),
                    (Suit.Diamonds, Rank.Six),
                    (Suit.Diamonds, Rank.Five),
                    (Suit.Diamonds, Rank.Four),
                    (Suit.Spades, Rank.Four),
                    (Suit.Spades, Rank.Five),
                    (Suit.Spades, Rank.Six),
                    (Suit.Hearts, Rank.King),
                    (Suit.Hearts, Rank.Ten),
                },
                new (Suit, Rank)?[]
                {
                    (Suit.Clubs, Rank.Nine),
                    (Suit.Clubs, Rank.Six),
                    (Suit.Clubs, Rank.Queen),
                    (Suit.Diamonds, Rank.Three),
                    (Suit.Diamonds, Rank.Two),
                    (Suit.Spades, Rank.Two),
                    (Suit.Spades, Rank.Three),
                    (Suit.Hearts, Rank.Queen),
                    (Suit.Hearts, Rank.Six),
                    (Suit.Hearts, Rank.Nine),
                },
                new (Suit, Rank)?[]
                {
                    (Suit.Clubs, Rank.Eight),
                    (Suit.Clubs, Rank.Five),
                    (Suit.Clubs, Rank.Three),
                    (Suit.Diamonds, Rank.Queen),
                    (Suit.Diamonds, Rank.Ace),
                    (Suit.Spades, Rank.Ace),
                    (Suit.Spades, Rank.Queen),
                    (Suit.Hearts, Rank.Three),
                    (Suit.Hearts, Rank.Five),
                    (Suit.Hearts, Rank.Eight),
                },
                new (Suit, Rank)?[]
                {
                    (Suit.Clubs, Rank.Seven),
                    (Suit.Clubs, Rank.Four),
                    (Suit.Clubs, Rank.Two),
                    (Suit.Clubs, Rank.Ace),
                    (Suit.Diamonds, Rank.King),
                    (Suit.Spades, Rank.King),
                    (Suit.Hearts, Rank.Ace),
                    (Suit.Hearts, Rank.Two),
                    (Suit.Hearts, Rank.Four),
                    (Suit.Hearts, Rank.Seven),
                },
                new (Suit, Rank)?[] {
                    (Suit.Hearts, Rank.Seven),
                    (Suit.Hearts, Rank.Four),
                    (Suit.Hearts, Rank.Two),
                    (Suit.Hearts, Rank.Ace),
                    (Suit.Spades, Rank.King),
                    (Suit.Diamonds, Rank.King),
                    (Suit.Clubs, Rank.Ace),
                    (Suit.Clubs, Rank.Two),
                    (Suit.Clubs, Rank.Four),
                    (Suit.Clubs, Rank.Seven),
                },
                new (Suit, Rank)?[] {
                    (Suit.Hearts, Rank.Eight),
                    (Suit.Hearts, Rank.Five),
                    (Suit.Hearts, Rank.Three),
                    (Suit.Spades, Rank.Queen),
                    (Suit.Spades, Rank.Ace),
                    (Suit.Diamonds, Rank.Ace),
                    (Suit.Diamonds, Rank.Queen),
                    (Suit.Clubs, Rank.Three),
                    (Suit.Clubs, Rank.Five),
                    (Suit.Clubs, Rank.Eight),
                },
                new (Suit, Rank)?[] {
                    (Suit.Hearts, Rank.Nine),
                    (Suit.Hearts, Rank.Six),
                    (Suit.Hearts, Rank.Queen),
                    (Suit.Spades, Rank.Three),
                    (Suit.Spades, Rank.Two),
                    (Suit.Diamonds, Rank.Two),
                    (Suit.Diamonds, Rank.Three),
                    (Suit.Clubs, Rank.Queen),
                    (Suit.Clubs, Rank.Six),
                    (Suit.Clubs, Rank.Nine),
                },
                new (Suit, Rank)?[] {
                    (Suit.Hearts, Rank.Ten),
                    (Suit.Hearts, Rank.King),
                    (Suit.Spades, Rank.Six),
                    (Suit.Spades, Rank.Five),
                    (Suit.Spades, Rank.Four),
                    (Suit.Diamonds, Rank.Four),
                    (Suit.Diamonds, Rank.Five),
                    (Suit.Diamonds, Rank.Six),
                    (Suit.Clubs, Rank.King),
                    (Suit.Clubs, Rank.Ten),
                },
                new (Suit, Rank)?[] {
                    null,
                    (Suit.Spades, Rank.Ten),
                    (Suit.Spades, Rank.Nine),
                    (Suit.Spades, Rank.Eight),
                    (Suit.Spades, Rank.Seven),
                    (Suit.Diamonds, Rank.Seven),
                    (Suit.Diamonds, Rank.Eight),
                    (Suit.Diamonds, Rank.Nine),
                    (Suit.Diamonds, Rank.Ten),
                    null,
                },
            };

            TheBoard = b
                .Select(r => r.ToImmutableArray())
                .ToImmutableArray();
        }

        internal static bool Matches(Coord coord, Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            var row = coord.Row;
            var column = coord.Column;

            if (row < 0 || row >= TheBoard.Length)
            {
                return false;
            }

            var theRow = TheBoard[row];

            if (column < 0 || column >= theRow.Length)
            {
                return false;
            }

            if (card.IsTwoEyedJack())
            {
                return true;
            }

            var match = theRow[column];

            return match.HasValue
                && card.Suit == match.Value.Item1
                && card.Rank == match.Value.Item2;
        }

        internal Board()
        {
            Chips = ImmutableDictionary<Coord, Team>.Empty;
        }

        internal IImmutableDictionary<Coord, Team> Chips { get; private set; }

        internal bool IsOccupied(Coord coord) => Chips.ContainsKey(coord);

        internal void Add(Coord coord, Team? chip)
        {
            if (chip == null)
            {
                Chips = Chips.Remove(coord);
            }
            else
            {
                Chips = Chips.Add(coord, chip.Value);
            }
        }

        public static Seq GetSequence(
            IImmutableDictionary<Coord, Team> chips,
            Coord coord,
            Team team)
        {
            if (chips == null)
            {
                throw new ArgumentNullException(nameof(chips));
            }

            var row = coord.Row;
            var col = coord.Column;

            bool TrySequence(IEnumerable<Coord> cs, out IImmutableList<Coord> seq)
            {
                seq = ImmutableList<Coord>.Empty;

                foreach (var c in cs)
                {
                    var isCorner =
                        (c.Row >= 0 && c.Row < TheBoard.Length) &&
                        (c.Column >= 0 && c.Column < TheBoard[c.Row].Length) &&
                        TheBoard[c.Row][c.Column] == null;

                    if (isCorner || chips.TryGetValue(c, out var t) && team == t)
                    {
                        seq = seq.Add(c);

                        if (seq.Count == Seq.DefaultLength)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        seq = ImmutableList<Coord>.Empty;
                    }
                }

                return false;
            }

            var range = Enumerable.Range(-5, 11);
            var vertical = range.Select(d => new Coord(col, row + d));
            var horizontal = range.Select(d => new Coord(col + d, row));
            var diagonal1 = range.Select(d => new Coord(col + d, row + d));
            var diagonal2 = range.Select(d => new Coord(col + d, row - d));

            IImmutableList<Coord> coords;

            if (TrySequence(vertical, out coords) ||
                TrySequence(horizontal, out coords) ||
                TrySequence(diagonal1, out coords) ||
                TrySequence(diagonal2, out coords))
            {
                return new Seq(team, coords);
            }

            return null;
        }
    }
}