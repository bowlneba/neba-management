<Query Kind="Program">
  <Connection>
    <ID>7ac3c7a2-d754-40b2-b9ae-e4870a3d53a8</ID>
    <NamingServiceVersion>3</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <SqlSecurity>true</SqlSecurity>
    <Server>localhost</Server>
    <UserName>neba</UserName>
    <Database>bowlneba</Database>
    <DisplayName>bowlneba (localhost)</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Npgsql.EntityFrameworkCore.PostgreSQL</EFProvider>
      <Port>19630</Port>
    </DriverData>
  </Connection>
  <NuGetReference>Microsoft.Data.SqlClient</NuGetReference>
  <NuGetReference>NameParserSharp</NuGetReference>
  <NuGetReference>Ardalis.SmartEnum</NuGetReference>
  <Namespace>Ardalis.SmartEnum</Namespace>
  <Namespace>Microsoft.Data.SqlClient</Namespace>
  <Namespace>NameParser</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	Titles.RemoveRange(Titles);
	Bowlers.RemoveRange(Bowlers);
	
	await SaveChangesAsync();
	
	var mergedBowlers = await MigrateBowlersAsync();
	
	var bowlerIdBySoftwareId = mergedBowlers.Where(b => b.softwareId.HasValue).ToDictionary(b => b.softwareId!.Value, b => b.bowlerId);
	var bowlerIdByWebsiteId = mergedBowlers.Where(b => b.websiteId.HasValue).ToDictionary(b => b.websiteId!.Value, b=> b.bowlerId);
	
	var softwareNamesBySoftwareId = mergedBowlers.Where(b => b.softwareId.HasValue).ToDictionary(b => b.softwareId!.Value, b=> b.softwareName);
	var websiteNamesByWebsiteId = mergedBowlers.Where(b => b.websiteId.HasValue).ToDictionary(b => b.websiteId!.Value, b=> b.websiteName!);
	
	await MigrateTitlesAsync(bowlerIdByWebsiteId);
}

// You can define other methods, fields, classes and namespaces here
public async Task<IEnumerable<(Guid bowlerId, int? websiteId, int? softwareId, HumanName? softwareName, HumanName? websiteName)>> MigrateBowlersAsync()
{
	DataTable websiteChampionsTable = await QueryStatsDatabaseAsync("select Id, FName, LName from dbo.champions");
	DataTable softwareBowlersTable = await QuerySoftwareDatabaseAsync("select Id, FirstName, MiddleInitial, LastName, Suffix, Champion from dbo.Bowlers");
	
	var websiteBowlers = websiteChampionsTable.AsEnumerable().Select(row => new
	{
		Id = row.Field<int>("Id"),
		Name = new NameParser.HumanName($"{row.Field<string>("FName")} {row.Field<string>("LName")}")
	}).ToList();
	
	int initialWebsiteBowlerCount = websiteBowlers.Count;

	foreach (var websiteBowler in websiteBowlers)
	{
		websiteBowler.Name.Normalize();
	}

	var softwareBowlers = softwareBowlersTable.AsEnumerable().Select(row => new
	{
		Id = row.Field<int>("Id"),
		Champion = row.Field<bool>("Champion"),
		Name = new NameParser.HumanName($"{row.Field<string>("FirstName")} {row.Field<string>("MiddleInitial")} {row.Field<string>("LastName")} {row.Field<string>("Suffix")}")
	}).ToList();
	
	int championCount = softwareBowlers.Count(b => b.Champion);

	//if (initialWebsiteBowlerCount != championCount)
	//{
	//	throw new InvalidOperationException($"Champions missing (or extra) in software.  Website: {initialWebsiteBowlerCount} / Software: {championCount}");
	//}
	
	var mergedBowlers = new List<(Guid bowlerId, int? websiteId, int? softwareId, HumanName? softwareName, HumanName? websiteName)>();

	foreach (var manualMatch in s_manualMatch)
	{
		var softwareBowler = softwareBowlers.SingleOrDefault(s => s.Id == manualMatch.softwareId) ?? throw new InvalidOperationException($"SoftwareId: {manualMatch.softwareId} not found");
		var websiteBowler = manualMatch.websiteId.HasValue ? websiteBowlers.SingleOrDefault(w => w.Id == manualMatch.websiteId.Value) ?? throw new InvalidOperationException($"No matching champion with website id {manualMatch.websiteId}") : null;

		if (websiteBowler is null && softwareBowler.Champion)
		{
			throw new InvalidOperationException($"Software Champion does not have website id: {softwareBowler.Id} {softwareBowler.Name.First} {softwareBowler.Name.Last}");
		}

		if (websiteBowler is not null && !softwareBowler.Champion)
		{
			throw new InvalidOperationException($"Software Non Champion has a website id: {websiteBowler.Id} {websiteBowler.Name.First} {websiteBowler.Name.Last}");
		}
		
		mergedBowlers.Add(new(Guid.NewGuid(), manualMatch.websiteId, manualMatch.softwareId, softwareBowler.Name, websiteBowler?.Name));
		softwareBowlers.Remove(softwareBowler);

		if (websiteBowler is not null)
		{
			websiteBowlers.Remove(websiteBowler);
		}
	}

	foreach (var softwareBowler in softwareBowlers)
	{		
		softwareBowler.Name.Normalize();
		
		var websiteBowler = websiteBowlers.SingleOrDefault(b => b.Name.First == softwareBowler.Name.First 
			&& b.Name.Last == softwareBowler.Name.Last
			&& b.Name.Suffix.Replace(".","") == softwareBowler.Name.Suffix.Replace(".",""));

		if (websiteBowler is not null)
		{
			if (!softwareBowler.Champion)
			{
				throw new InvalidOperationException($"Bowler not listed as champion but on website.  Verify Match / Champion status for {softwareBowler.Name.FullName} / softwareId: {softwareBowler.Id}");
			}
			
			(Guid bowlerId, int? websiteId, int softwareId, HumanName softwareName, HumanName websiteName) mergedBowler 
				= new(Guid.NewGuid(), websiteBowler.Id, softwareBowler.Id, softwareBowler.Name, websiteBowler.Name);
			websiteBowlers.Remove(websiteBowler);
			
			mergedBowlers.Add(mergedBowler);
			
			continue;
		}

		// need to do other possible filtering for manual matches until websiteBowlers count is zero		
		var lastNameMatch = websiteBowlers.Where(b => b.Name.Last == softwareBowler.Name.Last).ToList();
		if (lastNameMatch.Count > 1)
		{
			new { Software = softwareBowler, Website = lastNameMatch.OrderBy(x => x.Name.First) }.Dump();
		}
		if (lastNameMatch.Count == 1 )
		{
			var matchedName = lastNameMatch.Single();
			new { Software = softwareBowler, Website = matchedName }.Dump();
		}
		
		// no website match
		mergedBowlers.Add(new(Guid.NewGuid(), null, softwareBowler.Id, softwareBowler.Name, null));
	}

	foreach (var websiteBowler in websiteBowlers)
	{
		mergedBowlers.Add(new(Guid.NewGuid(), websiteBowler.Id, null, null, websiteBowler.Name));
	}
	
	
	//todo: do we care about all middle initials / suffixes having period at the end?  do we auto format upon saving?
	var mappedBowlers = mergedBowlers.Select(mergedBowler => new Bowlers
	{
		Id = mergedBowler.bowlerId,
		ApplicationId = mergedBowler.softwareId,
		WebsiteId = mergedBowler.websiteId,
		FirstName = mergedBowler.softwareName?.First ?? mergedBowler.websiteName?.First ?? throw new InvalidOperationException($"No First Name for {mergedBowler.softwareId ?? mergedBowler.websiteId}"),
		MiddleName = !string.IsNullOrWhiteSpace(mergedBowler.softwareName?.Middle) ? mergedBowler.softwareName.Middle : !string.IsNullOrWhiteSpace(mergedBowler.websiteName?.Middle) ? mergedBowler.websiteName?.Middle : null,
		LastName = mergedBowler.softwareName?.Last ?? mergedBowler.websiteName?.Last ?? throw new InvalidOperationException($"No Last Name for {mergedBowler.softwareId ?? mergedBowler.websiteId}"),
		Suffix = !string.IsNullOrWhiteSpace(mergedBowler.softwareName?.Suffix) ? mergedBowler.softwareName.Suffix.Replace(".","").Trim() : !string.IsNullOrWhiteSpace(mergedBowler.websiteName?.Suffix) ? mergedBowler.websiteName?.Suffix.Replace(".","").Trim() : null,
		Nickname = !string.IsNullOrWhiteSpace(mergedBowler.softwareName?.Nickname) ? mergedBowler.softwareName.Nickname : !string.IsNullOrWhiteSpace(mergedBowler.websiteName?.Nickname) ? mergedBowler.websiteName?.Nickname : null,
	}).ToList();
	
	//manual name fixes -----------------------------------------------------
	var johnPaulBordage = mappedBowlers.Single(b => b.ApplicationId == 21);
	johnPaulBordage.FirstName = "John Paul";
	johnPaulBordage.MiddleName = null;
	
	var michelleScherrer = mappedBowlers.Single(b => b.ApplicationId == 1185);
	michelleScherrer.LastName = "Scherrer";
	
	var chrisDosSantos = mappedBowlers.Single(b => b.ApplicationId == 2070);
	chrisDosSantos.MiddleName = "D";
	chrisDosSantos.LastName = "Dos Santos";
	
	var nicoleCalca = mappedBowlers.Single(b => b.ApplicationId == 2942);
	nicoleCalca.LastName = "Calca";
	
	var rjBurlone = mappedBowlers.Single(b => b.ApplicationId == 4653);
	rjBurlone.FirstName = "RJ";
	rjBurlone.MiddleName = null;
	
	var laJones = mappedBowlers.Single(b => b.ApplicationId == 1188);
	laJones.FirstName = "L.A.";

	//-----------------------------------------------------------------------

	//foreach (var bowler in mappedBowlers)
	//{
	//	Bowlers.Add(bowler);
	//	await SaveChangesAsync();
	//}
	
	Bowlers.AddRange(mappedBowlers);

	await SaveChangesAsync();
	
	"Bowlers Migrated".Dump();

	return mergedBowlers;
}

public async Task<DataTable> QueryStatsDatabaseAsync(string query)
	=> await QueryDatabaseAsync(Util.GetPassword("nebastatsdb.connectionstring"), query);
	
public async Task<DataTable> QuerySoftwareDatabaseAsync(string query)
	=> await QueryDatabaseAsync(Util.GetPassword("nebamgmtv3.connectionstring"), query);

private async Task<DataTable> QueryDatabaseAsync(string connectionString, string query)
{
	using SqlConnection websiteConnection = new(connectionString);

	await websiteConnection.OpenAsync();

	using SqlCommand command = new(query, websiteConnection);
	SqlDataAdapter sqlDataAdapter = new(command);

	DataTable dataTable = new();
	await Task.Run(() => sqlDataAdapter.Fill(dataTable));

	return dataTable;
}

public sealed class Month : SmartEnum<Month>
{
	/// <summary>
	/// Represents the month of January.
	/// </summary>
	public static readonly Month January = new("January", 1);

	/// <summary>
	/// Represents the month of February.
	/// </summary>
	public static readonly Month February = new("February", 2);

	/// <summary>
	/// Represents the month of March.
	/// </summary>
	public static readonly Month March = new("March", 3);

	/// <summary>
	/// Represents the month of April.
	/// </summary>
	public static readonly Month April = new("April", 4);

	/// <summary>
	/// Represents the month of May.
	/// </summary>
	public static readonly Month May = new("May", 5);

	/// <summary>
	/// Represents the month of June.
	/// </summary>
	public static readonly Month June = new("June", 6);

	/// <summary>
	/// Represents the month of July.
	/// </summary>
	public static readonly Month July = new("July", 7);

	/// <summary>
	/// Represents the month of August.
	/// </summary>
	public static readonly Month August = new("August", 8);

	/// <summary>
	/// Represents the month of September.
	/// </summary>
	public static readonly Month September = new("September", 9);

	/// <summary>
	/// Represents the month of October.
	/// </summary>
	public static readonly Month October = new("October", 10);

	/// <summary>
	/// Represents the month of November.
	/// </summary>
	public static readonly Month November = new("November", 11);

	/// <summary>
	/// Represents the month of December.
	/// </summary>
	public static readonly Month December = new("December", 12);

	/// <summary>
	/// Initializes a new instance of the <see cref="Month"/> class.
	/// </summary>
	/// <param name="name">The full name of the month.</param>
	/// <param name="value">The numeric value of the month (1-12).</param>
	private Month(string name, int value)
		: base(name, value)
	{
	}

	/// <summary>
	/// Here for EF Core materialization purposes only.
	/// </summary>
	private Month()
		: base("", 0)
	{ }

	/// <summary>
	/// Returns the three-letter abbreviation for the month (e.g., "Jan" for January).
	/// </summary>
	/// <returns>The three-letter abbreviated month name.</returns>
	public string ToShortString()
	{
		return this.Name.Substring(0, 3);
	}
}

public sealed class TournamentType
	: SmartEnum<TournamentType>
{
	/// <summary>
	/// Singles tournament (1 player per team).
	/// </summary>
	public static readonly TournamentType Singles = new("Singles", 10, 1,13);

	/// <summary>
	/// Doubles tournament (2 players per team).
	/// </summary>
	public static readonly TournamentType Doubles = new("Doubles", 20, 2, 1);

	/// <summary>
	/// Trios tournament (3 players per team).
	/// </summary>
	public static readonly TournamentType Trios = new("Trios", 30, 3, 2);

	/// <summary>
	/// Non-Champions tournament.
	/// </summary>
	public static readonly TournamentType NonChampions = new("Non-Champs", 11, 1,8);

	/// <summary>
	/// Tournament of Champions event.
	/// </summary>
	public static readonly TournamentType TournamentOfChampions = new("T of C", 12, 1,7);

	/// <summary>
	/// Invitational tournament.
	/// </summary>
	public static readonly TournamentType Invitational = new("Invitational", 13, 1, 11);

	/// <summary>
	/// Masters tournament.
	/// </summary>
	public static readonly TournamentType Masters = new("Masters", 14, 1,12);

	/// <summary>
	/// High Roller tournament.
	/// </summary>
	public static readonly TournamentType HighRoller = new("High Roller", 15, 1, 3);

	/// <summary>
	/// Senior tournament.
	/// </summary>
	public static readonly TournamentType Senior = new("Senior", 16, 1, 4);

	/// <summary>
	/// Women tournament.
	/// </summary>
	public static readonly TournamentType Women = new("Women's", 17, 1,15);

	/// <summary>
	/// Over 40 tournament.
	/// </summary>
	public static readonly TournamentType OverForty = new("Over 40", 18, 1,6);

	/// <summary>
	/// 40-49 age group tournament.
	/// </summary>
	public static readonly TournamentType FortyToFortyNine = new("40 - 49", 19, 1,5);

	/// <summary>
	/// Over/Under 50 Doubles tournament (2 players per team).
	/// </summary>
	public static readonly TournamentType OverUnderFiftyDoubles = new("Over/Under 50 Doubles", 21, 2,14);

	/// <summary>
	/// Over/Under 40 Doubles tournament (2 players per team).
	/// </summary>
	public static readonly TournamentType OverUnderFortyDoubles = new("Under/Over 40 Doubles", 22, 2,9);

	/// <summary>
	/// Initializes a new instance of the <see cref="TournamentType"/> class.
	/// </summary>
	/// <param name="name">The display name of the tournament type.</param>
	/// <param name="value">The unique integer value for the tournament type.</param>
	/// <param name="teamSize">The number of players per team for this tournament type.</param>
	private TournamentType(string name, int value, int teamSize, int websiteId)
		: base(name, value)
	{
		TeamSize = teamSize;
		WebsiteId = websiteId;
	}

	/// <summary>
	/// Here for EF Core materialization purposes only.
	/// </summary>
	private TournamentType()
		: base("", 0)
	{ }

	/// <summary>
	/// Gets the number of players per team for this tournament type.
	/// </summary>
	public int TeamSize { get; }

	public int WebsiteId { get; }
	
	public static TournamentType FromWebsiteId(int websiteId)
		=> List.Single(tournamentType => tournamentType.WebsiteId == websiteId);
}

public async Task MigrateTitlesAsync(Dictionary<int ,Guid> bowlerIdByWebsiteId)
{
	var titlesTable = await QueryStatsDatabaseAsync("select * from dbo.Titles");

	var titles = titlesTable.AsEnumerable()
	.Where(row => row.Field<int>("ChampionId") != 424) //there is a bad row in the database
	.Select(row => new Titles
	{
		Id = Guid.NewGuid(),
		BowlerId = bowlerIdByWebsiteId[row.Field<int>("ChampionId")],
		Month = row.Field<DateTime>("TitleDate").Month,
		Year = row.Field<DateTime>("TitleDate").Year,
		TournamentType = TournamentType.FromWebsiteId(row.Field<int>("Type")).Value
	}).ToList();
	
	Titles.AddRange(titles);
	
	await SaveChangesAsync();
	
	"Titles Migrated".Dump();
}

static List<(int? websiteId, int? softwareId)> s_manualMatch = new()
{
	new(115, 57), 	// Rich Stravato
	new(385, 98), 	// Terry Perssico Jr
	new(473, 273),	// Steve Hardy Jr
	new(407, 318),	// Rich Fulton Jr
	new(82, 364), 	// Niorm Ginsberg
	new(338, 446),	// Ed Veltri
	new(88, 1637),    // Phil Karwoski Sr
	new(523, 598),	// Gary Kurensky Jr
	new(519, 658),	// Zac Gentile
	new(457, 701),	// Dave Paquin Jr
	new(17, 943), 	// Stephen Dale Jr
	new(83, 1022),	// Orville Gordon
	new(525, 1111),   // Mike E Rose Jr (1126 is Mike P Rose Jr)
	new(402, 1236),   // Thomas Coco Jr
	new(120, 1282),   // Jeff Voght
	new(433, 1284),   // Ken Lefebvre
	new(107, 1372),   // Jimmie Pritts Jr (1029 is Jimmie Pritts Sr and needs to be deleted)
	new(335, 1398),   // Peter Valenti, Jr
	new(380, 1406),   // Joshua Corum
	new(3, 1628),	 // Christine Rebstock
	new(155, 1774),   // Steve Brown Jr
	new(396, 1861),   // Bob Greene (Fairfield)
	new(null, 93),	// Robert Greene
	new(98, 642),     // Rick Mochrie Sr
	new(109, 2072),   // Timothy Riordan
	new(157, 2239),   // Ryan Burlone Sr
	new(184, 2305),   // Patrick Donohoe Jr
	new(165, 2445),   // Douglas Carlson
	new(437, 2449),   // Sammy Ventura
	new(476, 2910),   // Jim Sicard
	new(397, 3203),   // Johnny Petraglia Jr
	new(111, 3339),   // Brentt Smith
	new(336, 3384),   // Jon van Hees
	new(360, 3440),   // Dan Gauthier
	new(386, 339),    // Matt Brockett
	new(245, 4170),   // Jeff Lemon
	new(147, 4226),   // Billy Black
	new(415, 290),	// Jayme Silva Sr,
	new(null, 43),	// Brian Smith
	new(null, 67),	// Christopher Brown
	new(null, 77),	// Christopher Baker
	new(null, 124),   // Jason Brown
	new(null, 135),   // Alex Major
	new(null, 138),   // Fred Trudell
	new(null, 144),   // Terry Robinson
	new(null, 193),   // Shirley Major
	new(null, 259),   // Andrew Broege
	new(null, 278),   // Jayme Silva Jr
	new(null, 302),   // David Collins
	new(null, 310),   // Clint Jones
	new(null, 337),   // Scott Hall
	new(null, 346),   // Nick Major
	new(346, 366),	// Michael Williams 
	new(null, 489),   // George Jones
	new(null, 559),   // Roger Major
	new(null, 588),   // Paul Bouchard
	new(null, 639),   // Tim Hansen
	new(null, 683),   // Ken Smith Jr
	new(null, 749),   // Tim L Smith
	new(null, 775),   // Barbara Webb
	new(null, 812),   // John Brown
	new(null, 863),   // Donald Hall
	new(null, 875),   // Josh C Hall
	new(null, 883),   // Ronald Perry
	new(null, 909),   // Marty Jones
	new(null, 928),   // Kevin Brown
	new(null, 959),   // Mike Major
	new(322, 1028),   // Jennifer Burlone (Swanson on Website)
	new(null, 1044),  // Joseph Ferraro Jr
	new(null, 1188),  // L.A. Jones
	new(null, 1231),  // Justin Hansen
	new(null, 1260),  // Brian Perry
	new(null, 1264),  // Jeff Bennett
	new(401, 1373),   // Billy Trudell
	new(null, 1450),  // Tyler Grant
	new(null, 1552),  // Phil Hall
	new(null, 1560),  // Timothy Jones
	new(null, 1613),  // Todd Jones
	new(null, 1866),  // Al Ferraro
	new(null, 1948),  // Timothy Major
	new(null, 1999),  // Brett Bennett
	new(null, 2043),  // James Perry
	new(null, 2194),  // Norm Major
	new(null, 2344),  // Leslie Hall
	new(null, 2364),  // William Webb
	new(366, 2466),   // Tom Hansen
	new(null, 2864),  // Matt Jones
	new(null, 4499),  // Matt I Hansen
	new(null, 19),    // Christopher Lovewell
	new(null, 25),    // Dave Lopes
	new(null, 49),	// Thomas Hamilton
	new(null, 54),    // Pete McConnell
	new(399, 61),     // Dave Debiase
	new(null, 71),    // James Girard
	new(null, 92),    // Kenneth Sweet Sr
	new(347, 104),    // Hank Williamson
	new(null, 109),   // Jim Gillick
	new(null, 141),   // Scott W Green
	new(null, 178),   // Robert White
	new(null, 182),   // Jason R Briggs
	new(null, 201),   // Bill Roberts
	new(null, 241),   // Michale Dove
	new(null, 267),   // Colby Wood
	new(null, 293),   // John White
	new(null, 312),   // Jacob Chase
	new(null, 315),   // Michael Johnson
	new(null, 317),   // Adam Desmarais
	new(null, 329),   // Steve Thomas
	new(null, 354),   // Johnna Williamson
	new(null, 359),   // Aaron Roberts
	new(null, 369),   // Lance Williams
	new(null, 371),   // Bill Cornell
	new(null, 373),   // Eric McConnell
	new(null, 398),   // Chris Girard
	new(null, 399),   // Eric Taylor
	new(null, 415),   // Alonzo McDowell
	new(null, 429),   // Justin M Rollins
	new(372, 433),	// Jay Mahon
	new(null, 437),   // Robert Baral
	new(null, 447),   // George Clark III
	new(null, 458),   // Christopher Blanchette
	new(450, 493),    // Michael Macedo
	new(null, 514),   // Jay Marine
	new(null, 516),   // Country Alfonso Jr
	new(null, 521),   // Don Perillo
	new(null, 540),   // Willie Hanna
	new(null, 547),   // Steve Hugo
	new(null, 556),   // Adam Harvey
	new(358, 578),    // Bill Tessier
	new(null, 594),   // Stephen Rogers
	new(null, 610),   // Jaime Tessier
	new(84, 616),     // Jim Harger
	new(null, 633),   // Brian McNeil
	new(null, 682),   // Jim Thomas
	new(null, 709),   // James White
	new(null, 730),   // Greg Rogers
	new(411, 763),    // Stephen Puishys
	new(null, 784),   // Craig Coplan
	new(null, 809),   // John-david Edwards
	new(null, 814),   // Don Fournier
	new(null, 817),   // Greg White
	new(null, 819),   // Calvin Sellers
	new(null, 844),   // Paul Dumas
	new(null, 854),   // Maurice Thomas
	new(null, 877),   // Frank Mirabile
	new(null, 887),   // Steve Cote
	new(null, 907),   // Lenny Colby
	new(null, 917),   // Kenny Sweet
	new(null, 930),   // John Miskolczi
	new(null, 939),   // Don Harger III
	new(null, 981),   // Amy Robinson
	new(null, 999),   // Ken Fortier
	new(null, 1018),  // Eddie Hanna
	new(null, 1061),  // Daniel Solimine
	new(null, 1065),  // Len Robertson
	new(null, 1076),  // Raymond Clark
	new(455, 1081),   // Nicholas Marien
	new(null, 1086),  // Joey Baral
	new(null, 1102),  // Tom Walsh Jr
	new(null, 1114),  // Raymond Desmarais
	new(null, 1117),  // Ernie Franklin
	new(null, 1128),  // Jim Williams
	new(null, 1157),  // Matt Brown
	new(null, 1172),  // Stephen King
	new(null, 1185),  // Michelle Peloquin (Scherrer)
	new(118, 1186),   // Robert Tetrault
	new(null, 1204),  // Robert Tobin
	new(null, 1253),  // Ron Rusin Sr
	new(null, 1262),  // Bryan Travers
	new(null, 1286),  // John Papa
	new(null, 1297),  // Mike McDowell
	new(null, 1344),  // Liat Cornog
	new(null, 1350),  // Andrew Weeks (not dup of Andy Weeks)
	new(null, 1355),  // Brian White
	new(null, 1366),  // Andre White
	new(null, 1386),  // Jeff A Baker
	new(null, 1399),  // James Oliver
	new(null, 1404),  // Ashlie Kipperman
	new(221, 1448),   // Duncan Harvey (on website as Duan)
	new(null, 1465),  // Joshua Hurne
	new(null, 1468),  // Tim Brown
	new(null, 1498),  // Keith Taylor
	new(null, 1531),  // Shawn Fitzpatrick Sr
	new(null, 1533),  // Eric Camara
	new(null, 1540),  // Michael Walsh
	new(null, 1564),  // Ray Lathrop
	new(null, 1566),  // Matthew Demello
	new(null, 1578),  // Janine Forry
	new(null, 1584),  // Bob Dimuccio Sr
	new(null, 1590),  // John Perillo III
	new(null, 1595),  // Greg Brown
	new(null, 1600),  // Dennis Robinson
	new(null, 1603),  // Jeffery Bedard
	new(null, 1606),  // Robert Fredette
	new(null, 1619),  // Joshua Sweet
	new(null, 1622),  // Joey Bouchard
	new(null, 1638),  // Rick Bogholtz
	new(null, 1654),  // Matt Favreau
	new(341, 1667),   // Robert Volk
	new(null, 1669),  // Stephen Wood
	new(null, 1675),  // Ryan Morgan
	new(null, 1730),  // Ed Couture
	new(null, 1734),  // Mark A Taylor
	new(null, 1758),  // Tim M Frye
	new(null, 1804),  // David Hebert
	new(null, 1837),  // Scott Miranda
	new(null, 1843),  // Sean White
	new(null, 1846),  // Tyler Coplan
	new(null, 1859),  // Pete Tremblay
	new(null, 1870),  // Pat Hayes
	new(null, 1885),  // Mas Thomas
	new(null, 1903),  // Shawn Taylor
	new(null, 1907),  // Cristina M Reale
	new(null, 1938),  // Cassaundra L Collins
	new(null, 1949),  // John W Bedard
	new(null, 2006),  // David Rogers
	new(null, 2032),  // Bob Hamilton
	new(null, 2039),  // Billy Heath
	new(null, 2042),  // Chuck Douglas
	new(null, 2070),  // Chris D Dos Santos
	new(321, 2077),   // Gordy Strain
	new(null, 2085),  // Dave Dupuis
	new(260, 2092),   // David Miranda
	new(null, 2112),  // Mark James
	new(null, 2129),  // Rich Lecain
	new(23, 2135),    // Mike Klapik
	new(null, 2195),  // Scott Morgan
	new(null, 2221),  // Eric Robinson
	new(null, 2298),  // Jesse Verni
	new(158, 2299),   // Chuck Burr
	new(438, 2314),   // Jonathan Sellers
	new(null, 2327),  // Richard Hardy
	new(null, 2334),  // Dan Lambert
	new(null, 2351),  // Derek Reynolds
	new(null, 2354),  // Jason Lopes
	new(null, 2395),  // Chris Thomas
	new(null, 2425),  // Jeffrey Santos
	new(null, 2428),  // Gary Couture
	new(null, 2477),  // Dave Constantine
	new(null, 2520),  // Tony Ferraro
	new(null, 2523),  // Adam Fischer
	new(null, 2530),  // Chuck Hardy
	new(320, 2548),   // Robert Snell Sr (Bob Snell on site)
	new(null, 2563),  // Jim Taylor
	new(null, 2633),  // Kenny Hall
	new(null, 2638),  // David Morgan
	new(null, 2639),  // Donald Hardy
	new(31, 2696),	// Mike Colby
	new(null, 2698),  // Frank Brown
	new(null, 2714),  // Bryan L Thomas
	new(null, 2734),  // Damion Collins
	new(null, 2754),  // Sean S Thomas
	new(92, 2760),    // David Lisowski
	new(null, 2792),  // Nelson Brown Jr
	new(null, 2798),  // Justin McNeil
	new(205, 2825),   // Gerald Gitlitz
	new(null, 2889),  // Michael Brown
	new(146, 2851),   // Brandan Bierch
	new(466, 2903),   // Charlie Bonis Jr
	new(393, 2934),   // Rob Greene
	new(null, 2942),  // Nicole Calca
	new(null, 2956),  // Scott Hurne Jr
	new(null, 2964),  // Sean Perry
	new(null, 3006),  // Thomas McConnell,
	new(468, 3018),   // Joseph Lussier
	new(null, 3041),  // Craig Taylor
	new(null, 3078),  // Allan Tremblay
	new(null, 3089),  // Thomas Mongeon
	new(null, 3099),  // Benjamin Rosen
	new(null, 3103),  // Tom Taylor
	new(null, 3112),  // Ferrill McNeil
	new(null, 3141),  // Gian Papa
	new(null, 3208),  // Michael Erickson
	new(null, 3214),  // Bert Santos
	new(null, 3255),  // Keith Perry
	new(null, 3265),  // Melissa Brown
	new(null, 3295),  // Tony R Federici
	new(null, 3306),  // Jake Tobin
	new(null, 3351),  // John Hayes Jr
	new(null, 3358),  // Bill Renaud
	new(316, 3374),   // Michael Shoop
	new(null, 3391),  // Jamie Taylor
	new(null, 3394),  // Arthur Baker III
	new(null, 3433),  // Dennis Reale
	new(null, 3495),  // Bill L Baker Jr
	new(null, 3499),  // Dave Perry
	new(486, 3528),   // Joseph C Colcord
	new(null, 3563),  // Josh Baker
	new(null, 3589),  // Travis A Shaw
	new(null, 3610),  // Benjamin M Mullen
	new(null, 3633),  // Justin A Shaw
	new(null, 3750),   // Joseph P Lussier Jr
	new(null, 3858),  // Don Frye
	new(461, 3909),   // David Simard
	new(null, 3917),  // Eldon Dunbar IV
	new(null, 3942),  // Ray J Carhart
	new(null, 4000),  // Oliver O Bernier
	new(null, 4013),  // Riley P Taylor
	new(null, 4096),  // Josh Morgan
	new(null, 4104),  // Matt Bouchard
	new(null, 4109),  // Brandon J Crandall
	new(null, 4118),  // Eldon H DUnbar III
	new(null, 4134),  // Wesley Grant
	new(null, 4182),  // Jesse P Robinson
	new(null, 4250),  // Tyler A Baker
	new(513, 4252),   // Nathaniel J Clarke
	new(null, 4313),  // John J Santos
	new(null, 4323),  // Raychon Brown
	new(null, 4360),  // Michael J Armstrong
	new(516, 4386),   // Mike J Puzo
	new(null, 4396),  // Jordan Byrnes
	new(null, 4462),  // Morgan Walsh
	new(null, 4550),  // Andre Thomas
	new(null, 4567),  // James Baker
	new(null, 4583),  // Jennifer Bernier
	new(null, 4613),  // Joseph E Carney
	new(null, 4653),  // RJ Burlone
	new(null, 4655),  // Zach Tobin
	new(null, 4659),  // Christopher D Taylor
	new(null, 4670),  // Kendall R Robinson
	new(null, 4675),  // Nicole L Taylor
	new(null, 4754),  // Keith Robinson
	new(null, 4793),  // François Couture
	new(null, 4808),  // Aidan Robinson
	new(null, 4815),  // Colin Robinson
	new(null, 4843),  // Matthew Shaw
	new(null, 4868),  // Taylon K Bernier
	new(null, 4891),  // Brady Robinson
	new(null, 4931),  // Zachary J Taylor
	new(null, 4942),  // Dylan M Grant
	new(null, 4943),  // Jay Hayes
	new(null, 4993),  // Gavin Brown
	new(null, 5014),  // Thomas H Brown II
};