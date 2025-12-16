
using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Documents.Bylaws;

/// <summary>
/// Query to retrieve the organization bylaws as an HTML string.  
/// </summary>
public sealed record GetBylawsQuery
    : IQuery<string>;
