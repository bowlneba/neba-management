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
  <NuGetReference>Ardalis.SmartEnum</NuGetReference>
  <Namespace>Ardalis.SmartEnum</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
</Query>

void Main()
{
	var rawJson = File.ReadAllText("/Users/kippermand/Desktop/titles-year.json");
	var websiteTitles = JsonSerializer.Deserialize<List<WebsiteTitles>>(rawJson);
	
	var bowlerIdLookup = Bowlers.ToDictionary(b => b.WebsiteId, b => b.Id);

	var titles = websiteTitles.Select(title => new Titles
	{
		Id = Guid.NewGuid(),
		BowlerId = bowlerIdLookup[title.BowlerId],
		Month = title.Month,
		Year = title.Year,
		TournamentType = TournamentType.FromName(title.TournamentType)
	}).ToList();
	
	Titles.AddRange(titles);
	SaveChanges();
}

// You can define other methods, fields, classes and namespaces here
public sealed record WebsiteTitles(
[property: JsonPropertyName("bowlerId")]int BowlerId, 
[property: JsonPropertyName("month")]int Month, 
[property: JsonPropertyName("year")]int Year, 
[property: JsonPropertyName("tournamentType")]string TournamentType);

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
	public static readonly TournamentType Singles = new("Singles", 10, 1);

	/// <summary>
	/// Doubles tournament (2 players per team).
	/// </summary>
	public static readonly TournamentType Doubles = new("Doubles", 20, 2);

	/// <summary>
	/// Trios tournament (3 players per team).
	/// </summary>
	public static readonly TournamentType Trios = new("Trios", 30, 3);

	/// <summary>
	/// Non-Champions tournament.
	/// </summary>
	public static readonly TournamentType NonChampions = new("Non-Champs", 11, 1);

	/// <summary>
	/// Tournament of Champions event.
	/// </summary>
	public static readonly TournamentType TournamentOfChampions = new("T of C", 12, 1);

	/// <summary>
	/// Invitational tournament.
	/// </summary>
	public static readonly TournamentType Invitational = new("Invitational", 13, 1);

	/// <summary>
	/// Masters tournament.
	/// </summary>
	public static readonly TournamentType Masters = new("Masters", 14, 1);

	/// <summary>
	/// High Roller tournament.
	/// </summary>
	public static readonly TournamentType HighRoller = new("High Roller", 15, 1);

	/// <summary>
	/// Senior tournament.
	/// </summary>
	public static readonly TournamentType Senior = new("Senior", 16, 1);

	/// <summary>
	/// Women tournament.
	/// </summary>
	public static readonly TournamentType Women = new("Women's", 17, 1);

	/// <summary>
	/// Over 40 tournament.
	/// </summary>
	public static readonly TournamentType OverForty = new("Over 40", 18, 1);

	/// <summary>
	/// 40-49 age group tournament.
	/// </summary>
	public static readonly TournamentType FortyToFortyNine = new("40 - 49", 19, 1);

	/// <summary>
	/// Over/Under 50 Doubles tournament (2 players per team).
	/// </summary>
	public static readonly TournamentType OverUnderFiftyDoubles = new("Over/Under 50 Doubles", 21, 2);

	/// <summary>
	/// Over/Under 40 Doubles tournament (2 players per team).
	/// </summary>
	public static readonly TournamentType OverUnderFortyDoubles = new("Under/Over 40 Doubles", 22, 2);

	/// <summary>
	/// Initializes a new instance of the <see cref="TournamentType"/> class.
	/// </summary>
	/// <param name="name">The display name of the tournament type.</param>
	/// <param name="value">The unique integer value for the tournament type.</param>
	/// <param name="teamSize">The number of players per team for this tournament type.</param>
	private TournamentType(string name, int value, int teamSize)
		: base(name, value)
	{
		TeamSize = teamSize;
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
}