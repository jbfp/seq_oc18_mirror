using System;

namespace Sequence.Core
{
    public sealed class PlayerId : IEquatable<PlayerId>
    {
        private readonly string _value;

        public PlayerId(string value) => this._value = value ?? throw new ArgumentNullException(nameof(value));

        public bool Equals(PlayerId other) => other != null && string.Equals(_value, other._value, StringComparison.Ordinal);

        public override bool Equals(object obj) => Equals(obj as PlayerId);

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value;
    }
}
