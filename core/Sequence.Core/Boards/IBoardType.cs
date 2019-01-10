using System.Collections.Immutable;

namespace Sequence.Core.Boards
{
    internal interface IBoardType
    {
        ImmutableArray<ImmutableArray<Tile>> Board { get; }
    }
}
