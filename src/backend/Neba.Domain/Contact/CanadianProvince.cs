using Ardalis.SmartEnum;

namespace Neba.Domain.Contact;

/// <summary>
/// Represents a Canadian province with its name and abbreviation.
/// </summary>
public sealed class CanadianProvince
    : SmartEnum<CanadianProvince, string>
{
    /// <summary>Alberta (AB).</summary>
    public static readonly CanadianProvince Alberta = new("Alberta", "AB");

    /// <summary>British Columbia (BC).</summary>
    public static readonly CanadianProvince BritishColumbia = new("British Columbia", "BC");

    /// <summary>Manitoba (MB).</summary>
    public static readonly CanadianProvince Manitoba = new("Manitoba", "MB");

    /// <summary>New Brunswick (NB).</summary>
    public static readonly CanadianProvince NewBrunswick = new("New Brunswick", "NB");

    /// <summary>Newfoundland and Labrador (NL).</summary>
    public static readonly CanadianProvince NewfoundlandAndLabrador = new("Newfoundland and Labrador", "NL");

    /// <summary>Nova Scotia (NS).</summary>
    public static readonly CanadianProvince NovaScotia = new("Nova Scotia", "NS");

    /// <summary>Ontario (ON).</summary>
    public static readonly CanadianProvince Ontario = new("Ontario", "ON");

    /// <summary>Prince Edward Island (PE).</summary>
    public static readonly CanadianProvince PrinceEdwardIsland = new("Prince Edward Island", "PE");

    /// <summary>Quebec (QC).</summary>
    public static readonly CanadianProvince Quebec = new("Quebec", "QC");

    /// <summary>Saskatchewan (SK).</summary>
    public static readonly CanadianProvince Saskatchewan = new("Saskatchewan", "SK");

    /// <summary>Northwest Territories (NT).</summary>
    public static readonly CanadianProvince NorthwestTerritories = new("Northwest Territories", "NT");

    /// <summary>Nunavut (NU).</summary>
    public static readonly CanadianProvince Nunavut = new("Nunavut", "NU");

    /// <summary>Yukon (YT).</summary>
    public static readonly CanadianProvince Yukon = new("Yukon", "YT");

    private CanadianProvince(string name, string value)
        : base(name, value)
    { }
}
