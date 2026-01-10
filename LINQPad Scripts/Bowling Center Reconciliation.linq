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
      <ExtraCxOptions>Include Error Detail=true;</ExtraCxOptions>
    </DriverData>
  </Connection>
  <NuGetReference>Google.Apis.Sheets.v4</NuGetReference>
  <NuGetReference>Google.Apis.Auth</NuGetReference>
  <NuGetReference>Ulid</NuGetReference>
  <Namespace>Google.Apis.Auth.OAuth2</Namespace>
  <Namespace>Google.Apis.Services</Namespace>
  <Namespace>Google.Apis.Sheets.v4</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
</Query>

void Main()
{
	// read google sheets
	var json = Util.GetPassword("credentials.sheets.google");

	var serviceAccountCredential = CredentialFactory.FromJson<ServiceAccountCredential>(json);
	var googleCredential = serviceAccountCredential.ToGoogleCredential().CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

	SheetsService sheetsService = new(new BaseClientService.Initializer
	{
		HttpClientInitializer = googleCredential,
		ApplicationName = "NEBA Api"
	});
	
	const string spreadsheetId = "1dMTZ4OiXqkFmQNXBUjREVKH_cduwLmZDIcpWmhJY6ds";
	const string tableName = "Tournaments";
	
	var request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, tableName);
	var response = request.Execute();
	
	var values = response.Values;
	
	var tournaments = new List<TournamentRecord>();
	
	var centersNotMatchedByUsbcName = new HashSet<string>();
	var centersMatchedByUsbcName = new HashSet<string>();
	
	var bowlingCenterNames = BowlingCenters.Select(bc => bc.Name).ToList();
	
	var firstTimeAtCenter = new Dictionary<string, int>();
	var lastTimeAtCenter = new Dictionary<string, int>();

	foreach (var row in values.Skip(1))
	{
		var tournamentRecord = new TournamentRecord
		{
			OverallTournamentCount = int.TryParse(row[0]?.ToString(), out var overallCount) ? overallCount : 0,
			SinglesTournamentCount = int.TryParse(row[1]?.ToString(), out var singleCount) ? singleCount : null,
			Month = int.TryParse(row[2]?.ToString(), out var month) ? month : 0,
			Year = int.TryParse(row[3]?.ToString(), out var year) ? year : 0,
			SoftwareId = int.TryParse(row[4]?.ToString(), out var softwareId) ? softwareId : null,
			TournamentName = row[5]?.ToString() ?? string.Empty,
			StartDate = DateOnly.TryParse(row[6]?.ToString(), out var startDate) ? startDate : DateOnly.MinValue,
			EndDate = DateOnly.TryParse(row[7]?.ToString(), out var endDate) ? endDate : DateOnly.MaxValue,
			TournamentType = row[8]?.ToString() ?? string.Empty,
			BowlingCenter = row[9]?.ToString() ?? string.Empty,
			CityState = row[10]?.ToString() ?? string.Empty,
			Entries = int.TryParse(row[12]?.ToString(), out var entries) ? entries : 0,
			Winners = row[13]?.ToString()?.Split("/").Select(id => int.TryParse(id, out var idValue) ? idValue : 0).ToList().AsReadOnly() ?? []
		};
		
		tournaments.Add(tournamentRecord);

		if ((!string.IsNullOrWhiteSpace(tournamentRecord.BowlingCenter) && !bowlingCenterNames.Contains(tournamentRecord.BowlingCenter)) || tournamentRecord.BowlingCenter == "Valley Bowl" || tournamentRecord.BowlingCenter.Contains("Meadow") && tournamentRecord.CityState.Contains("CT"))
		{
			if (centersNotMatchedByUsbcName.Add($"{tournamentRecord.BowlingCenter} - {tournamentRecord.CityState}"))
			{
				firstTimeAtCenter.Add($"{tournamentRecord.BowlingCenter} - {tournamentRecord.CityState}", tournamentRecord.Year);	
			}
			
			lastTimeAtCenter[$"{tournamentRecord.BowlingCenter} - {tournamentRecord.CityState}"] = tournamentRecord.Year;
		}
		else
		{
			centersMatchedByUsbcName.Add($"{tournamentRecord.BowlingCenter} - {tournamentRecord.CityState}");
		}
	}

	centersMatchedByUsbcName.Order().Dump("USBC Match As Entered");
	
	//todo: need to create new list (and remove from not matched) of centers manually accounted for in port
	HashSet<string> centersManuallyUpdated = [];
	centersNotMatchedByUsbcName.Remove("AMF Hamden Lanes - Hamden, CT");
	centersManuallyUpdated.Add("AMF Hamden Lanes - Hamden, CT");
	
	centersManuallyUpdated.Dump("Centers Manually Handled");
	
	centersNotMatchedByUsbcName.Select(name => new 
	{
		Name = name.Split(" - ")[0],
		Location = name.Split(" - ")[1],
		FirstYear = firstTimeAtCenter[name],
		LastYear = lastTimeAtCenter[name]
	}).OrderBy(x => x.Location).Dump("No USBC Match As Entered");
}

// You can define other methods, fields, classes and namespaces here
public sealed record TournamentRecord
{
	public required int OverallTournamentCount {get; init;}

	public int? SinglesTournamentCount {get;init;}

	public required int Month {get;init;}

	public required int Year {get;init;}

	public int? SoftwareId {get;init;}

	public required string TournamentName {get; init;}

	public required DateOnly StartDate {get; init;}

	public required DateOnly EndDate {get; init;}

	public required string TournamentType { get; init;}

	public required string BowlingCenter { get; init;}

	public required string CityState {get;init;}

	public int? Entries {get;init;}

	public IReadOnlyCollection<int>? Winners {get;init;}
}