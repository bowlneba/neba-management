using System;

namespace Neba.Web.Server.HallOfFame;

/// <summary>
/// Temporary test data URIs used by the Hall of Fame page. These are
/// intentionally kept in a separate C# file so analyzer suppressions
/// can be applied locally and reverted when real data is available.
/// </summary>
public static class TestDataUris
{
    /// <summary>
    /// A temporary photo URI used for the John Anderson fake inductee.
    /// </summary>
#pragma warning disable S1075 // Strings should not be hardcoded
    public static readonly Uri JohnAndersonPhoto = new(
        "https://media.gettyimages.com/id/155395933/photo/retro-bowler.jpg?s=2048x2048&w=gi&k=20&c=bgEkWewpbShIBnBWnco_69NAKSoZ3bVPC43UBfs-eeU=");
#pragma warning restore S1075
}
