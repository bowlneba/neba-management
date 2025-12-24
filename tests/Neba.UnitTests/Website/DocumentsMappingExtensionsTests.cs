using Neba.Website.Endpoints.Documents;
using Neba.Tests.Documents;
using Neba.Application.Documents;
using Neba.Contracts;

namespace Neba.UnitTests.Website;

public sealed class DocumentsMappingExtensionsTests
{
    [Fact]
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

