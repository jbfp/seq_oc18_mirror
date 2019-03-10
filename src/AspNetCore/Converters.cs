using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sequence.GetGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sequence.AspNetCore
{
    internal sealed class GameIdJsonConverter : JsonConverter
    {
        public override bool CanRead { get; } = true;
        public override bool CanWrite { get; } = true;

        public override bool CanConvert(Type objectType) => typeof(GameId) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string) && reader.Value is string str)
            {
                return new GameId(Guid.Parse(str));
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    internal sealed class PlayerHandleJsonConverter : JsonConverter
    {
        public override bool CanRead { get; } = true;
        public override bool CanWrite { get; } = true;

        public override bool CanConvert(Type objectType) => typeof(PlayerHandle) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string) && reader.Value is string str)
            {
                return new PlayerHandle(str);
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    internal sealed class PlayerIdJsonConverter : JsonConverter
    {
        public override bool CanRead { get; } = true;
        public override bool CanWrite { get; } = true;

        public override bool CanConvert(Type objectType) => typeof(PlayerId) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(long) && reader.Value is long x)
            {
                return new PlayerId((int)x);
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((PlayerId)value).ToInt32());
        }
    }

    internal sealed class TileJsonConverter : JsonConverter
    {
        public override bool CanRead { get; } = false;
        public override bool CanWrite { get; } = true;

        public override bool CanConvert(Type objectType) => typeof((Suit, Rank)?) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else if (value is ValueTuple<Suit, Rank> tile)
            {
                writer.WriteStartArray();
                serializer.Serialize(writer, tile.Item1);
                serializer.Serialize(writer, tile.Item2);
                writer.WriteEndArray();
            }
            else
            {
                throw new JsonSerializationException($"Cannot write value: '{value}'.");
            }
        }
    }

    internal sealed class GameEventConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(GameEventBase) != objectType
                && typeof(GameEventBase).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var type = value.GetType();
            var chars = type.Name
                .SelectMany(NameToChars)
                .ToArray();

            var name = new string(chars).Trim('-');

            var contractResolver = (DefaultContractResolver)serializer.ContractResolver;
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            var properties = type
                .GetProperties(bindingFlags)
                .Select(p => (p.Name, Value: p.GetValue(value)))
                .ToDictionary(
                    p => contractResolver.GetResolvedPropertyName(p.Name),
                    p => p.Value);

            writer.WriteStartObject();
            writer.WritePropertyName("kind");
            writer.WriteValue(name);

            foreach (var property in properties)
            {
                writer.WritePropertyName(property.Key);
                serializer.Serialize(writer, property.Value);
            }

            writer.WriteEnd();
        }

        private static IEnumerable<char> NameToChars(char c)
        {
            if (char.IsUpper(c))
            {
                yield return '-';
                yield return char.ToLowerInvariant(c);
            }
            else
            {
                yield return c;
            }
        }
    }
}
