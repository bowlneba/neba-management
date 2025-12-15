using System.Text.Json.Serialization;

namespace Neba.Infrastructure.Documents;

internal sealed record GoogleDocsSettings
{
    public required GoogleDocsCredentials Credentials { get; init; }

    public required IReadOnlyCollection<GoogleDocument> Documents { get; init; }
}

internal sealed record GoogleDocsCredentials
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("project_id")]
    public required string ProjectId { get; init; }

    [JsonPropertyName("private_key_id")]
    public string? PrivateKeyId { get; init; }

    [JsonPropertyName("private_key")]
    public string? PrivateKey { get; init; }

    [JsonPropertyName("client_email")]
    public string? ClientEmail { get; init; }

    [JsonPropertyName("client_id")]
    public string? ClientId { get; init; }

    [JsonPropertyName("auth_uri")]
    public required string AuthUri { get; init; }

    [JsonPropertyName("token_uri")]
    public required string TokenUri { get; init; }

    [JsonPropertyName("auth_provider_x509_cert_url")]
    public required string AuthProviderX509CertUrl { get; init; }

    [JsonPropertyName("client_x509_cert_url")]
    public string? ClientX509CertUrl { get; init; }

    [JsonPropertyName("universe_domain")]
    public required string UniverseDomain { get; init; }
}

internal sealed record GoogleDocument
{
    public required string Name { get; init; }

    public required string DocumentId { get; init; }
}
