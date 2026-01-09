<Query Kind="Program">
  <Connection>
    <ID>03d6fe58-6e10-4e62-bde0-15ee51396c76</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>bowlneba-eastus.database.windows.net</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <UseMicrosoftDataSqlClient>true</UseMicrosoftDataSqlClient>
    <DisplayName>NEBA v3</DisplayName>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>neba</Database>
    <SqlSecurity>true</SqlSecurity>
    <UserName>nebamgmt</UserName>
    <DbVersion>Azure</DbVersion>
    <IsProduction>true</IsProduction>
    <MapXmlToString>false</MapXmlToString>
    <DriverData>
      <SkipCertificateCheck>false</SkipCertificateCheck>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	var tournaments = Tournaments.Select(t => new
	{
		SoftwareTournamentId = t.Id,
		TournamentName = t.Name,
		TournamentStartDate = t.Start.DateOnly(),
		TournamentEndDate = t.End.DateOnly(),
		TournamentType = Tournaments_SinglesTournaments.Select(s => s.Id).Contains(t.Id)
			? MapSinglesTournamentType(t.Tournaments_SinglesTournament.TournamentType)
			: MapDoublesTournamentType(t.Tournaments_TeamTournament.TeamSize, t.Tournaments_TeamTournament.OverUnder),
		CenterName = t.BowlingCenter.Name,
		CenterAddress = $"{t.BowlingCenter.MailingAddress_City}, {t.BowlingCenter.MailingAddress_State}",
		Winners = string.Join(" / ",
	t.TournamentStats
		.Where(ts => ts.Stats_ResultsStats != null && ts.Stats_ResultsStats.Place == 1)
		.Select(ts => ts.Bowler.FirstName + " " + ts.Bowler.LastName)
		.ToList()),
		Entries = t.TournamentStats.Count(t => t.Stats_QualifyingStats != null) / (t.Tournaments_TeamTournament != null ? t.Tournaments_TeamTournament.TeamSize : 1)
	});
	
	tournaments.Dump();
}

// You can define other methods, fields, classes and namespaces here
private string MapSinglesTournamentType(int tournamentType)
{
	switch (tournamentType)
	{
		case 0:
			return "Singles";
		case 1:
			return "Non-Champions";
		case 2:
		case 8:
			return "Senior";
		case 3:
			return "Women";
		case 4:
			return "Tournament of Champions";
		case 5:
			return "Invitational";
		case 6:
			return "Masters";
		case 7:
			return "Youth";
		default:
			throw new InvalidOperationException("Can't find tournament type: " + tournamentType);
	}
}

private string MapDoublesTournamentType(int teamSize, bool overUnder)
{
	if (teamSize == 3)
	{
		return "Trios";
	}
	
	return overUnder ? "Over/Under 50" : "Doubles";
}