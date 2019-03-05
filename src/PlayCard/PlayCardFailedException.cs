using System;

namespace Sequence.PlayCard
{
    public sealed class PlayCardFailedException : Exception
    {
        public PlayCardFailedException(PlayCardError error) : base(error.ToString())
        {
            Error = error;
        }

        public PlayCardError Error { get; }
    }
}
