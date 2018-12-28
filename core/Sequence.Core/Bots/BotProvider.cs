using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Sequence.Core
{
    public sealed class BotProvider
    {
        private static readonly IImmutableDictionary<string, Type> _botTypes;

        static BotProvider()
        {
            var assembly = typeof(IBot).Assembly;

            var botTypes = assembly
                .DefinedTypes
                .Where(type => type.ImplementedInterfaces.Contains(typeof(IBot)));

            _botTypes = botTypes
                .Select(type => (Type: type.AsType(), FriendlyName: type
                    .GetCustomAttributes()
                    .OfType<BotAttribute>()
                    .Select(attribute => attribute.Name) // Use the BotAttribute.Name...
                    .DefaultIfEmpty(type.Name) // ... or the type name.
                    .Single()))
                .ToImmutableDictionary(t => t.FriendlyName, t => t.Type);
        }

        public static IImmutableSet<string> BotTypes => _botTypes.Keys.ToImmutableSortedSet();

        public static IBot Create(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_botTypes.TryGetValue(name, out var type))
            {
                return (IBot)Activator.CreateInstance(type, nonPublic: true);
            }

            return null;
        }
    }
}
