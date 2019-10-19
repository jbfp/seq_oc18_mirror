using System;

namespace Sequence.PlayCard
{
    public sealed class PlayCardFailedException : Exception
    {
        public PlayCardFailedException(PlayCardError error) : base(error.ToString())
        {
            Error = error;
        }

        public PlayCardFailedException()
        {
        }

        public PlayCardFailedException(string message) : base(message)
        {
        }

        public PlayCardFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PlayCardError? Error { get; }
    }

    public sealed class ExchangeDeadCardFailedException : Exception
    {
        public ExchangeDeadCardFailedException(ExchangeDeadCardError error) : this(error.ToString())
        {
            Error = error;
        }

        public ExchangeDeadCardFailedException()
        {
        }

        public ExchangeDeadCardFailedException(string? message) : this(message, null)
        {
        }

        public ExchangeDeadCardFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public ExchangeDeadCardError? Error { get; }
    }
}
