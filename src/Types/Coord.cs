using System;

namespace Sequence
{
    public struct Coord : IEquatable<Coord>
    {
        public Coord(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; }
        public int Row { get; }

        public bool Equals(Coord other) => Column.Equals(other.Column) && Row.Equals(other.Row);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj.GetType() != typeof(Coord))
            {
                return false;
            }

            return Equals((Coord)obj);
        }

        public override int GetHashCode() => HashCode.Combine(Column, Row);

        public override string ToString() => $"(Column, Row): ({Column}, {Row})";
    }
}
