using System;
using Sequence.Core.Boards;

namespace Sequence.Core
{
    public enum BoardType
    {
        OneEyedJack,
        Sequence,
    }

    public static class BoardTypeExtensions
    {
        public static Type ToType(this BoardType boardType)
        {
            switch (boardType)
            {
                case BoardType.OneEyedJack:
                    return typeof(OneEyedJackBoard);

                case BoardType.Sequence:
                    return typeof(SequenceBoard);

                default:
                    throw new ArgumentOutOfRangeException(nameof(boardType), boardType, null);
            }
        }

        public static IBoardType Create(this BoardType boardType)
        {
            return (IBoardType)Activator.CreateInstance(boardType.ToType());
        }
    }
}
