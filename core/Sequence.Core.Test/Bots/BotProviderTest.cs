using System;
using Xunit;

namespace Sequence.Core.Bots.Test
{
    public sealed class BotProviderTest
    {
        [Fact]
        public void BotTypesContainsNullBot()
        {
            Assert.Contains(NullBot.Name, BotProvider.BotTypes);
        }

        [Fact]
        public void ThrowsOnNullBotName()
        {
            Assert.Throws<ArgumentNullException>(() => BotProvider.Create(name: null));
        }

        [Theory]
        [InlineData(NullBot.Name)]
        public void CanCreateBot(string botName)
        {
            var bot = BotProvider.Create(botName);
            Assert.NotNull(bot);
            Assert.IsType<NullBot>(bot);
        }

        [Fact]
        public void CannotCreateUnknownBot()
        {
            Assert.Null(BotProvider.Create("ogiejgoer"));
        }
    }
}
