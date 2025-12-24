using Neba.Application.Documents;
using Neba.Contracts;

namespace Neba.Website.Endpoints.Documents;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class DocumentsMappingExtensions
{
    extension(DocumentDto dto)
    {
        public DocumentResponse<string> ToStringResponse()
        {
            return new DocumentResponse<string>
            {
                Content = dto.Content,
                Metadata = dto.Metadata
            };
        }
    }
}
