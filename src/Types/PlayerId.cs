using System;
using System.Globalization;

namespace Sequence
{
    public sealed class PlayerId : IEquatable<PlayerId>
    {
        private readonly int _value;

        public PlayerId(int value) => _value = value;

        public bool Equals(PlayerId? other) => other != null && _value.Equals(other._value);

        public override bool Equals(object obj) => Equals(obj as PlayerId);

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);

        public int ToInt32() => _value;
    }
}
