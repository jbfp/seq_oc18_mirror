using System;

namespace Sequence.CreateGame
{
    public sealed class NewPlayer : IEquatable<NewPlayer>
    {
        public NewPlayer(PlayerHandle handle, PlayerType type)
        {
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));

            if (Enum.IsDefined(typeof(PlayerType), type))
            {
                Type = type;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public PlayerHandle Handle { get; }
        public PlayerType Type { get; }

        public bool Equals(NewPlayer other)
        {
            if (other == null)
            {
                return false;
            }

            return Handle.Equals(other.Handle) && Type.Equals(other.Type);
        }

        public override bool Equals(object obj) => Equals(obj as NewPlayer);

        public override int GetHashCode() => HashCode.Combine(Handle, Type);
    }
}
