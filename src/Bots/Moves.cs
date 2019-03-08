using Sequence.GetGameView;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.Bots
{
    public static class Moves
    {
        public static IImmutableList<Move> FromGameView(GameView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            var moves = ImmutableList.CreateBuilder<Move>();
            var boardType = view.Rules.BoardType.Create();
            var chips = view.Chips;
            var hand = view.Hand;
            var team = view.Team;

            var occupiedCoords = chips
                .Select(chip => chip.Coord)
                .ToImmutableHashSet();

            var coordsInSequence = view.Chips
                .Where(chip => chip.IsLocked)
                .Select(chip => chip.Coord)
                .ToImmutableHashSet();

            foreach (var card in hand)
            {
                if (card.IsOneEyedJack())
                {
                    foreach (var chip in chips)
                    {
                        var coord = chip.Coord;
                        var isNotOwnTeam = chip.Team != team;
                        var isNotPartOfSequence = !coordsInSequence.Contains(coord);

                        if (isNotOwnTeam && isNotPartOfSequence)
                        {
                            moves.Add(new Move(card, coord));
                        }
                    }
                }
                else if (card.IsTwoEyedJack())
                {
                    foreach (var (row, y) in boardType.Board.Select((row, y) => (row, y)))
                    {
                        foreach (var (cell, x) in row.Select((cell, x) => (cell, x)))
                        {
                            var coord = new Coord(x, y);
                            var isNotCorner = cell != null;
                            var isFree = !occupiedCoords.Contains(coord);

                            if (isNotCorner && isFree)
                            {
                                moves.Add(new Move(card, coord));
                            }
                        }
                    }
                }
                else if (view.DeadCards.Contains(card))
                {
                    moves.Add(new Move(card, new Coord(-1, -1)));
                }
                else
                {
                    var tile = new Tile(card.Suit, card.Rank);

                    if (boardType.CoordsByTile.TryGetValue(tile, out var coords))
                    {
                        if (!occupiedCoords.Contains(coords.Item1))
                        {
                            moves.Add(new Move(card, coords.Item1));
                        }

                        if (!occupiedCoords.Contains(coords.Item2))
                        {
                            moves.Add(new Move(card, coords.Item2));
                        }
                    }
                }
            }

            return moves.ToImmutable();
        }
    }
}
