using System.Text.Json;
using System.Text.Json.Serialization;

namespace BAL.DTOs.Common
{
    /// <summary>
    /// Optional field wrapper to distinguish:
    /// - property omitted (HasValue = false)
    /// - property provided as null (HasValue = true, Value = default)
    /// - property provided with value (HasValue = true, Value = value)
    /// </summary>
    [JsonConverter(typeof(OptionalFieldJsonConverterFactory))]
    public readonly struct OptionalField<T>
    {
        public bool HasValue { get; }
        public T? Value { get; }

        public OptionalField(T? value)
        {
            HasValue = true;
            Value = value;
        }
    }

    internal sealed class OptionalFieldJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(OptionalField<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var innerType = typeToConvert.GetGenericArguments()[0];
            var converterType = typeof(OptionalFieldJsonConverter<>).MakeGenericType(innerType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private sealed class OptionalFieldJsonConverter<TInner> : JsonConverter<OptionalField<TInner>>
        {
            public override OptionalField<TInner> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // If the property exists in JSON, this converter is invoked.
                // Handle explicit null.
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return new OptionalField<TInner>(default);
                }

                var value = JsonSerializer.Deserialize<TInner>(ref reader, options);
                return new OptionalField<TInner>(value);
            }

            public override void Write(Utf8JsonWriter writer, OptionalField<TInner> value, JsonSerializerOptions options)
            {
                if (!value.HasValue)
                {
                    writer.WriteNullValue();
                    return;
                }

                JsonSerializer.Serialize(writer, value.Value, options);
            }
        }
    }
}

