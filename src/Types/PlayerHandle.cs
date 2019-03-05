using System;

namespace Sequence
{
    public sealed class PlayerHandle : IEquatable<PlayerHandle>
    {
        private readonly string _value;

        public PlayerHandle(string value) => this._value = value ?? throw new ArgumentNullException(nameof(value));

        public bool Equals(PlayerHandle other) => other != null && string.Equals(_value, other._value, StringComparison.Ordinal);

        public override bool Equals(object obj) => Equals(obj as PlayerHandle);

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value;
    }
}
