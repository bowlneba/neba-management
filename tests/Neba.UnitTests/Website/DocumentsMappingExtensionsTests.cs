using Neba.Application.Documents;
using Neba.Contracts;
using Neba.Tests.Documents;
using Neba.Website.Endpoints.Documents;

namespace Neba.UnitTests.Website;

public sealed class DocumentsMappingExtensionsTests
{
    [Fact(DisplayName = "Maps all properties from DocumentDto to response model")]
    public void ToStringResponseModel_ShouldMapAllProperties()
    {
        // Arrange
        DocumentDto dto = DocumentDtoFactory.Create();

        // Act
        DocumentResponse<string> response = dto.ToStringResponse();

        // Assert
        response.Content.ShouldBe(dto.Content);
        response.Metadata.ShouldBe(dto.Metadata);
    }
}

