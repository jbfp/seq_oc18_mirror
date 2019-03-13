using System;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.PlayCard
{
    public static class Moves
    {
        public static IImmutableList<Move> GenerateMoves(
            GameState state,
            PlayerId playerId)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            var playerIdx = state.PlayerIdByIdx.IndexOf(playerId);

            if (playerIdx == -1)
            {
                throw new ArgumentException("Playerw is not in game.");
            }

            var moves = ImmutableList.CreateBuilder<Move>();
            var boardType = state.BoardType;
            var chips = state.Chips;
            var hand = state.PlayerHandByIdx[playerIdx];
            var team = state.PlayerTeamByIdx[playerIdx];

            var occupiedCoords = chips.Keys.ToImmutableHashSet();
            var coordsInSequence = state.Sequences
                .SelectMany(seq => seq.Coords)
                .ToImmutableHashSet();

            foreach (var card in hand)
            {
                if (card.IsOneEyedJack())
                {
                    foreach (var chip in chips)
                    {
                        var coord = chip.Key;
                        var isNotOwnTeam = chip.Value != team;
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
                else if (state.DeadCards.Contains(card))
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
