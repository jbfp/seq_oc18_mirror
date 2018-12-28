using System;

namespace Sequence.Core
{
    public sealed class Player : IEquatable<Player>
    {
        public Player(PlayerId id, PlayerHandle handle)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));
        }

        public PlayerId Id { get; }
        public PlayerHandle Handle { get; }

        public bool Equals(Player other) => Id.Equals(other?.Id);
        public override bool Equals(object obj) => Equals(obj as Player);
        public override int GetHashCode() => Id.GetHashCode();
        public override string ToString() => Handle.ToString();
    }
}
