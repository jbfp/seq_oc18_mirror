using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Sequence
{
    public sealed class BotProvider
    {
        static BotProvider()
        {
            var assembly = typeof(IBot).Assembly;

            var botTypes = assembly
                .DefinedTypes
                .Where(type => type.ImplementedInterfaces.Contains(typeof(IBot)));

            BotTypes = botTypes
                .Select(type => (Type: type.AsType(), FriendlyName: type
                    .GetCustomAttributes()
                    .OfType<BotAttribute>()
                    .Select(attribute => attribute.Name) // Use the BotAttribute.Name...
                    .DefaultIfEmpty(type.Name) // ... or the type name.
                    .Single()))
                .ToImmutableDictionary(t => t.FriendlyName, t => t.Type);
        }

        public static IImmutableDictionary<string, Type> BotTypes { get; }
    }
}
