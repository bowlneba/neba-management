using ErrorOr;
using Neba.Infrastructure.Caching;
using Shouldly;
using Xunit;

namespace Neba.UnitTests.Infrastructure.Caching;

[Trait("Category", "Unit")]
[Trait("Component", "Caching")]
public sealed class ErrorOrCacheHelperTests
{
    private sealed record TestDto(string Value);

    [Fact(DisplayName = "IsErrorOrType should detect ErrorOr<T> correctly")]
    public void IsErrorOrType_ShouldDetectErrorOrCorrectly()
    {
        // Arrange
        Type errorOrType = typeof(ErrorOr<string>);
        Type plainType = typeof(string);
        Type errorOrDtoType = typeof(ErrorOr<TestDto>);
        Type collectionType = typeof(IReadOnlyCollection<string>);

        // Act & Assert
        ErrorOrCacheHelper.IsErrorOrType(errorOrType).ShouldBeTrue();
        ErrorOrCacheHelper.IsErrorOrType(plainType).ShouldBeFalse();
        ErrorOrCacheHelper.IsErrorOrType(errorOrDtoType).ShouldBeTrue();
        ErrorOrCacheHelper.IsErrorOrType(collectionType).ShouldBeFalse();
    }

    [Fact(DisplayName = "GetInnerType should extract T from ErrorOr<T>")]
    public void GetInnerType_ShouldExtractInnerType()
    {
        // Arrange
        Type errorOrString = typeof(ErrorOr<string>);
        Type errorOrDto = typeof(ErrorOr<TestDto>);

        // Act
        Type innerType1 = ErrorOrCacheHelper.GetInnerType(errorOrString);
        Type innerType2 = ErrorOrCacheHelper.GetInnerType(errorOrDto);

        // Assert
        innerType1.ShouldBe(typeof(string));
        innerType2.ShouldBe(typeof(TestDto));
    }

    [Fact(DisplayName = "GetInnerType should throw when type is not ErrorOr<T>")]
    public void GetInnerType_ShouldThrow_WhenTypeIsNotErrorOr()
    {
        // Arrange
        Type plainType = typeof(string);

        // Act & Assert
        Should.Throw<ArgumentException>(() => ErrorOrCacheHelper.GetInnerType(plainType));
    }

    [Fact(DisplayName = "IsError should detect error state correctly")]
    public void IsError_ShouldDetectErrorState()
    {
        // Arrange
        ErrorOr<string> successResult = "test value";
        ErrorOr<string> errorResult = Error.NotFound("Test.NotFound", "Not found");

        // Act
        bool isSuccessError = ErrorOrCacheHelper.IsError(successResult);
        bool isErrorError = ErrorOrCacheHelper.IsError(errorResult);

        // Assert
        isSuccessError.ShouldBeFalse();
        isErrorError.ShouldBeTrue();
    }

    [Fact(DisplayName = "GetValue should extract value from successful ErrorOr")]
    public void GetValue_ShouldExtractValue()
    {
        // Arrange
        var testDto = new TestDto("test");
        ErrorOr<TestDto> successResult = testDto;

        // Act
        object? value = ErrorOrCacheHelper.GetValue(successResult);

        // Assert
        value.ShouldNotBeNull();
        value.ShouldBeOfType<TestDto>();
        ((TestDto)value).Value.ShouldBe("test");
    }

    [Fact(DisplayName = "WrapValue should wrap T into ErrorOr<T>")]
    public void WrapValue_ShouldWrapValue()
    {
        // Arrange
        var testDto = new TestDto("test");
        Type innerType = typeof(TestDto);

        // Act
        object wrapped = ErrorOrCacheHelper.WrapValue(innerType, testDto);

        // Assert
        wrapped.ShouldBeOfType<ErrorOr<TestDto>>();

        var errorOr = (ErrorOr<TestDto>)wrapped;
        errorOr.IsError.ShouldBeFalse();
        errorOr.Value.ShouldBe(testDto);
    }

    [Fact(DisplayName = "WrapValue should maintain value equality")]
    public void WrapValue_ShouldMaintainValueEquality()
    {
        // Arrange
        string originalValue = "test string";
        Type innerType = typeof(string);

        // Act
        object wrapped = ErrorOrCacheHelper.WrapValue(innerType, originalValue);
        object? unwrapped = ErrorOrCacheHelper.GetValue(wrapped);

        // Assert
        unwrapped.ShouldBe(originalValue);
    }

    [Fact(DisplayName = "IsErrorOrType should throw when type is null")]
    public void IsErrorOrType_ShouldThrow_WhenTypeIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ErrorOrCacheHelper.IsErrorOrType(null!));
    }

    [Fact(DisplayName = "GetInnerType should throw when type is null")]
    public void GetInnerType_ShouldThrow_WhenTypeIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ErrorOrCacheHelper.GetInnerType(null!));
    }

    [Fact(DisplayName = "IsError should throw when instance is null")]
    public void IsError_ShouldThrow_WhenInstanceIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ErrorOrCacheHelper.IsError(null!));
    }

    [Fact(DisplayName = "GetValue should throw when instance is null")]
    public void GetValue_ShouldThrow_WhenInstanceIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ErrorOrCacheHelper.GetValue(null!));
    }

    [Fact(DisplayName = "WrapValue should throw when innerType is null")]
    public void WrapValue_ShouldThrow_WhenInnerTypeIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ErrorOrCacheHelper.WrapValue(null!, "value"));
    }

    [Fact(DisplayName = "WrapValue should throw when value is null")]
    public void WrapValue_ShouldThrow_WhenValueIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ErrorOrCacheHelper.WrapValue(typeof(string), null!));
    }
}
