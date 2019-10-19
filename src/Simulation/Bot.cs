namespace Sequence.Simulation
{
    public sealed class Bot
    {
        public Bot(string type)
        {
            BotType = type;
        }

        public string BotType { get; }
    }
}
