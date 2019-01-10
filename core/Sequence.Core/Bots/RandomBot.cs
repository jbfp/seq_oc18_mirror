using Sequence.Core.Boards;
using System;
using System.Collections.Generic;

namespace Sequence.Core.Bots
{
    [Bot(Name)]
    public sealed class RandomBot : IBot
    {
        public const string Name = "Random Bot";

        private readonly Random _rng;

        public RandomBot() => _rng = new Random();
        public RandomBot(int seed) => _rng = new Random(seed);

        public (Card, Coord) Decide(GameView game)
        {
            var board = game.Board;
            var hand = game.Hand;

            Card randomCard;

            do
            {
                // TODO: Support "random" one-eyed jack.
                randomCard = hand[_rng.Next(hand.Count)];
            } while (randomCard.IsOneEyedJack());

            var possibleCoords = new List<Coord>(2);

            for (int rowIdx = 0; rowIdx < board.Length; rowIdx++)
            {
                var row = board[rowIdx];

                for (int colIdx = 0; colIdx < row.Length; colIdx++)
                {
                    var coord = new Coord(colIdx, rowIdx);

                    if (board.Matches(coord, randomCard))
                    {
                        possibleCoords.Add(coord);
                    }
                }
            }

            var randomCoord = possibleCoords[_rng.Next(possibleCoords.Count)];

            return (randomCard, randomCoord);
        }
    }
}
