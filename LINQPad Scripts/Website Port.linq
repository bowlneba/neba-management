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
  <Namespace>Microsoft.Data.SqlClient</Namespace>
  <Namespace>NameParser</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	DataTable websiteChampionsTable = await QueryStatsDatabaseAsync("select Id, FName, LName from dbo.champions");
	DataTable softwareBowlersTable = await QuerySoftwareDatabaseAsync("select Id, FirstName, MiddleInitial, LastName, Suffix from dbo.Bowlers");

	var mergedBowlers = MergeBowlers(websiteChampionsTable, softwareBowlersTable);

	foreach (var mergedBowler in mergedBowlers)
	{
		var bowler = new Bowlers
		{
			Id = mergedBowler.bowlerId,
			ApplicationId = mergedBowler.softwareId,
			WebsiteId = mergedBowler.websiteId,
			FirstName = mergedBowler.softwareName.First,
			MiddleInitial = !string.IsNullOrWhiteSpace(mergedBowler.softwareName.Middle) ? mergedBowler.softwareName.Middle : null,
			LastName = mergedBowler.softwareName.Last,
			Suffix = !string.IsNullOrWhiteSpace(mergedBowler.softwareName.Suffix) ? mergedBowler.softwareName.Suffix : null,
			Nickname = !string.IsNullOrWhiteSpace(mergedBowler.softwareName.Nickname) ? mergedBowler.softwareName.Nickname : null
		};
		
		Bowlers.Add(bowler);
	}
	
	await SaveChangesAsync();
	
	var bowlerIdBySoftwareId = mergedBowlers.ToDictionary(b => b.softwareId, b => b.bowlerId);
	var bowlerIdByWebsiteId = mergedBowlers.Where(b => b.websiteId.HasValue).ToDictionary(b => b.websiteId!.Value, b=> b.bowlerId);
	
	var softwareNamesBySoftwareId = mergedBowlers.ToDictionary(b => b.softwareId, b=> b.softwareName);
	var websiteNamesByWebsiteId = mergedBowlers.Where(b => b.websiteId.HasValue).ToDictionary(b => b.websiteId!.Value, b=> b.websiteName!);
}

// You can define other methods, fields, classes and namespaces here
public IEnumerable<(Guid bowlerId, int? websiteId, int softwareId, HumanName softwareName, HumanName websiteName)> MergeBowlers(DataTable websiteBowlersTable, DataTable softwareBowlersTable)
{
	var websiteBowlers = websiteBowlersTable.AsEnumerable().Select(row => new
	{
		Id = row.Field<int>("Id"),
		Name = new NameParser.HumanName($"{row.Field<string>("FName")} {row.Field<string>("LName")}")
	}).ToList();

	foreach (var websiteBowler in websiteBowlers)
	{
		websiteBowler.Name.Normalize();
	}

	var softwareBowlers = softwareBowlersTable.AsEnumerable().Select(row => new
	{
		Id = row.Field<int>("Id"),
		Name = new NameParser.HumanName($"{row.Field<string>("FirstName")} {row.Field<string>("MiddleInitial")} {row.Field<string>("LastName")} {row.Field<string>("Suffix")}")
	}).ToList();
	
	var mergedBowlers = new List<(Guid bowlerId, int? websiteId, int softwareId, HumanName softwareName, HumanName? websiteName)>();

	foreach (var softwareBowler in softwareBowlers)
	{
		softwareBowler.Name.Normalize();
		
		var websiteBowler = websiteBowlers.SingleOrDefault(b => b.Name.First == softwareBowler.Name.First 
			&& b.Name.Last == softwareBowler.Name.Last
			&& b.Name.Suffix == softwareBowler.Name.Suffix);

		if (websiteBowler is not null)
		{
			(Guid bowlerId, int? websiteId, int softwareId, HumanName softwareName, HumanName websiteName) mergedBowler 
				= new(Guid.NewGuid(), websiteBowler.Id, softwareBowler.Id, softwareBowler.Name, websiteBowler.Name);
			websiteBowlers.Remove(websiteBowler);
			
			mergedBowlers.Add(mergedBowler);
			
			continue;
		}
		
		// no website match
		mergedBowlers.Add(new(Guid.NewGuid(), null, softwareBowler.Id, softwareBowler.Name, null));
	}
	
	websiteBowlers.Dump("No Software Match");
	
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