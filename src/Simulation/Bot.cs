using System;

namespace Sequence.Simulation
{
    public sealed class Bot
    {
        public Bot(string type)
        {
            BotType = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string BotType { get; }
    }
}
