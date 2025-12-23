
using Neba.Application.Documents;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Documents.Bylaws;

/// <summary>
/// Query to retrieve the organization bylaws document.
/// </summary>
public sealed record GetBylawsQuery
    : IQuery<DocumentDto>;
