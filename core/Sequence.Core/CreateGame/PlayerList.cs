using System.Collections.Immutable;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Sequence.Core.CreateGame
{
    public sealed class PlayerList : IEnumerable<PlayerHandle>
    {
        private static readonly ImmutableArray<int> _allowedGameSizes = ImmutableArray.Create(
            2, 3, 4, 6
        );

        public PlayerList(params PlayerHandle[] players)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            if (!_allowedGameSizes.Contains(players.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(players), players.Length, "Game size is not valid.");
            }

            var duplicatePlayers = players
                .GroupBy(playerId => playerId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            if (duplicatePlayers.Any())
            {
                throw new ArgumentException("Duplicate players are not allowed.");
            }

            Players = players.ToImmutableList();
        }

        public PlayerHandle FirstPlayer => Players.First();
        public IImmutableList<PlayerHandle> Players { get; }

        public IEnumerator<PlayerHandle> GetEnumerator() => Players.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
