using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum;

namespace Neba.Domain;

/// <summary>
/// Generic JSON converter for <see cref="SmartFlagEnum{TEnum}"/> types.
/// Serializes the enum as its integer value and deserializes using FromValue.
/// Handles both legacy object format and new number format for backward compatibility.
/// </summary>
/// <typeparam name="TEnum">The SmartFlagEnum type to convert.</typeparam>
public class SmartFlagEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : SmartFlagEnum<TEnum>
{
    /// <summary>
    /// Reads and converts the JSON to type <typeparamref name="TEnum"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted value.</returns>
    public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Handle legacy cached format: read the object and extract the Value property
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            if (doc.RootElement.TryGetProperty("Value", out JsonElement valueElement))
            {
                int value = valueElement.GetInt32();
                var items = SmartFlagEnum<TEnum>.FromValue(value).ToList();
                return items.Count > 0 ? items[0] : default;
            }

            throw new JsonException($"Expected 'Value' property in {typeof(TEnum).Name} object");
        }

        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Expected number or object token for {typeof(TEnum).Name}, got {reader.TokenType}");
        }

        int enumValue = reader.GetInt32();

        // FromValue returns an IEnumerable of flags, we need the first one
        // For a single flag value, this will return one item
        var enumItems = SmartFlagEnum<TEnum>.FromValue(enumValue).ToList();

        if (enumItems.Count == 0)
        {
            throw new JsonException($"Invalid {typeof(TEnum).Name} value: {enumValue}");
        }

        // For single values, return the first category
        // For combined flags, this will return the first flag in the combination
        // Note: This assumes we're deserializing individual categories, not combinations
        return enumItems[0];
    }

    /// <summary>
    /// Writes a specified value as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteNumberValue(value.Value);
    }
}
