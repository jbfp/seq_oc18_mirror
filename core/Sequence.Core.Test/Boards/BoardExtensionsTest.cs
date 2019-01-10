using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Sequence.Core.Boards.Test
{
    public sealed class BoardExtensionsTest
    {
        private static readonly Team _team = Team.Red;

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new object[] { (0, 0), (1, 0), (2, 0), (3, 0), (4, 0) },
            new object[] { (6, 0), (5, 1), (4, 2), (3, 3), (2, 4) },
            new object[] { (3, 3), (3, 4), (3, 5), (3, 6), (3, 7) },
            new object[] { (2, 2), (3, 3), (4, 4), (5, 5), (6, 6) },
            new object[] { (1, 1), (2, 2), (3, 3), (4, 4) },
            new object[] { (1, 0), (2, 0), (3, 0), (4, 0) },
            new object[] { (5, 0), (6, 0), (7, 0), (8, 0) },
            new object[] { (5, 5), (6, 6), (7, 7), (8, 8) },
            new object[] { (0, 8), (0, 7), (0, 6), (0, 5) },
        };

        [Theory]
        [MemberData(nameof(Data))]
        public void GetSequence_Test1(params (int, int)[] coords)
        {
            var boardSize = 10;
            var boardBuilder = ImmutableArray.CreateBuilder<ImmutableArray<Tile>>();

            for (int i = 0; i < boardSize; i++)
            {
                var row = ImmutableArray.CreateRange<Tile>(
                    Enumerable
                        .Range(0, 10)
                        .Select(_ => (Tile)null));

                boardBuilder.Add(row);
            }

            var board = boardBuilder.ToImmutable();
            var builder = ImmutableDictionary.CreateBuilder<Coord, Team>();

            foreach (var coord in coords)
            {
                builder.Add(new Coord(coord.Item1, coord.Item2), _team);
            }

            var chips = builder.ToImmutable();

            foreach (var coord in chips.Keys)
            {
                var seq = board.GetSequence(chips, coord, _team);
                Assert.NotNull(seq);
            }
        }
    }
}
