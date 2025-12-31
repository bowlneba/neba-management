namespace Neba.Web.Server.Maps;

/// <summary>
/// Configuration settings for Azure Maps integration.
/// </summary>
public sealed class AzureMapsSettings
{
    /// <summary>
    /// The Azure Maps Account ID (Client ID) used for Azure AD authentication.
    /// This is provided by Azure when using managed identity and is injected
    /// via app settings in production deployments.
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// The subscription key for Azure Maps, used for local development.
    /// This should be stored in user secrets locally and NOT committed to source control.
    /// In production, managed identity with RBAC is used instead of subscription keys.
    /// </summary>
    public string? SubscriptionKey { get; set; }
}
