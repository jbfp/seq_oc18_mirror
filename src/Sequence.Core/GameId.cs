using System;

namespace Sequence.Core
{
    public sealed class GameId : IEquatable<GameId>
    {
        private readonly object _value;

        public GameId(object value) => _value = value;

        public bool Equals(GameId other) => _value.Equals(other?._value);

        public override bool Equals(object obj) => Equals(obj as GameId);

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value.ToString();
    }
}
