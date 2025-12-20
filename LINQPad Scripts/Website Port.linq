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
  <NuGetReference>Microsoft.Data.SqlClient</NuGetReference>
  <NuGetReference>NameParserSharp</NuGetReference>
  <NuGetReference>Ardalis.SmartEnum</NuGetReference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <NuGetReference>Ulid</NuGetReference>
  <Namespace>Ardalis.SmartEnum</Namespace>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>Microsoft.Data.SqlClient</Namespace>
  <Namespace>NameParser</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
</Query>

async Task Main()
{
	SeasonAwards.RemoveRange(SeasonAwards);
	Titles.RemoveRange(Titles);
	Bowlers.RemoveRange(Bowlers);
	
	await SaveChangesAsync();

	var mergedBowlers = await MigrateBowlersAsync();
	
	var bowlerDbIdByBowlerId = Bowlers.ToDictionary(b => Ulid.Parse(b.BowlerId), b => b.DbId);
	
	var bowlerIdsBySoftwareId = mergedBowlers.Where(b => b.softwareId.HasValue).ToDictionary(b => b.softwareId!.Value, b => b.bowlerId);
	var bowlerIdsByWebsiteId = mergedBowlers.Where(b => b.websiteId.HasValue).ToDictionary(b => b.websiteId!.Value, b=> b.bowlerId);
	
	var softwareNamesBySoftwareId = mergedBowlers.Where(b => b.softwareId.HasValue).ToDictionary(b => b.softwareId!.Value, b=> b.softwareName);
	var websiteNamesByWebsiteId = mergedBowlers.Where(b => b.websiteId.HasValue).ToDictionary(b => b.websiteId!.Value, b=> b.websiteName!);

	var bowlerIdsByWebsiteName = (from bowlerNameByWebsiteId in websiteNamesByWebsiteId
								  from bowlerIdByWebsiteId in bowlerIdsByWebsiteId
								  where bowlerNameByWebsiteId.Key == bowlerIdByWebsiteId.Key
								  select new KeyValuePair<HumanName, Ulid>(websiteNamesByWebsiteId[bowlerIdByWebsiteId.Key], bowlerIdsByWebsiteId[bowlerIdByWebsiteId.Key])).ToList();

	var bowlerIdsBySoftwareName = (from bowlerNameBySoftwareId in softwareNamesBySoftwareId
								   from bowlerIdBySoftwareId in bowlerIdsBySoftwareId
								   where bowlerNameBySoftwareId.Key == bowlerIdBySoftwareId.Key
								   select new KeyValuePair<HumanName, Ulid>(softwareNamesBySoftwareId[bowlerIdBySoftwareId.Key]!, bowlerIdsBySoftwareId[bowlerIdBySoftwareId.Key])).ToList();

	await MigrateTitlesAsync(bowlerIdsByWebsiteId, bowlerDbIdByBowlerId);
	await MigrateBowlerOfTheYears(bowlerIdsByWebsiteName, bowlerIdsBySoftwareName, bowlerDbIdByBowlerId);
	await MigrateHighBlockAsync(bowlerIdsByWebsiteName, bowlerIdsBySoftwareName, bowlerDbIdByBowlerId);
	await MigrateHighAverageAsync(bowlerIdsByWebsiteName, bowlerIdsBySoftwareName, bowlerDbIdByBowlerId);
	
	"Migration Complete".Dump();
}

// You can define other methods, fields, classes and namespaces here
public async Task<IEnumerable<(Ulid bowlerId, int? websiteId, int? softwareId, HumanName? softwareName, HumanName? websiteName)>> MigrateBowlersAsync()
{
	DataTable websiteChampionsTable = await QueryStatsDatabaseAsync("select Id, FName, LName from dbo.champions");
	DataTable softwareBowlersTable = await QuerySoftwareDatabaseAsync("select Id, FirstName, MiddleInitial, LastName, Suffix, Champion from dbo.Bowlers");

	var websiteBowlers = websiteChampionsTable.AsEnumerable().Select(row => new
	{
		Id = row.Field<int>("Id"),
		Name = new NameParser.HumanName($"{row.Field<string>("FName")} {row.Field<string>("LName")}")
	}).Shuffle().ToList();

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
	}).Shuffle().ToList();

	int championCount = softwareBowlers.Count(b => b.Champion);

	//if (initialWebsiteBowlerCount != championCount)
	//{
	//	throw new InvalidOperationException($"Champions missing (or extra) in software.  Website: {initialWebsiteBowlerCount} / Software: {championCount}");
	//}

	var mergedBowlers = new List<(Ulid bowlerId, int? websiteId, int? softwareId, HumanName? softwareName, HumanName? websiteName)>();

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

		mergedBowlers.Add(new(new Ulid(Guid.NewGuid()), manualMatch.websiteId, manualMatch.softwareId, softwareBowler.Name, websiteBowler?.Name));
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
			&& b.Name.Suffix.Replace(".", "") == softwareBowler.Name.Suffix.Replace(".", ""));

		if (websiteBowler is not null)
		{
			if (!softwareBowler.Champion)
			{
				throw new InvalidOperationException($"Bowler not listed as champion but on website.  Verify Match / Champion status for {softwareBowler.Name.FullName} / softwareId: {softwareBowler.Id}");
			}

			(Ulid bowlerId, int? websiteId, int softwareId, HumanName softwareName, HumanName websiteName) mergedBowler
				= new(new Ulid(Guid.NewGuid()), websiteBowler.Id, softwareBowler.Id, softwareBowler.Name, websiteBowler.Name);
			websiteBowlers.Remove(websiteBowler);

			mergedBowlers.Add(mergedBowler);

			continue;
		}

		// need to do other possible filtering for manual matches until websiteBowlers count is zero		
		var lastNameMatch = websiteBowlers.Where(b => b.Name.Last == softwareBowler.Name.Last).ToList();
		if (lastNameMatch.Count > 1)
		{
			new { Software = softwareBowler, Website = lastNameMatch.OrderBy(x => x.Name.First) }.Dump($"Multi Match: {lastNameMatch.First().Name.Last}");
		}
		if (lastNameMatch.Count == 1)
		{
			var matchedName = lastNameMatch.Single();
			new { Software = softwareBowler, Website = matchedName }.Dump($"Single Match: {matchedName.Name.Last}");
		}

		// no website match
		mergedBowlers.Add(new(new Ulid(Guid.NewGuid()), null, softwareBowler.Id, softwareBowler.Name, null));
	}

	foreach (var websiteBowler in websiteBowlers)
	{
		mergedBowlers.Add(new(new Ulid(Guid.NewGuid()), websiteBowler.Id, null, null, websiteBowler.Name));
	}


	//todo: do we care about all middle initials / suffixes having period at the end?  do we auto format upon saving?
	var mappedBowlers = mergedBowlers.Select(mergedBowler => new Bowlers
	{
		BowlerId = mergedBowler.bowlerId.ToString(),
		ApplicationId = mergedBowler.softwareId,
		WebsiteId = mergedBowler.websiteId,
		FirstName = mergedBowler.softwareName?.First ?? mergedBowler.websiteName?.First ?? throw new InvalidOperationException($"No First Name for {mergedBowler.softwareId ?? mergedBowler.websiteId}"),
		MiddleName = !string.IsNullOrWhiteSpace(mergedBowler.softwareName?.Middle) ? mergedBowler.softwareName.Middle : !string.IsNullOrWhiteSpace(mergedBowler.websiteName?.Middle) ? mergedBowler.websiteName?.Middle : null,
		LastName = mergedBowler.softwareName?.Last ?? mergedBowler.websiteName?.Last ?? throw new InvalidOperationException($"No Last Name for {mergedBowler.softwareId ?? mergedBowler.websiteId}"),
		Suffix = !string.IsNullOrWhiteSpace(mergedBowler.softwareName?.Suffix) ? mergedBowler.softwareName.Suffix.Replace(".", "").Trim() : !string.IsNullOrWhiteSpace(mergedBowler.websiteName?.Suffix) ? mergedBowler.websiteName?.Suffix.Replace(".", "").Trim() : null,
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
	
	// do a query based on bowler id to get the db id.  it should be a static list so everything can just get it based on bowler id

	return mergedBowlers;
}

public async Task MigrateTitlesAsync(Dictionary<int, Ulid> bowlerIdByWebsiteId, Dictionary<Ulid, int> bowlerDbIdByBowlerId)
{
	var titlesTable = await QueryStatsDatabaseAsync("select * from dbo.Titles");

	var titles = titlesTable.AsEnumerable()
	.Where(row => row.Field<int>("ChampionId") != 424) //there is a bad row in the database
	.Select(row => new Titles
	{
		TitleId = new Ulid(Guid.NewGuid()).ToString(),
		BowlerDbId = bowlerDbIdByBowlerId[bowlerIdByWebsiteId[row.Field<int>("ChampionId")]],
		Month = row.Field<DateTime>("TitleDate").Month,
		Year = row.Field<DateTime>("TitleDate").Year,
		TournamentType = TournamentType.FromWebsiteId(row.Field<int>("Type")).Value
	}).ToList();

	Titles.AddRange(titles);

	
	await SaveChangesAsync();

	"Titles Migrated".Dump();
}

public async Task MigrateBowlerOfTheYears(
	IReadOnlyCollection<KeyValuePair<HumanName, Ulid>> bowlerIdsByWebsiteName,
	IReadOnlyCollection<KeyValuePair<HumanName, Ulid>> bowlerIdsBySoftwareName,
	Dictionary<Ulid, int> bowlerDbIdByBowlerId)
{
	var bowlerOfTheYearsTask = BowlerOfTheYearScraper.ScrapeAsync(@"https://www.bowlneba.com/history/bowler-of-the-year/");
	var womanOfTheYearsTask = BowlerOfTheYearScraper.ScrapeAsync(@"https://www.bowlneba.com/history/woman-bowler-of-the-year/");
	var seniorOfTheYearsTask = BowlerOfTheYearScraper.ScrapeAsync(@"https://www.bowlneba.com/history/senior-bowler-of-the-year/");
	var superSeniorOfTheYearsTask = BowlerOfTheYearScraper.ScrapeAsync(@"https://www.bowlneba.com/history/super-senior-bowler-of-the-year/");
	var rookieOfTheYearsTask = BowlerOfTheYearScraper.ScrapeAsync(@"https://www.bowlneba.com/history/rookie-of-the-year/");
	var youthOfTheYearsTask = BowlerOfTheYearScraper.ScrapeAsync(@"https://www.bowlneba.com/history/youth-bowler-of-the-year/");

	await Task.WhenAll(
		bowlerOfTheYearsTask,
		womanOfTheYearsTask,
		seniorOfTheYearsTask,
		superSeniorOfTheYearsTask,
		rookieOfTheYearsTask,
		youthOfTheYearsTask);

	var bowlerOfTheYears = await bowlerOfTheYearsTask;
	var womanOfTheYears = await womanOfTheYearsTask;
	var seniorOfTheYears = await seniorOfTheYearsTask;
	var superSeniorOfTheYears = await superSeniorOfTheYearsTask;
	var rookieOfTheYears = await rookieOfTheYearsTask;
	var youthOfTheYears = await youthOfTheYearsTask;

	foreach (var bowlerOfTheYear in bowlerOfTheYears)
	{
		await MigrateBowlerOfTheYear(BowlerOfTheYearCategory.Open, bowlerOfTheYear.year, bowlerOfTheYear.name, bowlerIdsByWebsiteName, bowlerIdsBySoftwareName, bowlerDbIdByBowlerId);
	}

	"Bowler of the Year Migrated".Dump();

	foreach (var womanOfTheYear in womanOfTheYears)
	{
		await MigrateBowlerOfTheYear(BowlerOfTheYearCategory.Woman, womanOfTheYear.year, womanOfTheYear.name, bowlerIdsByWebsiteName, bowlerIdsBySoftwareName, bowlerDbIdByBowlerId);
	}

	"Woman Bowler of the Year Migrated".Dump();

	foreach (var seniorOfTheYear in seniorOfTheYears)
	{
		await MigrateBowlerOfTheYear(BowlerOfTheYearCategory.Senior, seniorOfTheYear.year, seniorOfTheYear.name, bowlerIdsByWebsiteName, bowlerIdsBySoftwareName, bowlerDbIdByBowlerId);
	}

	"Senior Bowler of the Year Migrated".Dump();

	foreach (var superSeniorOfTheYear in superSeniorOfTheYears)
	{
		await MigrateBowlerOfTheYear(BowlerOfTheYearCategory.SuperSenior, superSeniorOfTheYear.year, superSeniorOfTheYear.name, bowlerIdsByWebsiteName, bowlerIdsBySoftwareName, bowlerDbIdByBowlerId);
	}

	"Super Senior Bowler of the Year Migrated".Dump();

	foreach (var rookieOfTheYear in rookieOfTheYears)
	{
		await MigrateBowlerOfTheYear(BowlerOfTheYearCategory.Rookie, rookieOfTheYear.year, rookieOfTheYear.name, bowlerIdsByWebsiteName, bowlerIdsBySoftwareName, bowlerDbIdByBowlerId);
	}

	"Rookie Bowler of the Year Migrated".Dump();

	foreach (var youthOfTheYear in youthOfTheYears)
	{
		await MigrateBowlerOfTheYear(BowlerOfTheYearCategory.Youth, youthOfTheYear.year, youthOfTheYear.name, bowlerIdsByWebsiteName, bowlerIdsBySoftwareName, bowlerDbIdByBowlerId);
	}

	"Youth Bowler of the Year Migrated".Dump();

	await SaveChangesAsync();
}

public async Task MigrateBowlerOfTheYear(BowlerOfTheYearCategory category, string year, string bowlerName,
	IReadOnlyCollection<KeyValuePair<HumanName, Ulid>> bowlerIdsByWebsiteName, IReadOnlyCollection<KeyValuePair<HumanName, Ulid>> bowlerIdsBySoftwareName,
	Dictionary<Ulid, int> bowlerDbIdByBowlerId)
{
	var websiteBowlers = bowlerIdsByWebsiteName.Where(b => bowlerName.Contains(b.Key.First, StringComparison.OrdinalIgnoreCase)
			&& bowlerName.Contains(b.Key.Last, StringComparison.OrdinalIgnoreCase));
	var softwareBowlers = bowlerIdsBySoftwareName.Where(b => bowlerName.Contains(b.Key.First, StringComparison.OrdinalIgnoreCase)
		&& bowlerName.Contains(b.Key.Last, StringComparison.OrdinalIgnoreCase));

	if (websiteBowlers.Any())
	{
		if (websiteBowlers.Count() > 1)
		{
			websiteBowlers.Dump($"Multiple Website People for {bowlerName}");
		}
		else
		{
			var bowler = websiteBowlers.Single();
			var record = new SeasonAwards
			{
				SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
				AwardType = SeasonAwardType.BowlerOfTheYear,
				BowlerDbId = bowlerDbIdByBowlerId[bowler.Value],
				BowlerOfTheYearCategory = category,
				Season = year.StartsWith("2020") ? "2020-2021" : year
			};

			SeasonAwards.Add(record);
		}
	}
	else if (softwareBowlers.Any())
	{
		if (softwareBowlers.Count() > 1)
		{
			softwareBowlers.Dump($"Multiple Software People for {bowlerName}");
		}
		else
		{
			var bowler = softwareBowlers.Single();
			var record = new SeasonAwards
			{
				SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
				AwardType = SeasonAwardType.BowlerOfTheYear,
				BowlerDbId = bowlerDbIdByBowlerId[bowler.Value],
				BowlerOfTheYearCategory = category,
				Season = year.StartsWith("2020") ? "2020-2021" : year
			};

			SeasonAwards.Add(record);
		}
	}
	else //Not on website or software
	{
		if (bowlerName.Contains("Brust") || bowlerName.Contains("Bissmann"))
		{
			var manualBowler = bowlerIdsBySoftwareName.Single(b => b.Key.Last == bowlerName.Split(' ')[1]);

			var manualRecord = new SeasonAwards
			{
				SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
				AwardType = SeasonAwardType.BowlerOfTheYear,
				BowlerDbId = bowlerDbIdByBowlerId[manualBowler.Value],
				BowlerOfTheYearCategory = category,
				Season = year
			};

			SeasonAwards.Add(manualRecord);

			return;
		}

		$"------ {bowlerName} is not a champion nor in the software, creating new ------".Dump();

		var newBowlerName = new HumanName(bowlerName);
		var newBowler = new Bowlers
		{
			BowlerId = new Ulid(Guid.NewGuid()).ToString(),
			FirstName = newBowlerName.First,
			MiddleName = string.IsNullOrWhiteSpace(newBowlerName.Middle) ? null : newBowlerName.Middle,
			LastName = newBowlerName.Last,
			Suffix = string.IsNullOrWhiteSpace(newBowlerName.Suffix) ? null : newBowlerName.Suffix.Replace(".", ""),
			Nickname = string.IsNullOrWhiteSpace(newBowlerName.Nickname) ? null : newBowlerName.Nickname,
			ApplicationId = null,
			WebsiteId = null
		};

		Bowlers.Add(newBowler);
		
		await SaveChangesAsync();
		
		bowlerDbIdByBowlerId.Add(Ulid.Parse(newBowler.BowlerId), newBowler.DbId);

		var record = new SeasonAwards
		{
			SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
			AwardType = SeasonAwardType.BowlerOfTheYear,
			BowlerDbId = newBowler.DbId,
			BowlerOfTheYearCategory = category,
			Season = year.StartsWith("2020") ? "2020-2021" : year
		};

		SeasonAwards.Add(record);
	}
}


public async Task MigrateHighBlockAsync(
	IReadOnlyCollection<KeyValuePair<HumanName, Ulid>> bowlerIdsByWebsiteName,
	IReadOnlyCollection<KeyValuePair<HumanName, Ulid>> bowlerIdsBySoftwareName,
	Dictionary<Ulid, int> bowlerDbIdByBowlerId)
{
	var highBlocks = await HighBlockScraper.ScrapeAsync(@"https://www.bowlneba.com/history/high-block/");

	foreach (var highBlock in highBlocks)
	{
		var websiteBowlerMatches = bowlerIdsByWebsiteName.Where(b => highBlock.name.Contains(b.Key.First, StringComparison.OrdinalIgnoreCase)
			&& highBlock.name.Contains(b.Key.Last, StringComparison.OrdinalIgnoreCase));
		var softwareBowlerMatches = bowlerIdsBySoftwareName.Where(b => highBlock.name.Contains(b.Key.First, StringComparison.OrdinalIgnoreCase)
			&& highBlock.name.Contains(b.Key.Last, StringComparison.OrdinalIgnoreCase));

		if (websiteBowlerMatches.Any())
		{
			if (websiteBowlerMatches.Count() > 1)
			{
				if (highBlock.name == "Steve Hardy")
				{
					SeasonAwards.Add(new()
					{
						SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
						AwardType = SeasonAwardType.High5GameBlock,
						BowlerDbId = bowlerDbIdByBowlerId[bowlerIdsByWebsiteName.Single(b => b.Key.FullName.Trim().Equals("Steve Hardy", StringComparison.OrdinalIgnoreCase)).Value],
						Season = highBlock.year,
						HighBlockScore = highBlock.score
					});

					continue;
				}
				else if (highBlock.name == "Mark Blanchette")
				{
					SeasonAwards.Add(new()
					{
						SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
						AwardType = SeasonAwardType.High5GameBlock,
						BowlerDbId = bowlerDbIdByBowlerId[bowlerIdsByWebsiteName.Single(b => b.Key.FullName.Trim().Equals("Mark Blanchette", StringComparison.OrdinalIgnoreCase)).Value],
						Season = highBlock.year,
						HighBlockScore = highBlock.score
					});

					continue;
				}
				
				websiteBowlerMatches.Dump($"Multiple Website People for {highBlock.name}");
			}
			else
			{
				var bowler = websiteBowlerMatches.Single();
				var record = new SeasonAwards
				{
					SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
					AwardType = SeasonAwardType.High5GameBlock,
					BowlerDbId = bowlerDbIdByBowlerId[bowler.Value],
					Season = highBlock.year.StartsWith("2020") ? "2020-2021" : highBlock.year,
					HighBlockScore = highBlock.score
				};

				SeasonAwards.Add(record);
			}
		}
		else if (softwareBowlerMatches.Any())
		{	
			if (softwareBowlerMatches.Count() > 1)
			{
				softwareBowlerMatches.Dump($"Multiple Software People for {highBlock.name}");
			}
			else
			{
				var bowler = softwareBowlerMatches.Single();
				var record = new SeasonAwards
				{
					SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
					AwardType = SeasonAwardType.High5GameBlock,
					BowlerDbId = bowlerDbIdByBowlerId[bowler.Value],
					Season = highBlock.year.StartsWith("2020") ? "2020-2021" : highBlock.year,
					HighBlockScore = highBlock.score
				};

				SeasonAwards.Add(record);
			}
		}
		else //Not on website or software
		{
			if (highBlock.name.Contains("Michaue")) //typo on website
			{
				var bowlerId = bowlerIdsBySoftwareName.Single(b => b.Key.First == "Russ" && b.Key.Last == "Michaud").Value;
				
				SeasonAwards.Add(new()
				{
					SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
					AwardType = SeasonAwardType.High5GameBlock,
					BowlerDbId = bowlerDbIdByBowlerId[bowlerId],
					Season = highBlock.year,
					HighBlockScore = highBlock.score
				});	
				
				continue;
			}
			
			$"------ {highBlock.name} is not a champion nor in the software, creating new ------".Dump();

			var newBowlerName = new HumanName(highBlock.name);
			var newBowler = new Bowlers
			{
				BowlerId = new Ulid(Guid.NewGuid()).ToString(),
				FirstName = newBowlerName.First,
				MiddleName = string.IsNullOrWhiteSpace(newBowlerName.Middle) ? null : newBowlerName.Middle,
				LastName = newBowlerName.Last,
				Suffix = string.IsNullOrWhiteSpace(newBowlerName.Suffix) ? null : newBowlerName.Suffix.Replace(".", ""),
				Nickname = string.IsNullOrWhiteSpace(newBowlerName.Nickname) ? null : newBowlerName.Nickname,
				ApplicationId = null,
				WebsiteId = null
			};

			Bowlers.Add(newBowler);
			
			await SaveChangesAsync();
			
			bowlerDbIdByBowlerId.Add(Ulid.Parse(newBowler.BowlerId), newBowler.DbId);

			var record = new SeasonAwards
			{
				SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
				AwardType = SeasonAwardType.High5GameBlock,
				BowlerDbId = newBowler.DbId,
				Season = highBlock.year.StartsWith("2020") ? "2020-2021" : highBlock.year,
				HighBlockScore = highBlock.score
			};

			SeasonAwards.Add(record);
		}
	}
	
	await SaveChangesAsync();

	"High Block Migrated".Dump();
}

public async Task MigrateHighAverageAsync(
	IReadOnlyCollection<KeyValuePair<HumanName, Ulid>> bowlerIdsByWebsiteName,
	IReadOnlyCollection<KeyValuePair<HumanName, Ulid>> bowlerIdsBySoftwareName,
	Dictionary<Ulid, int> bowlerDbIdByBowlerId)
{
	var highAverages = await HighAverageScraper.ScrapeAsync(@"https://www.bowlneba.com/history/high-average/");

	foreach (var highAverage in highAverages)
	{
		var websiteBowlerMatches = bowlerIdsByWebsiteName.Where(b => highAverage.name.Contains(b.Key.First, StringComparison.OrdinalIgnoreCase)
			&& highAverage.name.Contains(b.Key.Last, StringComparison.OrdinalIgnoreCase));
		var softwareBowlerMatches = bowlerIdsBySoftwareName.Where(b => highAverage.name.Contains(b.Key.First, StringComparison.OrdinalIgnoreCase)
			&& highAverage.name.Contains(b.Key.Last, StringComparison.OrdinalIgnoreCase));

		if (websiteBowlerMatches.Any())
		{
			if (websiteBowlerMatches.Count() > 1)
			{
				websiteBowlerMatches.Dump($"Multiple Website People for {highAverage.name}");
			}
			else
			{
				var bowler = websiteBowlerMatches.Single();
				var record = new SeasonAwards
				{
					SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
					AwardType = SeasonAwardType.HighAverage,
					BowlerDbId = bowlerDbIdByBowlerId[bowler.Value],
					Season = highAverage.year.StartsWith("2020") ? "2020-2021" : highAverage.year,
					Average = highAverage.average,
					SeasonTotalGames = highAverage.games,
					Tournaments = highAverage.tournaments
				};

				SeasonAwards.Add(record);
			}
		}
		else if (softwareBowlerMatches.Any())
		{
			if (softwareBowlerMatches.Count() > 1)
			{
				softwareBowlerMatches.Dump($"Multiple Software People for {highAverage.name}");
			}
			else
			{
				var bowler = softwareBowlerMatches.Single();
				var record = new SeasonAwards
				{
					SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
					AwardType = SeasonAwardType.HighAverage,
					BowlerDbId = bowlerDbIdByBowlerId[bowler.Value],
					Season = highAverage.year.StartsWith("2020") ? "2020-2021" : highAverage.year,
					Average = highAverage.average,
					SeasonTotalGames = highAverage.games,
					Tournaments = highAverage.tournaments
				};

				SeasonAwards.Add(record);
			}
		}
		else //Not on website or software
		{
			$"------ {highAverage.name} is not a champion nor in the software, creating new ------".Dump();

			var newBowlerName = new HumanName(highAverage.name);
			var newBowler = new Bowlers
			{
				BowlerId = new Ulid(Guid.NewGuid()).ToString(),
				FirstName = newBowlerName.First,
				MiddleName = string.IsNullOrWhiteSpace(newBowlerName.Middle) ? null : newBowlerName.Middle,
				LastName = newBowlerName.Last,
				Suffix = string.IsNullOrWhiteSpace(newBowlerName.Suffix) ? null : newBowlerName.Suffix.Replace(".", ""),
				Nickname = string.IsNullOrWhiteSpace(newBowlerName.Nickname) ? null : newBowlerName.Nickname,
				ApplicationId = null,
				WebsiteId = null
			};

			Bowlers.Add(newBowler);
			
			await SaveChangesAsync();
			
			bowlerDbIdByBowlerId.Add(Ulid.Parse(newBowler.BowlerId), newBowler.DbId);

			var record = new SeasonAwards
			{
				SeasonAwardId = new Ulid(Guid.NewGuid()).ToString(),
				AwardType = SeasonAwardType.HighAverage,
				BowlerDbId = newBowler.DbId,
				Season = highAverage.year.StartsWith("2020") ? "2020-2021" : highAverage.year,
				Average = highAverage.average,
				SeasonTotalGames = highAverage.games,
				Tournaments = highAverage.tournaments
			};

			SeasonAwards.Add(record);
		}
	}

	await SaveChangesAsync();

	"High Average Migrated".Dump();
}

#region Enumerations

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
	public static readonly TournamentType Singles = new("Singles", 10, 1, 13);

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
	public static readonly TournamentType NonChampions = new("Non-Champs", 11, 1, 8);

	/// <summary>
	/// Tournament of Champions event.
	/// </summary>
	public static readonly TournamentType TournamentOfChampions = new("T of C", 12, 1, 7);

	/// <summary>
	/// Invitational tournament.
	/// </summary>
	public static readonly TournamentType Invitational = new("Invitational", 13, 1, 11);

	/// <summary>
	/// Masters tournament.
	/// </summary>
	public static readonly TournamentType Masters = new("Masters", 14, 1, 12);

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
	public static readonly TournamentType Women = new("Women's", 17, 1, 15);

	/// <summary>
	/// Over 40 tournament.
	/// </summary>
	public static readonly TournamentType OverForty = new("Over 40", 18, 1, 6);

	/// <summary>
	/// 40-49 age group tournament.
	/// </summary>
	public static readonly TournamentType FortyToFortyNine = new("40 - 49", 19, 1, 5);

	/// <summary>
	/// Over/Under 50 Doubles tournament (2 players per team).
	/// </summary>
	public static readonly TournamentType OverUnderFiftyDoubles = new("Over/Under 50 Doubles", 21, 2, 14);

	/// <summary>
	/// Over/Under 40 Doubles tournament (2 players per team).
	/// </summary>
	public static readonly TournamentType OverUnderFortyDoubles = new("Under/Over 40 Doubles", 22, 2, 9);

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


/// <summary>
/// Represents a category for the Bowler of the Year award, using a smart enum pattern.
/// </summary>
public sealed class BowlerOfTheYearCategory
	: SmartEnum<BowlerOfTheYearCategory>
{
	/// <summary>
	/// Open category for Bowler of the Year.
	/// </summary>
	public static readonly BowlerOfTheYearCategory Open = new("Bowler of the Year", 1);

	/// <summary>
	/// Woman category for Bowler of the Year.
	/// </summary>
	public static readonly BowlerOfTheYearCategory Woman = new("Woman Bowler of the Year", 2);

	/// <summary>
	/// Senior category for Bowler of the Year.
	/// </summary>
	public static readonly BowlerOfTheYearCategory Senior = new("Senior Bowler of the Year", 50);

	/// <summary>
	/// Super Senior category for Bowler of the Year.
	/// </summary>
	public static readonly BowlerOfTheYearCategory SuperSenior = new("Super Senior Bowler of the Year", 60);

	/// <summary>
	/// Rookie of the Year category.
	/// </summary>
	public static readonly BowlerOfTheYearCategory Rookie = new("Rookie of the Year", 10);

	/// <summary>
	/// Youth category for Bowler of the Year.
	/// </summary>
	public static readonly BowlerOfTheYearCategory Youth = new("Youth Bowler of the Year", 20);

	/// <summary>
	/// Initializes a new instance of the <see cref="BowlerOfTheYearCategory"/> class with the specified name and value.
	/// </summary>
	/// <param name="name">The name of the category.</param>
	/// <param name="value">The integer value of the category.</param>
	private BowlerOfTheYearCategory(string name, int value)
		: base(name, value)
	{ }

	/// <summary>
	/// Private parameterless constructor for serialization or ORM support.
	/// </summary>
	private BowlerOfTheYearCategory()
		: this(string.Empty, 0)
	{ }
}

/// <summary>
/// Represents the types of season awards that can be given in NEBA, using a SmartEnum pattern for extensibility.
/// </summary>
public sealed class SeasonAwardType
	: SmartEnum<SeasonAwardType>
{
	internal static readonly SeasonAwardType s_default = new("Default", 0);

	/// <summary>
	/// Awarded to the bowler with the best overall performance during the season.
	/// </summary>
	public static readonly SeasonAwardType BowlerOfTheYear = new("Bowler of the Year", 1);

	/// <summary>
	/// Awarded for achieving the highest average during the season.
	/// </summary>
	public static readonly SeasonAwardType HighAverage = new("High Average", 2);

	/// <summary>
	/// Awarded for the highest 5-game block score in a single event or season.
	/// </summary>
	public static readonly SeasonAwardType High5GameBlock = new("High 5-Game Block", 3);

	/// <summary>
	/// Initializes a new instance of the <see cref="SeasonAwardType"/> class with the specified name and value.
	/// </summary>
	/// <param name="name">The display name of the award type.</param>
	/// <param name="value">The unique value of the award type.</param>
	private SeasonAwardType(string name, int value)
		: base(name, value)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="SeasonAwardType"/> class with default values.
	/// </summary>
	private SeasonAwardType()
		: this(string.Empty, 0)
	{ }
}

#endregion

#region Database Queries

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


#endregion

#region Website Scrapers

public static class WebScraperHelper
{
	public static async Task<HtmlDocument> FetchHtmlDocumentAsync(string url)
	{
		using var httpClient = new HttpClient();
		var html = await httpClient.GetStringAsync(url);

		var htmlDoc = new HtmlDocument();
		htmlDoc.LoadHtml(html);

		return htmlDoc;
	}
}

public static class BowlerOfTheYearScraper
{
	public static async Task<List<(string year, string name)>> ScrapeAsync(string url)
	{
		var results = new List<(string year, string name)>();

		var htmlDoc = await WebScraperHelper.FetchHtmlDocumentAsync(url);

		// Pattern to match year entries like "1963", "2020/21", etc.
		var yearPattern = new Regex(@"^(\d{4}(?:/\d{2})?)\s*[:.]?\s*(.+)$");

		// Collect all text nodes and parse them
		var allText = htmlDoc.DocumentNode.InnerText;

		// Decode HTML entities like &nbsp; and &copy;
		allText = System.Web.HttpUtility.HtmlDecode(allText);

		var lines = allText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

		foreach (var line in lines)
		{
			var trimmed = line.Trim();

			// Try to match year pattern
			var match = yearPattern.Match(trimmed);
			if (match.Success)
			{
				var year = match.Groups[1].Value;
				var name = match.Groups[2].Value.Trim();

				// Filter out invalid entries
				if (!string.IsNullOrWhiteSpace(name) && name.Length > 2)
				{
					// Skip entries that look like copyright, links, or other non-name content
					if (name.Contains("©") ||
						name.Contains("All Rights Reserved") ||
						name.Contains("Copyright") ||
						name.StartsWith("http", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					// Handle ties - split on " and " to get multiple names
					AddNamesFromEntry(results, year, name);
				}
			}
			else
			{
				// Alternative parsing: look for strong tags followed by text
				// Check if line starts with a 4-digit year
				if (Regex.IsMatch(trimmed, @"^\d{4}"))
				{
					var parts = trimmed.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length >= 2)
					{
						var year = parts[0].TrimEnd(':', '.');
						var name = parts[1].Trim();

						// Filter out invalid entries
						if (name.Contains("©") ||
							name.Contains("All Rights Reserved") ||
							name.Contains("Copyright"))
						{
							continue;
						}

						// Handle ties - split on " and " to get multiple names
						AddNamesFromEntry(results, year, name);
					}
				}
			}
		}

		// If the above didn't work well, try a more specific approach
		if (results.Count < 10)
		{
			results.Clear();

			// Look for strong tags containing years
			var strongNodes = htmlDoc.DocumentNode.SelectNodes("//strong");
			if (strongNodes != null)
			{
				foreach (var strongNode in strongNodes)
				{
					var yearText = System.Web.HttpUtility.HtmlDecode(strongNode.InnerText.Trim().TrimEnd(':', '.'));

					// Check if it's a year (4 digits or year range like 2020/21)
					if (Regex.IsMatch(yearText, @"^\d{4}(?:/\d{2})?$"))
					{
						// Get the next text node (the name)
						var nextNode = strongNode.NextSibling;
						while (nextNode != null && string.IsNullOrWhiteSpace(nextNode.InnerText))
						{
							nextNode = nextNode.NextSibling;
						}

						if (nextNode != null)
						{
							var name = System.Web.HttpUtility.HtmlDecode(nextNode.InnerText.Trim());
							// Clean up the name (remove leading colons, spaces, etc.)
							name = Regex.Replace(name, @"^[:\s]+", "").Trim();

							// Take only the first line if there are multiple lines
							var firstLine = name.Split('\n')[0].Trim();

							// Filter out invalid entries
							if (!string.IsNullOrWhiteSpace(firstLine) &&
								firstLine.Length > 2 &&
								!firstLine.Contains("©") &&
								!firstLine.Contains("All Rights Reserved") &&
								!firstLine.Contains("Copyright"))
							{
								// Handle ties - split on " and " to get multiple names
								AddNamesFromEntry(results, yearText, firstLine);
							}
						}
					}
				}
			}
		}

		return results.OrderBy(r => r.year).ToList();
	}

	private static void AddNamesFromEntry(List<(string year, string name)> results, string year, string nameEntry)
	{
		// Check if there are multiple names separated by " and "
		if (nameEntry.Contains(" and ", StringComparison.OrdinalIgnoreCase))
		{
			// Split on " and " (case-insensitive)
			var names = Regex.Split(nameEntry, @"\s+and\s+", RegexOptions.IgnoreCase);

			foreach (var name in names)
			{
				var trimmedName = name.Trim();
				if (!string.IsNullOrWhiteSpace(trimmedName) && !results.Any(r => r.year == year && r.name == trimmedName))
				{
					results.Add((year, trimmedName));
				}
			}
		}
		else
		{
			// Single name entry
			if (!results.Any(r => r.year == year && r.name == nameEntry))
			{
				results.Add((year, nameEntry));
			}
		}
	}
}

public static class HighBlockScraper
{
	public static async Task<List<(string year, string name, int score)>> ScrapeAsync(string url)
	{
		var results = new List<(string year, string name, int score)>();

		var htmlDoc = await WebScraperHelper.FetchHtmlDocumentAsync(url);

		// Get all text content
		var allText = htmlDoc.DocumentNode.InnerText;

		// Decode HTML entities like &nbsp; and &copy;
		allText = System.Web.HttpUtility.HtmlDecode(allText);

		var lines = allText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
			.Select(l => l.Trim())
			.Where(l => !string.IsNullOrWhiteSpace(l))
			.ToList();

		// Pattern to match a 4-digit year (or year range like 2020/21)
		var yearPattern = new Regex(@"^\d{4}(?:/\d{2})?$");
		var notAwardedPattern = new Regex(@"^Not\s+awarded$", RegexOptions.IgnoreCase);
		var scorePattern = new Regex(@"^\d{3,4}\*?$"); // Score with optional asterisk

		for (int i = 0; i < lines.Count - 2; i++)
		{
			var currentLine = lines[i];

			// Check if current line is a year
			if (yearPattern.IsMatch(currentLine))
			{
				var year = currentLine;
				var nextLine = lines[i + 1];

				// Check if next line is "Not awarded"
				if (notAwardedPattern.IsMatch(nextLine))
				{
					continue; // Skip this entry
				}

				// Next line should be the name, and the line after should be the score
				var nameEntry = nextLine;

				// Make sure we have a line after the name
				if (i + 2 < lines.Count)
				{
					var potentialScore = lines[i + 2];

					// Check if it looks like a score
					if (scorePattern.IsMatch(potentialScore))
					{
						// Remove asterisks or special characters from the score
						var scoreText = potentialScore.Replace("*", "").Trim();

						if (int.TryParse(scoreText, out int score))
						{
							// Check if there are multiple names separated by commas (ties)
							if (nameEntry.Contains(","))
							{
								// Split on comma to get multiple names
								var names = nameEntry.Split(',')
									.Select(n => n.Trim())
									.Where(n => !string.IsNullOrWhiteSpace(n) && n.Length > 2)
									.ToList();

								foreach (var name in names)
								{
									// Skip entries that look like copyright, links, or other non-name content
									if (!name.Contains("©") &&
										!name.Contains("All Rights Reserved") &&
										!name.Contains("Copyright") &&
										!name.StartsWith("http", StringComparison.OrdinalIgnoreCase))
									{
										results.Add((year, name, score));
									}
								}
							}
							else
							{
								// Single name entry
								var name = nameEntry;

								// Filter out invalid entries
								if (!string.IsNullOrWhiteSpace(name) && name.Length > 2)
								{
									// Skip entries that look like copyright, links, or other non-name content
									if (name.Contains("©") ||
										name.Contains("All Rights Reserved") ||
										name.Contains("Copyright") ||
										name.StartsWith("http", StringComparison.OrdinalIgnoreCase))
									{
										continue;
									}

									results.Add((year, name, score));
								}
							}
						}
					}
				}
			}
		}

		return results.OrderBy(r => r.year).ToList();
	}
}

public static class HighAverageScraper
{
	public static async Task<List<(string year, string name, decimal average, int? games, int? tournaments)>> ScrapeAsync(string url)
	{
		var results = new List<(string year, string name, decimal average, int? games, int? tournaments)>();

		var htmlDoc = await WebScraperHelper.FetchHtmlDocumentAsync(url);

		// Get all text content
		var allText = htmlDoc.DocumentNode.InnerText;

		// Decode HTML entities like &nbsp; and &copy;
		allText = System.Web.HttpUtility.HtmlDecode(allText);

		var lines = allText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
			.Select(l => l.Trim())
			.Where(l => !string.IsNullOrWhiteSpace(l))
			.ToList();

		// Pattern to match a 4-digit year (or year range like 2020/21)
		var yearPattern = new Regex(@"^\d{4}(?:/\d{2})?$");
		var notAwardedPattern = new Regex(@"^Not\s+awarded$", RegexOptions.IgnoreCase);

		for (int i = 0; i < lines.Count - 1; i++)
		{
			var currentLine = lines[i];

			// Check if current line is a year
			if (yearPattern.IsMatch(currentLine))
			{
				var year = currentLine;
				var nextLine = lines[i + 1];

				// Check if next line is "Not awarded"
				if (notAwardedPattern.IsMatch(nextLine))
				{
					continue; // Skip this entry
				}

				// Next line should be the name
				var nameEntry = nextLine;

				// Filter out invalid entries
				if (string.IsNullOrWhiteSpace(nameEntry) || nameEntry.Length <= 2)
				{
					continue;
				}

				// Skip entries that look like copyright, links, or other non-name content
				if (nameEntry.Contains("©") ||
					nameEntry.Contains("All Rights Reserved") ||
					nameEntry.Contains("Copyright") ||
					nameEntry.StartsWith("http", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				// Look for average (decimal number, possibly with asterisk), games, and tournaments
				decimal? average = null;
				int? games = null;
				int? tournaments = null;

				// Check the next few lines for average, games, and tournaments
				// We need to stop before hitting the next year
				for (int j = i + 2; j < Math.Min(i + 6, lines.Count); j++)
				{
					var potentialValue = lines[j].Replace("*", "").Trim();

					// Stop if we hit another year
					if (yearPattern.IsMatch(potentialValue))
					{
						break;
					}

					// Try to parse as decimal for average
					if (average == null && decimal.TryParse(potentialValue, out decimal avgValue))
					{
						average = avgValue;
					}
					// Try to parse as integer for games or tournaments (in that order)
					else if (int.TryParse(potentialValue, out int intValue))
					{
						if (games == null)
						{
							games = intValue;
						}
						else if (tournaments == null)
						{
							tournaments = intValue;
							break; // We have all the data we need
						}
					}
				}

				// Average is required, so only add if we have it
				if (average.HasValue)
				{
					results.Add((year, nameEntry, average.Value, games, tournaments));
				}
			}
		}

		return results.OrderBy(r => r.year).ToList();
	}
}

#endregion

#region Manual Bowler Match

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
	new(null, 4738),  // Cory Martin
	new(519, 658),	// Zac Gentile
	new(null, 2900),  // David Anton
	new(457, 701),	// Dave Paquin Jr
	new(null, 1914),  // David Travers
	new(17, 943), 	// Stephen Dale Jr
	new(null, 4634),  // Melissa Smith
	new(83, 1022),	// Orville Gordon
	new(525, 1111),   // Mike E Rose Jr (1126 is Mike P Rose Jr)
	new(null, 3263),  // Willie Clark
	new(null, 3276),  // Anne Connor
	new(402, 1236),   // Thomas Coco Jr
	new(null, 4085),  // Joseph G Williams
	new(null, 3775),  // Richard Raymond II
	new(null, 4631),  // Tyler Smith
	new(null, 4097),  // Mandi Fournier
	new(120, 1282),   // Jeff Voght
	new(433, 1284),   // Ken Lefebvre
	new(107, 1372),   // Jimmie Pritts Jr (1029 is Jimmie Pritts Sr and needs to be deleted)
	new(null, 4452),  // Jeffry Johnson
	new(335, 1398),   // Peter Valenti, Jr
	new(null, 4489),  // Jamie Williams
	new(380, 1406),   // Joshua Corum
	new(3, 1628),	 // Christine Rebstock
	new(null, 1209),  // Jodi Arruda
	new(155, 1774),   // Steve Brown Jr
	new(396, 1861),   // Bob Greene (Fairfield)
	new(null, 93),	// Robert Greene
	new(null, 1571),  // Stephanie O Millward
	new(null, 2786),  // Stephen Transue
	new(98, 642),     // Rick Mochrie Sr
	new(109, 2072),   // Timothy Riordan
	new(157, 2239),   // Ryan Burlone Sr
	new(null, 2927),  // Chris Cote
	new(184, 2305),   // Patrick Donohoe Jr
	new(null, 4053),  // Imani Williams
	new(null, 2614),  // Jeremy Koziol
	new(null, 1749),  // Tyler Scott
	new(null, 4623),  // Chris Roberts
	new(165, 2445),   // Douglas Carlson
	new(437, 2449),   // Sammy Ventura
	new(null, 4427),  // Mick Perrone
	new(null, 3478),  // Al Green
	new(null, 4273),  // Amber Wood
	new(null, 4080),  // Samuel Blanchette
	new(476, 2910),   // Jim Sicard
	new(null, 2861),  // Mark White
	new(null, 2668),  // Jeff Paternostro
	new(null, 2879),  // Roger Ferguson
	new(null, 2976),  // Debbie Valenti
	new(null, 1228),  // Phillip Scott
	new(null, 3317),  // Justin Kampf
	new(null, 3618),  // Rick Wilbur
	new(null, 3164),  // Mark Zimmerman
	new(null, 3901),  // Keith Wiggins
	new(null, 1563),  // Kevin Smith
	new(null, 751),   // Paul Silva
	new(null, 1993),  // Robert Travers
	new(null, 3862),  // William E Williams
	new(null, 4643),  // Christopher Williams
	new(null, 4616),  // Diane Allen
	new(null, 2780),  // Nicholas Jenkins
	new(460, 2783),   // Mallory Clark (Nutting)
	new(null, 3250),  // James Roberts
	new(null, 2827),  // Nick Demaine
	new(null, 4375),  // Skylar Smith
	new(null, 4298),  // Jason Johnson
	new(null, 3718),  // Alex White
	new(null, 3256),  // Tyler Brooks
	new(null, 4699),  // Theo Johnson
	new(null, 3863),  // Dominik Blanchet
	new(null, 2758),  // Amy Viale
	new(null, 4906),  // Zach Scott
	new(null, 2216),  // Chris Sprague
	new(null, 2891),  // Bill Kempton
	new(null, 3707),  // Bryan Novaco
	new(null, 3548),  // Ken Bennett
	new(null, 3428),  // Derick Thibeault
	new(null, 4715),  // Raymond Oliver
	new(null, 2800),  // Kenny Martin
	new(null, 1510),  // Chris Smith
	new(null, 4447),  // Jeff Sprague
	new(null, 3715),  // Pete Williams
	new(null, 3423),  // Dean Jones
	new(null, 2784),  // Alan Oliver
	new(397, 3203),   // Johnny Petraglia Jr
	new(null, 3693),  // Andy Smith
	new(111, 3339),   // Brentt Smith
	new(336, 3384),   // Jon van Hees
	new(360, 3440),   // Dan Gauthier
	new(null, 4619),  // Troy Bouchard
	new(null, 3952),  // Gervais Edwards
	new(386, 339),    // Matt Brockett
	new(null, 3209),  // Chris Perrone
	new(null, 2863),  // Edward Williams
	new(null, 3699),  // Kyle Egan
	new(null, 2003),  // Jeremy Smith
	new(null, 3190),  // Kendall Roberts
	new(null, 3726),  // Anne Connor
	new(null, 4180),  // Spencer Collins
	new(null, 4458),  // Robert Connor
	new(null, 2504),  // Darryn Martin
	new(null, 1892),  // Ian Williams
	new(245, 4170),   // Jeff Lemon
	new(147, 4226),   // Billy Black
	new(415, 290),	// Jayme Silva Sr
	new(null, 3385),  // Mike Perrone
	new(null, 2438),  // Jimmy Williams
	new(null, 2660),  // Justin Williams
	new(null, 4324),  // Jonathan Edwards
	new(null, 1868),  // Rodney Rapoza
	new(null, 601),   // Russ Sprague
	new(null, 3062),  // Nick Roberts
	new(null, 3945),  // Damion Ferraro
	new(null, 4750),  // Jonathon Durand
	new(null, 3930),  // Joseph Williams, Jr
	new(null, 4369),  // Gregory Bourque
	new(null, 4395),  // Octavia Hall
	new(null, 3187),  // Jason Cornog
	new(null, 3879),  // Marvin Clark
	new(null, 4794),  // Jean-pierre Cote
	new(null, 4869),  // Jason Lopes
	new(null, 2023),  // Mike Talmadge
	new(null, 4744),  // Casey Kearney
	new(null, 4976),  // Wyatt Smith
	new(null, 4130),  // Michelle Grexer
	new(null, 595),   // Chris Silva
	new(null, 4973),  // Daryl Smith
	new(null, 2253),  // John Ferguson
	new(null, 4393),  // Nancy Cote
	new(null, 3279),  // Michael Conroy
	new(null, 4810),  // Zachery Demello
	new(null, 2723),  // Robert King
	new(null, 2174),  // Kevin Fournier
	new(null, 2651),  // Michael Allen
	new(null, 3398),  // Gerry Fournier
	new(null, 1197),  // Zachery Campbell
	new(null, 4617),  // Timmy Smith
	new(null, 2370),  // Jerry Smith
	new(null, 2282),  // Dan Smith
	new(null, 4846),  // Jocelyn Smith
	new(null, 3493),  // Mike Wilbur
	new(null, 1733),  // Glenn Smith
	new(null, 3068),  // Ken Corkhum
	new(null, 1862),  // Tyler Blanchet
	new(null, 3759),  // Thomas King
	new(null, 43),	// Brian Smith
	new(null, 2057),  // Jeffrey Smith
	new(null, 67),	// Christopher Brown
	new(null, 77),	// Christopher Baker
	new(null, 3988),  // Jadee Scott-jones
	new(null, 4236),  // Tristan Erickson
	new(null, 1343),  // James Martin
	new(null, 124),   // Jason Brown
	new(null, 3929),  // Nicholas Williams
	new(null, 4192),  // Will Smith
	new(null, 135),   // Alex Major
	new(null, 138),   // Fred Trudell
	new(null, 4697),  // Keith Martin
	new(null, 2383),  // Forrest Williams
	new(null, 2036),  // Mike Williams
	new(null, 4695),  // Andre Borges
	new(null, 3793),  // Chris Green
	new(null, 667),   // Clayton Jenkins
	new(null, 2708),  // Al Williams
	new(null, 144),   // Terry Robinson
	new(null, 193),   // Shirley Major
	new(null, 2213),  // Rick Campbell
	new(null, 2756),  // Richard Dube
	new(null, 3592),  // Mindy Hardy
	new(null, 259),   // Andrew Broege
	new(null, 4741),  // Joshua Jones
	new(null, 278),   // Jayme Silva Jr
	new(null, 302),   // David Collins
	new(null, 310),   // Clint Jones
	new(null, 337),   // Scott Hall
	new(null, 1225),  // Scott Johnson
	new(null, 986),   // Steven Amaral
	new(null, 5001),  // William F Johnson
	new(null, 4601),  // Corey Major
	new(null, 3382),  // William Clark
	new(null, 1580),  // Chris Williams
	new(null, 3692),  // Chris Smith
	new(null, 1995),  // Derek Thibeault
	new(null, 3230),  // James Roberts
	new(null, 2833),  // Craig Amaral
	new(null, 2238),  // Anthony Allen
	new(null, 4504),  // Mishawn Williams
	new(null, 2035),  // William Robertson
	new(null, 3205),  // Ben Dube
	new(null, 4924),  // Ryan Hoesterey
	new(null, 3270),  // Don Silva
	new(null, 3926),  // Brian Smith
	new(null, 4438),  // Frederick Green
	new(null, 3892),  // Zach Smith
	new(null, 3143),  // Jaime Smith
	new(null, 2759),  // Mark Strong
	new(null, 2309),  // Joanne Johnson
	new(null, 4399),  // Alex King
	new(null, 240),   // Anthony Green
	new(null, 1777),  // Mark Johnson
	new(null, 4238),  // Kyle Haines
	new(null, 2007),  // Joshua Roberts
	new(null, 2345),  // Adam Michaud
	new(null, 4194),  // Michael Perrone,
	new(null, 3299),  // Adam Amaral
	new(null, 1378),  // Sheila Allen
	new(null, 4767),  // Hunter J Lopes
	new(null, 2975),  // Greg Green
	new(null, 4293),  // Joe Johnson
	new(null, 3038),  // Timothy Scott
	new(null, 346),   // Nick Major
	new(null, 4268),  // Michael Smith
	new(null, 3976),  // Casey Smith
	new(null, 1888),  // William Razor
	new(null, 3094),  // Shawn Martin
	new(null, 2093),  // Matthew Fredette
	new(null, 4400),  // Robert E Smith
	new(null, 740),   // Jack Kampf
	new(null, 4725),  // Mathis Blanchette
	new(null, 4390),  // Connor Egan
	new(null, 3601),  // Natasha Fazzone
	new(null, 2561),  // Tracy van Hees
	new(null, 1323),  // Edgar Johnson
	new(null, 4298),  // Jason Johnson
	new(null, 4703),  // Jacob Dunbar
	new(null, 2324),  // Nathan Clark
	new(null, 4822),  // Matthew Dupuis
	new(null, 3558),  // Joely O'grady
	new(null, 4272),  // Shawn D Wood
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
	new(null, 1008),  // Robert Dube
	new(null, 1296),  // Paul Arruda
	new(null, 3679),  // Ricky Smith
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
	new(null, 2374),  // Kevin Williams
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
	new(null, 4290),  // Nicholas Martin
	new(null, 2517),  // Scott Bourget
	new(null, 730),   // Greg Rogers
	new(411, 763),    // Stephen Puishys
	new(null, 784),   // Craig Coplan
	new(null, 809),   // John-david Edwards
	new(null, 3722),  // Daryl Wood
	new(null, 2163),  // Rick Johnson
	new(null, 4102),  // Michael Silva
	new(null, 2328),  // John Smith
	new(null, 2329),  // Harry Thibeault
	new(null, 4451),  // Brandon Collins
	new(null, 2667),  // Robert Thibeault
	new(null, 1425),  // Kyra Smith
	new(null, 4595),  // Andrew Smith
	new(null, 3907),  // David Raymond
	new(null, 4115),  // Nicholas Smith
	new(null, 3868),  // Lawanda Scott
	new(null, 1238),  // Philip Scot
	new(null, 2377),  // Paul Cote
	new(null, 814),   // Don Fournier
	new(null, 4054),  // Darnell Williams
	new(null, 817),   // Greg White
	new(null, 4128),  // Christopher Williams
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
	new(null, 4486),  // Keven Green
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
	new(null, 4819),  // Claire Smith
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
	new(null, 3918),  // Anthony Johnson
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
	new(null, 733),   // Dennis J Hamm
};

#endregion