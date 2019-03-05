using System;

namespace Sequence
{
    public sealed class Player : IEquatable<Player>
    {
        public Player(PlayerId id, PlayerHandle handle, PlayerType type = PlayerType.User)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
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

        public PlayerId Id { get; }
        public PlayerHandle Handle { get; }
        public PlayerType Type { get; }

        public bool Equals(Player other) => Id.Equals(other?.Id);
        public override bool Equals(object obj) => Equals(obj as Player);
        public override int GetHashCode() => Id.GetHashCode();
        public override string ToString() => Handle.ToString();
    }
}
