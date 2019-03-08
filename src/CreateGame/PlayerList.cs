using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.CreateGame
{
    public sealed class PlayerList : IEnumerable<NewPlayer>
    {
        private static readonly ImmutableArray<int> _allowedGameSizes = ImmutableArray.Create(
            2, 3, 4, 6
        );

        public PlayerList(bool randomFirstPlayer, params NewPlayer[] players)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            RandomFirstPlayer = randomFirstPlayer;

            if (!_allowedGameSizes.Contains(players.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(players), players.Length, "Game size is not valid.");
            }

            var duplicatePlayers = players
                .Where(player => player.Type == PlayerType.User)
                .GroupBy(player => player.Handle)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            if (duplicatePlayers.Any())
            {
                throw new ArgumentException("Duplicate players are not allowed.");
            }

            Players = players.ToImmutableList();
        }

        public bool RandomFirstPlayer { get; }
        public IImmutableList<NewPlayer> Players { get; }

        public IEnumerator<NewPlayer> GetEnumerator() => Players.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
