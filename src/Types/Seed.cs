using System;

namespace Sequence
{
    public struct Seed : IEquatable<Seed>
    {
        private readonly int _value;

        public Seed(int value)
        {
            _value = value;
        }

        public Random ToRandom() => new Random(_value);

        public int ToInt32() => _value;

        public bool Equals(Seed other) => _value.Equals(other._value);

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (obj.GetType() != typeof(Seed))
            {
                return false;
            }

            return Equals((Seed)obj);
        }

        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(Seed left, Seed right) => left.Equals(right);
        public static bool operator !=(Seed left, Seed right) => !(left == right);
    }
}
