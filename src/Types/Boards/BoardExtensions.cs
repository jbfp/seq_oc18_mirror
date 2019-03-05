using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence
{
    using Board = ImmutableArray<ImmutableArray<Tile>>;

    public static class BoardExtensions
    {
        public static bool Matches(this Board board, Coord coord, Card card)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            var row = coord.Row;
            var column = coord.Column;

            if (row < 0 || row >= board.Length)
            {
                return false;
            }

            var theRow = board[row];

            if (column < 0 || column >= theRow.Length)
            {
                return false;
            }

            if (card.IsTwoEyedJack())
            {
                return true;
            }

            var match = theRow[column];

            if (match == null)
            {
                return false;
            }

            return match.Equals(card);
        }

        public static Seq GetSequence(
            this Board board,
            IImmutableDictionary<Coord, Team> chips,
            IImmutableSet<Coord> coordsInSequences,
            Coord coord,
            Team team)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (chips == null)
            {
                throw new ArgumentNullException(nameof(chips));
            }

            var row = coord.Row;
            var col = coord.Column;

            bool IsSequence(IEnumerable<Coord> cs)
            {
                int shared = 0;

                foreach (var c in cs)
                {
                    var isCorner =
                        (c.Row >= 0 && c.Row < board.Length) &&
                        (c.Column >= 0 && c.Column < board[c.Row].Length) &&
                        board[c.Row][c.Column] == null;

                    var teamOwnsCoord = chips.TryGetValue(c, out var t) && team == t;

                    if (isCorner || teamOwnsCoord)
                    {
                        var isPartOfAnotherSequence =
                            coordsInSequences.Contains(c) &&
                            chips.ContainsKey(c) && // 'chips' won't contain c is it's a corner.
                            chips[c] == team;

                        if (isPartOfAnotherSequence)
                        {
                            shared++;
                        }
                    }
                    else
                    {
                        // This is not a valid sequence. Exit early.
                        return false;
                    }
                }

                // This is a valid sequence if all coords of 'cs' are owned by 'team' and only 1
                // other coord is part of a sequence.
                return shared < 2;
            }

            bool TrySequence(IEnumerable<Coord> cs, out IImmutableList<Coord> seq)
            {
                int i = 0;

                while (true)
                {
                    seq = cs
                        .Skip(i++)
                        .Take(Seq.DefaultLength)
                        .ToImmutableList();

                    if (seq.Count < Seq.DefaultLength)
                    {
                        break;
                    }

                    if (IsSequence(seq))
                    {
                        return true;
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
