using Neba.Domain;
using Neba.Domain.Contact;
using Neba.Domain.Identifiers;

namespace Neba.Website.Domain.BowlingCenters;

/// <summary>
/// Represents a bowling center entity containing identity, name and address information.
/// </summary>
public sealed class BowlingCenter
    : Aggregate<BowlingCenterId>
{
    /// <summary>
    /// Parameterless constructor used by serializers and ORM tools.
    /// </summary>
    internal BowlingCenter()
        : base(BowlingCenterId.Empty)
    { }

    /// <summary>
    /// Creates a new <see cref="BowlingCenter"/> with a generated identity.
    /// </summary>
    private BowlingCenter(BowlingCenterId id)
        : base(id)
    { }

    /// <summary>
    /// The display name of the bowling center.
    /// </summary>
    public string Name { get; internal init; } = string.Empty;

    /// <summary>
    /// The physical address of the bowling center.
    /// </summary>
    public Address Address { get; internal init; } = Address.Empty;


    /// <summary>
    /// The primary contact phone number for the bowling center.
    /// Stored as a domain `PhoneNumber` value object containing country code, digits and optional extension.
    /// </summary>
    public PhoneNumber PhoneNumber { get; internal init; } = PhoneNumber.Empty;

    /// <summary>
    /// Indicator if the bowling center has been closed
    /// </summary>
    public bool IsClosed { get; internal init; }

    /// <summary>
    /// Id for bowling center in legacy website system.
    /// </summary>
    public int? WebsiteId { get; internal init; }

    /// <summary>
    /// Id for bowling center in legacy application system.
    /// </summary>
    public int? ApplicationId { get; internal set; }
}
