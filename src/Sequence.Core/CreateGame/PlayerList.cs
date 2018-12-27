using System.Collections.Immutable;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Sequence.Core.CreateGame
{
    public sealed class PlayerList : IEnumerable<PlayerId>
    {
        public PlayerList(params PlayerId[] players)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
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

        public PlayerId FirstPlayer => Players.First();
        public IImmutableList<PlayerId> Players { get; }

        public IEnumerator<PlayerId> GetEnumerator() => Players.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
