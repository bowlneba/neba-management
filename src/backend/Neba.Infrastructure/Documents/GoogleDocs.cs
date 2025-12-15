using System.Text.Json.Serialization;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// Configuration settings for Google Docs integration.
/// </summary>
internal sealed record GoogleDocsSettings
{
    /// <summary>
    /// Gets the Google service account credentials.
    /// </summary>
    public required GoogleDocsCredentials Credentials { get; init; }

    /// <summary>
    /// Gets the collection of configured Google Documents.
    /// </summary>
    public required IReadOnlyCollection<GoogleDocument> Documents { get; init; }
}

/// <summary>
/// Google Cloud service account credentials for accessing Google Docs API.
/// </summary>
internal sealed record GoogleDocsCredentials
{
    /// <summary>
    /// Gets the type of credential (typically "service_account").
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Gets the Google Cloud project ID.
    /// </summary>
    [JsonPropertyName("project_id")]
    public required string ProjectId { get; init; }

    /// <summary>
    /// Gets the private key ID for the service account.
    /// </summary>
    [JsonPropertyName("private_key_id")]
    public string? PrivateKeyId { get; init; }

    /// <summary>
    /// Gets or sets the private key for the service account in PEM format.
    /// </summary>
    [JsonPropertyName("private_key")]
    public string? PrivateKey { get; set; }

    /// <summary>
    /// Gets the service account email address.
    /// </summary>
    [JsonPropertyName("client_email")]
    public string? ClientEmail { get; init; }

    /// <summary>
    /// Gets the client ID for the service account.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string? ClientId { get; init; }

    /// <summary>
    /// Gets the OAuth2 authorization server URI.
    /// </summary>
    [JsonPropertyName("auth_uri")]
    public required string AuthUri { get; init; }

    /// <summary>
    /// Gets the OAuth2 token server URI.
    /// </summary>
    [JsonPropertyName("token_uri")]
    public required string TokenUri { get; init; }

    /// <summary>
    /// Gets the URL for the OAuth2 provider's X.509 certificate.
    /// </summary>
    [JsonPropertyName("auth_provider_x509_cert_url")]
    public required string AuthProviderX509CertUrl { get; init; }

    /// <summary>
    /// Gets the URL for the client's X.509 certificate.
    /// </summary>
    [JsonPropertyName("client_x509_cert_url")]
    public string? ClientX509CertUrl { get; init; }

    /// <summary>
    /// Gets the universe domain for the Google Cloud project.
    /// </summary>
    [JsonPropertyName("universe_domain")]
    public required string UniverseDomain { get; init; }
}

/// <summary>
/// Represents a configured Google Document with its mapping information.
/// </summary>
internal sealed record GoogleDocument
{
    /// <summary>
    /// Gets the logical name used to reference this document (e.g., "tournament-rules").
    /// This name is used by the application layer to request documents.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the Google Docs document ID (from the Google Docs URL).
    /// </summary>
    public required string DocumentId { get; init; }

    /// <summary>
    /// Gets the application route/path associated with this document.
    /// </summary>
    public required string Route { get; init; }
}
