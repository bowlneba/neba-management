using Ardalis.SmartEnum;

namespace Neba.Domain.Contact;

/// <summary>
/// Represents the set of United States states (including the District of Columbia)
/// as an Ardalis.SmartEnum where the enum's value is the state's postal abbreviation (e.g. "CA").
/// </summary>
public sealed class UsState
    : SmartEnum<UsState, string>
{
    /// <summary>Alabama (AL).</summary>
    public static readonly UsState Alabama = new("Alabama", "AL");

    /// <summary>Alaska (AK).</summary>
    public static readonly UsState Alaska = new("Alaska", "AK");

    /// <summary>Arizona (AZ).</summary>
    public static readonly UsState Arizona = new("Arizona", "AZ");

    /// <summary>Arkansas (AR).</summary>
    public static readonly UsState Arkansas = new("Arkansas", "AR");

    /// <summary>California (CA).</summary>
    public static readonly UsState California = new("California", "CA");

    /// <summary>Colorado (CO).</summary>
    public static readonly UsState Colorado = new("Colorado", "CO");

    /// <summary>Connecticut (CT).</summary>
    public static readonly UsState Connecticut = new("Connecticut", "CT");

    /// <summary>Delaware (DE).</summary>
    public static readonly UsState Delaware = new("Delaware", "DE");

    /// <summary>Florida (FL).</summary>
    public static readonly UsState Florida = new("Florida", "FL");

    /// <summary>Georgia (GA).</summary>
    public static readonly UsState Georgia = new("Georgia", "GA");

    /// <summary>Hawaii (HI).</summary>
    public static readonly UsState Hawaii = new("Hawaii", "HI");

    /// <summary>Idaho (ID).</summary>
    public static readonly UsState Idaho = new("Idaho", "ID");

    /// <summary>Illinois (IL).</summary>
    public static readonly UsState Illinois = new("Illinois", "IL");

    /// <summary>Indiana (IN).</summary>
    public static readonly UsState Indiana = new("Indiana", "IN");

    /// <summary>Iowa (IA).</summary>
    public static readonly UsState Iowa = new("Iowa", "IA");

    /// <summary>Kansas (KS).</summary>
    public static readonly UsState Kansas = new("Kansas", "KS");

    /// <summary>Kentucky (KY).</summary>
    public static readonly UsState Kentucky = new("Kentucky", "KY");

    /// <summary>Louisiana (LA).</summary>
    public static readonly UsState Louisiana = new("Louisiana", "LA");

    /// <summary>Maine (ME).</summary>
    public static readonly UsState Maine = new("Maine", "ME");

    /// <summary>Maryland (MD).</summary>
    public static readonly UsState Maryland = new("Maryland", "MD");

    /// <summary>Massachusetts (MA).</summary>
    public static readonly UsState Massachusetts = new("Massachusetts", "MA");

    /// <summary>Michigan (MI).</summary>
    public static readonly UsState Michigan = new("Michigan", "MI");

    /// <summary>Minnesota (MN).</summary>
    public static readonly UsState Minnesota = new("Minnesota", "MN");

    /// <summary>Mississippi (MS).</summary>
    public static readonly UsState Mississippi = new("Mississippi", "MS");

    /// <summary>Missouri (MO).</summary>
    public static readonly UsState Missouri = new("Missouri", "MO");

    /// <summary>Montana (MT).</summary>
    public static readonly UsState Montana = new("Montana", "MT");

    /// <summary>Nebraska (NE).</summary>
    public static readonly UsState Nebraska = new("Nebraska", "NE");

    /// <summary>Nevada (NV).</summary>
    public static readonly UsState Nevada = new("Nevada", "NV");

    /// <summary>New Hampshire (NH).</summary>
    public static readonly UsState NewHampshire = new("New Hampshire", "NH");

    /// <summary>New Jersey (NJ).</summary>
    public static readonly UsState NewJersey = new("New Jersey", "NJ");

    /// <summary>New Mexico (NM).</summary>
    public static readonly UsState NewMexico = new("New Mexico", "NM");

    /// <summary>New York (NY).</summary>
    public static readonly UsState NewYork = new("New York", "NY");

    /// <summary>North Carolina (NC).</summary>
    public static readonly UsState NorthCarolina = new("North Carolina", "NC");

    /// <summary>North Dakota (ND).</summary>
    public static readonly UsState NorthDakota = new("North Dakota", "ND");

    /// <summary>Ohio (OH).</summary>
    public static readonly UsState Ohio = new("Ohio", "OH");

    /// <summary>Oklahoma (OK).</summary>
    public static readonly UsState Oklahoma = new("Oklahoma", "OK");

    /// <summary>Oregon (OR).</summary>
    public static readonly UsState Oregon = new("Oregon", "OR");

    /// <summary>Pennsylvania (PA).</summary>
    public static readonly UsState Pennsylvania = new("Pennsylvania", "PA");

    /// <summary>Rhode Island (RI).</summary>
    public static readonly UsState RhodeIsland = new("Rhode Island", "RI");

    /// <summary>South Carolina (SC).</summary>
    public static readonly UsState SouthCarolina = new("South Carolina", "SC");

    /// <summary>South Dakota (SD).</summary>
    public static readonly UsState SouthDakota = new("South Dakota", "SD");

    /// <summary>Tennessee (TN).</summary>
    public static readonly UsState Tennessee = new("Tennessee", "TN");

    /// <summary>Texas (TX).</summary>
    public static readonly UsState Texas = new("Texas", "TX");

    /// <summary>Utah (UT).</summary>
    public static readonly UsState Utah = new("Utah", "UT");

    /// <summary>Vermont (VT).</summary>
    public static readonly UsState Vermont = new("Vermont", "VT");

    /// <summary>Virginia (VA).</summary>
    public static readonly UsState Virginia = new("Virginia", "VA");

    /// <summary>Washington (WA).</summary>
    public static readonly UsState Washington = new("Washington", "WA");

    /// <summary>West Virginia (WV).</summary>
    public static readonly UsState WestVirginia = new("West Virginia", "WV");

    /// <summary>Wisconsin (WI).</summary>
    public static readonly UsState Wisconsin = new("Wisconsin", "WI");

    /// <summary>Wyoming (WY).</summary>
    public static readonly UsState Wyoming = new("Wyoming", "WY");

    /// <summary>District of Columbia (DC).</summary>
    public static readonly UsState DistrictOfColumbia = new("District of Columbia", "DC");

    /// <summary>
    /// Private constructor used to create each <see cref="UsState"/> instance.
    /// The <paramref name="name"/> is the full state name and <paramref name="value"/> is the postal abbreviation.
    /// </summary>
    private UsState(string name, string value)
        : base(name, value)
    { }
}
