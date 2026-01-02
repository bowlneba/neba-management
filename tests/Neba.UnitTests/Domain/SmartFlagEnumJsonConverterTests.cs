using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum;
using Neba.Domain;

namespace Neba.UnitTests.Domain;

[Trait("Category", "Unit")]
[Trait("Component", "Domain")]

public sealed class SmartFlagEnumJsonConverterTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact(DisplayName = "Should serialize SmartFlagEnum as integer value")]
    public void Serialize_ShouldWriteIntegerValue()
    {
        // Arrange
        TestFlagEnum testEnum = TestFlagEnum.Read;

        // Act
        string json = JsonSerializer.Serialize(testEnum, _jsonOptions);

        // Assert
        json.ShouldBe("1");
    }

    [Fact(DisplayName = "Should deserialize integer to SmartFlagEnum")]
    public void Deserialize_FromInteger_ShouldReturnCorrectEnum()
    {
        // Arrange
        const string json = "2"; // Write flag

        // Act
        TestFlagEnum? result = JsonSerializer.Deserialize<TestFlagEnum>(json, _jsonOptions);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(TestFlagEnum.Write);
        result.Value.ShouldBe(2);
        result.Name.ShouldBe("Write");
    }

    [Fact(DisplayName = "Should deserialize legacy object format")]
    public void Deserialize_FromLegacyObject_ShouldReturnCorrectEnum()
    {
        // Arrange
        const string json = """{"Name":"Execute","Value":4}""";

        // Act
        TestFlagEnum? result = JsonSerializer.Deserialize<TestFlagEnum>(json, _jsonOptions);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(TestFlagEnum.Execute);
        result.Value.ShouldBe(4);
        result.Name.ShouldBe("Execute");
    }

    [Fact(DisplayName = "Should serialize and deserialize collection of flags")]
    public void SerializeDeserialize_Collection_ShouldPreserveAllFlags()
    {
        // Arrange
        var flags = new List<TestFlagEnum> { TestFlagEnum.Read, TestFlagEnum.Write, TestFlagEnum.Execute };

        // Act
        string json = JsonSerializer.Serialize(flags, _jsonOptions);
        List<TestFlagEnum>? result = JsonSerializer.Deserialize<List<TestFlagEnum>>(json, _jsonOptions);

        // Assert
        json.ShouldBe("[1,2,4]");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldContain(TestFlagEnum.Read);
        result.ShouldContain(TestFlagEnum.Write);
        result.ShouldContain(TestFlagEnum.Execute);
    }

    [Fact(DisplayName = "Should handle None flag value")]
    public void SerializeDeserialize_None_ShouldWorkCorrectly()
    {
        // Arrange
        TestFlagEnum testEnum = TestFlagEnum.None;

        // Act
        string json = JsonSerializer.Serialize(testEnum, _jsonOptions);
        TestFlagEnum? result = JsonSerializer.Deserialize<TestFlagEnum>(json, _jsonOptions);

        // Assert
        json.ShouldBe("0");
        result.ShouldNotBeNull();
        result.ShouldBe(TestFlagEnum.None);
        result.Value.ShouldBe(0);
    }

    [Fact(DisplayName = "Should throw JsonException for invalid token type")]
    public void Deserialize_InvalidTokenType_ShouldThrowJsonException()
    {
        // Arrange
        const string json = """["array","of","strings"]""";

        // Act & Assert
        JsonException exception = Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<TestFlagEnum>(json, _jsonOptions));

        exception.Message.ShouldContain("Expected number or object token for TestFlagEnum");
    }

    [Fact(DisplayName = "Should throw JsonException for invalid enum value")]
    public void Deserialize_InvalidEnumValue_ShouldThrowSmartEnumException()
    {
        // Arrange
        const string json = "999"; // Invalid flag value

        // Act & Assert
        SmartEnumNotFoundException exception = Should.Throw<SmartEnumNotFoundException>(() =>
            JsonSerializer.Deserialize<TestFlagEnum>(json, _jsonOptions));

        exception.Message.ShouldContain("No TestFlagEnum with Value 999 found.");
    }

    [Fact(DisplayName = "Should throw JsonException for object without Value property")]
    public void Deserialize_ObjectWithoutValueProperty_ShouldThrowJsonException()
    {
        // Arrange
        const string json = """{"Name":"Read"}"""; // Missing Value property

        // Act & Assert
        JsonException exception = Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<TestFlagEnum>(json, _jsonOptions));

        exception.Message.ShouldContain("Expected 'Value' property in TestFlagEnum object");
    }

    [Fact(DisplayName = "Should deserialize collection from legacy object format")]
    public void Deserialize_CollectionFromLegacyFormat_ShouldReturnCorrectEnums()
    {
        // Arrange
        const string json = """[{"Name":"Read","Value":1},{"Name":"Write","Value":2}]""";

        // Act
        List<TestFlagEnum>? result = JsonSerializer.Deserialize<List<TestFlagEnum>>(json, _jsonOptions);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result[0].ShouldBe(TestFlagEnum.Read);
        result[1].ShouldBe(TestFlagEnum.Write);
    }

    [Fact(DisplayName = "Should handle round-trip serialization")]
    public void RoundTrip_ShouldPreserveEnumValue()
    {
        // Arrange
        TestFlagEnum original = TestFlagEnum.Execute;

        // Act
        string json = JsonSerializer.Serialize(original, _jsonOptions);
        TestFlagEnum? deserialized = JsonSerializer.Deserialize<TestFlagEnum>(json, _jsonOptions);
        string jsonAgain = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        deserialized.ShouldBe(original);
        jsonAgain.ShouldBe(json);
    }

    [Fact(DisplayName = "Should deserialize each flag in combined value separately")]
    public void Deserialize_CombinedFlags_ShouldDecompose()
    {
        // Arrange - Combined flags (Read | Write = 3) decomposed into collection [1, 2]
        const string json = "[1,2]";

        // Act
        List<TestFlagEnum>? result = JsonSerializer.Deserialize<List<TestFlagEnum>>(json, _jsonOptions);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldContain(TestFlagEnum.Read);
        result.ShouldContain(TestFlagEnum.Write);
    }
}

/// <summary>
/// Test SmartFlagEnum for unit testing the generic SmartFlagEnumJsonConverter.
/// </summary>
[JsonConverter(typeof(TestFlagEnumJsonConverter))]
internal sealed class TestFlagEnum : SmartFlagEnum<TestFlagEnum>
{
    public static readonly TestFlagEnum None = new(nameof(None), 0);
    public static readonly TestFlagEnum Read = new(nameof(Read), 1 << 0);
    public static readonly TestFlagEnum Write = new(nameof(Write), 1 << 1);
    public static readonly TestFlagEnum Execute = new(nameof(Execute), 1 << 2);

    private TestFlagEnum(string name, int value)
        : base(name, value)
    { }

    private TestFlagEnum()
        : base(string.Empty, 0)
    { }
}

#pragma warning disable CA1812 // This class is only instantiated by the JSON serializer via reflection

/// <summary>
/// JSON converter for TestFlagEnum.
/// </summary>
internal sealed class TestFlagEnumJsonConverter
    : SmartFlagEnumJsonConverter<TestFlagEnum>;
