using System;

namespace Sequence
{
    public sealed class GameId : IEquatable<GameId>
    {
        private readonly Guid _value;

        public GameId(Guid value) => _value = value;

        public bool Equals(GameId other) => other != null && _value.Equals(other._value);

        public override bool Equals(object obj) => Equals(obj as GameId);

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value.ToString();
    }
}
