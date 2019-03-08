using System;
using System.Collections.Generic;
using System.Linq;

namespace Sequence.Test.PlayCard
{
    public sealed class GameEventEqualityComparer : EqualityComparer<GameEvent>
    {
        public override bool Equals(GameEvent x, GameEvent y)
        {
            return x.ByPlayerId.Equals(y.ByPlayerId)
                && x.CardDrawn.Equals(y.CardDrawn)
                && x.CardUsed.Equals(y.CardUsed)
                && x.Chip.Equals(y.Chip)
                && x.Coord.Equals(y.Coord)
                && x.Index.Equals(y.Index)
                && (x.NextPlayerId?.Equals(y.NextPlayerId) ?? true)
                && x.Sequences.SequenceEqual(y.Sequences);
        }

        public override int GetHashCode(GameEvent obj)
        {
            throw new NotImplementedException();
        }
    }
}
