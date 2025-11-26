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
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
</Query>

//https://claude.ai/chat/831a16f0-c810-427c-8d96-e23ecf18bd87

void Main()
{
	Bowlers.RemoveRange(Bowlers);
	SaveChanges();
	
	var rawJson = File.ReadAllText("/Users/kippermand/Desktop/champions-export.json");

	var websiteBowlers = JsonSerializer.Deserialize<List<WebsiteBowler>>(rawJson);

	foreach (var websiteBowler in websiteBowlers)
	{
		var bowler = new Bowlers
		{
			Id = Guid.NewGuid(),
			FirstName = websiteBowler.FirstName,
			MiddleInitial = string.IsNullOrWhiteSpace(websiteBowler.MiddleInitial) ? null : websiteBowler.MiddleInitial,
			LastName = websiteBowler.LastName.Replace(",",string.Empty),
			Suffix = string.IsNullOrWhiteSpace(websiteBowler.Suffix) ? null : websiteBowler.Suffix,
			Nickname = null,
			WebsiteId = websiteBowler.Id,
			ApplicationId = null //look into pulling from existing, or just update later
		};

		if (bowler.WebsiteId == 484) //Ditto
		{
			bowler.Nickname = "Ditto";
			bowler.LastName = "Fitzpatrick";
		}
		
		Bowlers.Add(bowler);
	}
	
	SaveChanges();
}

// You can define other methods, fields, classes and namespaces here
public record WebsiteBowler(
	[property: JsonPropertyName("bowlerId")]int Id, 
	[property: JsonPropertyName("firstName")]string FirstName, 
	[property: JsonPropertyName("middleInitial")]string MiddleInitial, 
	[property: JsonPropertyName("lastName")]string LastName, 
	[property: JsonPropertyName("suffix")]string Suffix);