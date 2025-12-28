using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neba.Domain.Awards;

/// <summary>
/// JSON converter for <see cref="HallOfFameCategory"/> to support serialization and deserialization.
/// Serializes the category as its integer value and deserializes using FromValue.
/// </summary>
public sealed class HallOfFameCategoryJsonConverter : JsonConverter<HallOfFameCategory>
{
    /// <summary>
    /// Reads and converts the JSON to type <see cref="HallOfFameCategory"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted value.</returns>
    public override HallOfFameCategory? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Expected number token for HallOfFameCategory, got {reader.TokenType}");
        }

        int value = reader.GetInt32();

        // FromValue returns an IEnumerable of flags, we need the first one
        // For a single flag value, this will return one item
        var categories = HallOfFameCategory.FromValue(value).ToList();

        if (categories.Count == 0)
        {
            throw new JsonException($"Invalid HallOfFameCategory value: {value}");
        }

        // For single values, return the first category
        // For combined flags, this will return the first flag in the combination
        // Note: This assumes we're deserializing individual categories, not combinations
        return categories[0];
    }

    /// <summary>
    /// Writes a specified value as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, HallOfFameCategory value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteNumberValue(value.Value);
    }
}
