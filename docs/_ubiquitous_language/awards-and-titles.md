---
layout: default
title: Ubiquitous Language - Awards & Titles
---

# Ubiquitous Language: Awards & Titles

This document defines the core terminology used in the **Awards** and **Titles** features of the NEBA Management application. These terms represent the shared language used by domain experts, developers, and stakeholders to ensure consistent understanding.

> **Note:** The Bowler entity is documented separately as it's a core concept used across multiple bounded contexts (website public pages, admin, awards, titles, etc.).

---

## Awards Domain

### SeasonAward

**Definition:** A season award recognizes exceptional achievement by a bowler during a NEBA bowling season. Awards are given for outstanding performance in specific categories: overall performance (Bowler of the Year), highest average, or highest 5-game qualifying block score.

**Properties:**

- `SeasonAwardId` - Unique identifier
- `AwardType` - Type of award (BowlerOfTheYear, HighAverage, High5GameBlock)
- `Season` - The season for which the award is given (typically "YYYY" format, e.g., "2025"; exception: "2020/2021" for the COVID-impacted combined season)
- `BowlerId` - The bowler receiving the award
- `BowlerOfTheYearCategory` - Category for BOTY awards (Open, Woman, Senior, SuperSenior, Rookie, Youth); null for non-BOTY awards
- `HighBlockScore` - The highest 5-game block score (for High5GameBlock awards only)
- `Average` - The bowler's season average (for HighAverage awards only)
- `SeasonTotalGames` - Total games bowled in stat-eligible tournaments during the season (for HighAverage awards; provides context for comparison)
- `Tournaments` - Number of tournaments participated in during the season (for HighAverage awards; provides context and potential tie-breaking)

**Season Definition:**

- Standard seasons run from January 1 - December 31 (calendar year)
- Exception: 2020/2021 was a combined season due to COVID-19 tournament cancellations (the only time in NEBA history tournaments were cancelled)
- The Season property is a string to accommodate the "2020/2021" format

**Business Rules:**

- A bowler can win multiple awards in the same season, provided they meet the eligibility criteria for each
- A bowler can win awards in multiple BOTY categories in the same season (e.g., a 62-year-old woman could win Woman, Senior, SuperSenior, and Open BOTY)
- Awards can technically be corrected if errors are discovered, though this has not yet occurred in practice
- Ties result in co-winners unless tie-breaker rules are established (currently no formal tie-breakers defined; potential tie-breakers under consideration: number of tournaments bowled, then number of games)

**Related Terms:** Bowler, SeasonAwardType, BowlerOfTheYearCategory, Stat-Eligible Tournament

**Code Reference:** `src/backend/Neba.Domain/Awards/SeasonAward.cs`

---

### SeasonAwardType

**Definition:** The types of season awards that can be earned by NEBA bowlers, each recognizing different aspects of competitive excellence.

**Values:**

- **BowlerOfTheYear** (value: 1) - Awarded to the bowler(s) with the best overall performance during the season, determined by a points system based on tournament finishes. Points are earned through placement in stat-eligible tournaments, with additional points awarded for being high qualifier. Different tournament types (Singles, Doubles, etc.) have different point structures, but the concept of earning points based on finish position remains consistent across all formats.

- **HighAverage** (value: 2) - Awarded for achieving the highest average score during the season across stat-eligible tournaments.
  - **Eligibility:** Minimum of 4.5 × (number of stat-eligible tournaments completed) games, with decimals dropped (e.g., 4.5 × 9 = 40.5 → 40 games required)
  - **Calculation:** All games bowled in stat-eligible tournaments count toward the average
  - **No tournament minimum:** Only the total games requirement must be met
  - Team tournament games count toward individual averages (exception: baker team finals do not count)

- **High5GameBlock** (value: 3) - Awarded for the highest total score across 5 qualifying games in a single tournament.
  - **Games counted:** First 5 games of qualifying only (if a tournament has more than 5 qualifying games, only the first 5 count)
  - **No minimum tournaments:** Technically requires 1 tournament (since the bowler must have bowled the score), but no formal minimum beyond that
  - **Note:** The award type name includes "5Game" to preserve historical context; if qualifying game counts change in the future, a new award type would be created

**Business Rules:**

- All award types are calculated and awarded per season
- Multiple winners (co-winners) are possible for any award type in case of ties
- BOTY is currently calculated in an external system that is being migrated to this application
- Awards are based only on stat-eligible tournaments (see Stat-Eligible Tournament definition below)

**Related Terms:** Stat-Eligible Tournament, BowlerOfTheYearCategory

**Code Reference:** `src/backend/Neba.Domain/Awards/SeasonAwardType.cs`

---

### BowlerOfTheYearCategory

**Definition:** Categories used to group bowlers for Bowler of the Year awards, allowing recognition across different demographics and experience levels. Each category has specific eligibility criteria, and bowlers can compete in multiple categories simultaneously.

**Values:**

- **Open** (value: 1) - All NEBA members are eligible. This is the most prestigious BOTY category with no restrictions.

- **Woman** (value: 2) - Female NEBA members competing in stat-eligible tournaments open to all women.
  - **Note:** Women bowling in women-only tournaments earn Woman BOTY points, but women bowling in tournaments restricted to other categories (e.g., Senior tournament not open to all women) do not earn Woman BOTY points for those events.

- **Senior** (value: 50) - Bowlers aged 50 or older.
  - **Age determination:** As of the date of each tournament
  - **Example:** A bowler who turns 50 on June 30 can earn Senior BOTY points starting July 1 for all tournaments from that date through December 31

- **SuperSenior** (value: 60) - Bowlers aged 60 or older.
  - **Age determination:** As of the date of each tournament (same rules as Senior)
  - **Example:** A bowler who turns 60 on June 30 can earn Super Senior BOTY points starting July 1

- **Rookie** (value: 10) - Bowlers in their first year of NEBA membership.
  - **Eligibility:** The season in which they purchased a "New Membership" (as opposed to a "Renewal Membership")
  - **Note:** Membership types and management will be developed in future iterations

- **Youth** (value: 20) - Bowlers under age 18.
  - **Age determination:** As of the date of each tournament
  - **Example:** A bowler who turns 18 on June 30 can earn Youth BOTY points through June 29; starting June 30 they no longer earn Youth points

**Business Rules:**

- **Multiple category eligibility:** A bowler can qualify for and win BOTY in multiple categories during the same season
  - **Example:** A 62-year-old woman bowling in stat-eligible tournaments could win Woman BOTY, Senior BOTY, Super Senior BOTY, and Open BOTY in the same season
- **Age-based categories:** Age eligibility is determined tournament-by-tournament based on the bowler's age on the tournament date
- **Points accumulation:** In stat-eligible tournaments, bowlers earn BOTY points simultaneously for all categories they're eligible for
  - **Exception:** Category-specific tournaments (e.g., Senior tournament) only award points for those specific categories, not for Open or other categories where not all members are eligible to compete
- **Category determination:** Category eligibility is automatically determined based on bowler attributes (age, gender, membership type) at the time of each tournament

**Related Terms:** SeasonAward, SeasonAwardType, Stat-Eligible Tournament

**Code Reference:** `src/backend/Neba.Domain/Awards/BowlerOfTheYearCategory.cs`

---

### Stat-Eligible Tournament

**Definition:** A tournament in which the entire NEBA membership is eligible to participate. Games and results from stat-eligible tournaments count toward individual statistics (averages), BOTY points for Open category, and award eligibility.

**Criteria:**

A tournament is stat-eligible if:

- All NEBA members are allowed to enter (no restrictions based on age, gender, previous wins, etc.)
- The tournament is open for registration to the full membership

**Examples of Stat-Eligible Tournaments:**

- Singles
- Doubles
- Trios
- Masters
- Invitational
- Tournament of Champions

**Examples of NON Stat-Eligible Tournaments:**

- **First Non-Champions event** - Not stat-eligible because there is no corresponding Tournament of Champions that weekend, meaning the entire membership is not eligible to bowl (previous champions are excluded)
- **Senior tournament** - Not stat-eligible for Open BOTY or general average because it's restricted by age; however, it IS eligible for Senior and Super Senior BOTY points
- **Women tournament** - Not stat-eligible for Open BOTY because it's restricted by gender; however, it IS eligible for Woman BOTY points

**Impact on Statistics:**

- **Stat-eligible tournaments:** Games count toward High Average eligibility (4.5× formula), Open BOTY points, and general season statistics
- **Category-specific tournaments:** Games and points count only toward the relevant category (e.g., Senior tournament → Senior BOTY points only)
- **Team tournaments:** Individual games in team tournaments (Doubles, Trios) count toward individual statistics
  - **Exception:** Baker team finals do not count toward individual averages (but team game finals do count)

**Note:** The term "stat-eligible" is the current terminology used within NEBA. This term may be refined in future iterations as the domain language evolves.

**Related Terms:** SeasonAward, SeasonAwardType, BowlerOfTheYearCategory

---

## Titles Domain

### Title / Champion

**Definition:** Title and Champion are two perspectives of the same domain concept: a bowler winning a NEBA tournament championship.

- **Title** (Bowler's perspective): A championship win earned by a bowler. When discussing what a bowler has accomplished, we say they "won a title" or "have X titles"
- **Champion** (Tournament's perspective): The winner(s) of a tournament. When discussing who won a tournament, we say "the tournament champions" or "X is the champion"

This is not an entity with separate records - it's a many-to-many relationship between Tournament and Bowler, stored in the `tournament_champions` join table. Each row represents one bowler winning one tournament.

**Data Model:**

The title/champion relationship is implemented as a many-to-many navigation:

- `Tournament.Champions` - Collection of Bowler entities who won the tournament
- `Bowler.Titles` - Collection of Tournament entities the bowler won

**Domain Information:**

From either perspective, the relationship captures:

- Which bowler won which tournament
- Tournament type (Singles, Doubles, Trios, etc.) - from the Tournament entity
- Tournament date (StartDate, EndDate) - from the Tournament entity
- Bowler name and identity - from the Bowler entity

**Business Rules:**

- **One championship per tournament entry:** Each tournament may have one or more champions depending on tournament type
- **Multiple titles possible:** A bowler can win the same tournament type multiple times (e.g., Singles in January and Singles again in March)
- **Team tournament champions:** For team tournaments (Doubles, Trios), each team member is recorded as a champion
  - Example: A winning Doubles team has two entries in tournament_champions, one for each bowler
  - Example: A winning Trios team has three entries in tournament_champions, one for each bowler
- **Tournament of Champions eligibility:** Any bowler with at least one tournament championship is eligible to compete in the Tournament of Champions
- **Historical preservation:** Championship records are permanent and are never deleted, even for inactive tournament formats
- **Tournament invariants:** Tournament owns the business rules about champions (e.g., Singles tournaments have exactly one champion, Doubles tournaments have exactly two champions)

**Tournament Frequency:**

- Tournaments are held regularly throughout the year, with at least one tournament per month guaranteed
- Tournament dates capture when championships were won

**Related Terms:** Bowler, Tournament, TournamentType

**Code References:**

- Domain: `src/backend/Neba.Website.Domain/Tournaments/Tournament.cs` (Tournament.Champions navigation)
- Domain: `src/backend/Neba.Website.Domain/Bowlers/Bowler.cs` (Bowler.Titles navigation)
- Infrastructure: `src/backend/Neba.Website.Infrastructure/Database/Configurations/TournamentConfiguration.cs` (many-to-many configuration)
- DTOs: `src/backend/Neba.Website.Application/Tournaments/TitleDto.cs` (query results)

---

### TournamentType

**Definition:** The classification system for different formats of NEBA tournaments. Each tournament type defines the structure, eligibility requirements, and competitive format. Tournament types can be active (currently held) or inactive (historical only).

**Properties:**

- `Name` - Display name of the tournament type
- `Value` - Numeric identifier used for ordering and reference
- `TeamSize` - Number of bowlers per team (1 for individual tournaments, 2+ for team tournaments)
- `ActiveFormat` - Indicates whether this tournament format is currently being held (true) or is historical only (false)

**Values:**

#### Individual Tournaments (TeamSize: 1)

- **Singles** (value: 10, active) - The standard individual tournament format, typically held monthly with 3 squads over a weekend (Saturday/Sunday). Stat-eligible.

- **Non-Champions** (value: 11, active) - Individual tournament restricted to bowlers who have never won a NEBA title.
  - **Timing:** One event early in the year (3 squads over Sat/Sun, same format as Singles, but champions excluded); another event in November (2 squads on Saturday with regular finals)
  - **Eligibility:** Bowlers without any Title records
  - **Stat-eligibility:** The first Non-Champions event of the year is NOT stat-eligible (since the entire membership cannot participate). The November Non-Champions event follows the same stat-eligibility rules.

- **Tournament of Champions** (value: 12, active) - Elite individual tournament restricted to bowlers who have won at least one NEBA title. One of the three NEBA Majors.
  - **Timing:** Held in November on Sunday (pairs with Non-Champions on Saturday)
  - **Eligibility:** Must have at least one Title record (checked via `Champion` property on Bowler entity)
  - **BOTY Points:** Worth more points as a Major tournament
  - **Stat-eligible:** Yes

- **Invitational** (value: 13, active) - Special individual tournament with participation requirements and unique format. One of the three NEBA Majors.
  - **Eligibility:** Must have entered a minimum number of tournaments prior to the event (currently 3 tournaments; count includes all tournaments, not just stat-eligible)
  - **Note:** The minimum tournament count has varied historically
  - **Format:** Different tournament structure (details to be defined when tournament domain is developed)
  - **BOTY Points:** Worth more points as a Major tournament
  - **Stat-eligible:** Yes

- **Masters** (value: 14, active) - Premium individual tournament featuring higher difficulty and prestige. One of the three NEBA Majors.
  - **Format:** Different tournament structure from standard Singles
  - **Lane Conditions:** Harder/more challenging lane pattern than standard tournaments
  - **Entry Fee:** Higher than standard tournaments
  - **BOTY Points:** Worth more points as a Major tournament
  - **Stat-eligible:** Yes

- **High Roller** (value: 15, **inactive**) - Historical tournament format no longer in use.
  - **Note:** Inactive for the duration of current organizational memory; distinguishing features not documented

- **Senior** (value: 16, active) - Individual tournament restricted to bowlers aged 50 or older.
  - **Eligibility:** Age 50+ as of the tournament date
  - **Stat-eligibility:** NOT stat-eligible for Open BOTY or general average; counts for Senior and Super Senior BOTY points only

- **Women** (value: 17, active) - Individual tournament restricted to female bowlers.
  - **Eligibility:** Female NEBA members
  - **Stat-eligibility:** NOT stat-eligible for Open BOTY; counts for Woman BOTY points only

- **Over Forty** (value: 18, **inactive**) - Historical age-restricted tournament format no longer in use.
  - **Former eligibility:** Age 40+ as of the tournament date

- **40-49** (value: 19, **inactive**) - Historical age-range tournament format no longer in use.
  - **Former eligibility:** Ages 40-49 as of the tournament date

#### Team Tournaments

- **Doubles** (value: 20, team size: 2, active) - Standard two-person team tournament format, typically held monthly.
  - **Team Size:** 2 bowlers per team
  - **Title Awards:** Each team member receives an individual Title record when the team wins
  - **BOTY Points:** Different point structure than Singles, but same concept based on finish position
  - **Stat-eligible:** Yes

- **Trios** (value: 30, team size: 3, active) - Three-person team tournament format.
  - **Team Size:** 3 bowlers per team
  - **Title Awards:** Each team member receives an individual Title record when the team wins
  - **BOTY Points:** Different point structure than Singles/Doubles
  - **Stat-eligible:** Yes

- **Over/Under 50 Doubles** (value: 21, team size: 2, active) - Two-person team tournament with age-based team composition requirement.
  - **Team Size:** 2 bowlers per team
  - **Eligibility:** Team must have one bowler aged 50+ and one bowler under 50 (as of the tournament date)
  - **Stat-eligible:** Yes

- **Over/Under 40 Doubles** (value: 22, team size: 2, **inactive**) - Historical two-person team tournament format no longer in use.
  - **Former eligibility:** Team must have one bowler aged 40+ and one bowler under 40 (as of the tournament date)

**Business Rules:**

- **Active vs Inactive formats:**
  - Active formats (`ActiveFormat = true`) are currently held and can be selected when creating new tournaments
  - Inactive formats (`ActiveFormat = false`) are preserved for historical record-keeping only; new tournaments of these types cannot be created
  - Inactive formats: High Roller, Over Forty, 40-49, Over/Under 40 Doubles
- **NEBA Majors:** Tournament of Champions, Invitational, and Masters are considered Major tournaments and award additional BOTY points
- **Tournament frequency:** Most active tournament types are held at least monthly; some (like Tournament of Champions) are held annually
- **Team tournament titles:** When a team wins, each individual team member receives their own Title record

**Related Terms:** Title, Stat-Eligible Tournament, BOTY Points

**Code Reference:** `src/backend/Neba.Domain/Tournaments/TournamentType.cs`

---

## Shared Concepts

### Bowler

**Definition:** A bowler represents a NEBA member who participates in tournaments. The Bowler entity serves as the aggregate root for tracking competitive achievements, including titles won and season awards earned.

> **Note:** Bowler is a core entity used across multiple bounded contexts (awards, titles, website display, admin management). The current implementation is minimal, focused on website display needs. Additional properties for member management will be added when migrating from the organization management software.

**Properties (Current Implementation):**

- `BowlerId` - Unique identifier for the bowler in the new system
- `Name` - The bowler's full name (value object containing first name, last name, middle name, suffix, and optional nickname)
- `WebsiteId` - Legacy identifier from the existing NEBA website database (used for data migration; maintained for historical reference)
- `ApplicationId` - Legacy identifier from the existing organization management software (used for data migration; maintained for historical reference)
- `Titles` - Read-only collection of championship titles won by the bowler
- `SeasonAwards` - Read-only collection of season awards earned by the bowler
- `HallOfFameInductions` - Read-only collection of Hall of Fame inductions for the bowler

**Aggregate Boundary:**

The Bowler is an aggregate root containing:

- **Title entities:** Championships won in tournaments
- **SeasonAward entities:** Season-level achievements (BOTY, High Average, High Block)
- **HallOfFameInduction entities:** Hall of Fame recognitions for superior performance, meritorious service, or friend of NEBA

**Invariants maintained:**

- All titles belong to this specific bowler
- All season awards belong to this specific bowler
- All Hall of Fame inductions belong to this specific bowler
- Bowler identity is immutable once created

**Future Properties:**

When organization management functionality is migrated, additional bowler properties will be defined, including:

- Date of birth (required for age-based tournament/award eligibility)
- Gender (required for gender-specific tournaments/awards)
- Membership information (join date, membership type, renewal status)
- Contact and personal information (address, phone, email)
- Privacy settings (public vs admin-only data visibility)

These properties and their business rules will be documented when that domain work begins.

**Business Rules (Current):**

- **Legacy identifiers:** WebsiteId and ApplicationId are nullable and used only for migration from legacy systems
- **Duplicate handling:** Duplicate bowler records are identified and merged manually during data migration scripts
- **Immutable identity:** Once a BowlerId is assigned, it never changes
- **Aggregate consistency:** Titles and SeasonAwards can only be added/modified through the Bowler aggregate root

**Privacy Considerations:**

Privacy rules for public website vs admin access will be defined when member management features are developed. Considerations include:

- Public data: Name, titles, awards, possibly city/state for tournament results
- Private data: Contact information, detailed address, financial information (e.g., for 1099 reporting)

**Related Terms:** Title, SeasonAward, HallOfFameInduction, Name, Tournament of Champions

**Code Reference:** `src/backend/Neba.Domain/Bowlers/Bowler.cs`

---

## Value Objects

### Name

**Definition:** A value object representing a bowler's complete name information, including legal name components and an optional nickname for informal display. The Name value object provides multiple formatting options for different contexts (legal documents, public display, formal communications).

**Properties:**

- `FirstName` (required) - The bowler's given first name, used in official records
- `LastName` (required) - The bowler's family or surname, used in official records
- `MiddleName` (optional) - The bowler's middle name, used in legal or formal contexts
- `Suffix` (optional) - Name suffix such as Jr., Sr., III, etc., used in official records
- `Nickname` (optional) - The bowler's preferred informal name (e.g., "Dave" for David, "Mike" for Michael, or non-standard nicknames like "Ditto")

**Validation Rules:**

- **Required fields:** FirstName and LastName must be provided and cannot be empty or whitespace
- **No maximum length:** Field lengths are pragmatically sized and can be adjusted if a unique scenario requires it; no strict business rule maximum
- **Special characters:** Standard name characters are allowed, including hyphens, apostrophes, and other common name punctuation
- **Character encoding:** Standard Unicode support for international characters

**Display Formats:**

The Name value object provides three display format methods:

1. **Legal Name (`ToLegalName()`)**: FirstName [MiddleName] LastName[, Suffix]
   - **Format:** Full legal name with all components
   - **Example:** "David Michael Smith, Jr."
   - **Use case:** Official documents, 1099 tax reporting, legal records

2. **Display Name (`ToDisplayName()`)**: [Nickname|FirstName] LastName
   - **Format:** Uses nickname if available, otherwise uses first name
   - **Example with nickname:** "Dave Smith" (nickname replaces first name)
   - **Example without nickname:** "David Smith"
   - **Use case:** Public website display, tournament results, awards lists

3. **Formal Name (`ToFormalName()`)**: FirstName LastName
   - **Format:** First and last name only, ignoring nickname
   - **Example:** "David Smith"
   - **Use case:** Formal communications where nicknames are inappropriate

**Business Rules:**

- **Nickname flexibility:** No restrictions on nickname content; can be traditional derivatives (Dave/David) or completely unrelated (Ditto)
- **Immutability:** Name is a value object; any changes create a new Name instance
- **Equality:** Two Name objects are equal if all properties match (value-based equality)
- **Default string representation:** Calling `ToString()` returns the legal name format

**Related Terms:** Bowler

**Code Reference:** `src/backend/Neba.Domain/Bowlers/Name.cs`

---

### Month

**Definition:** A value object representing a calendar month, used primarily to record when tournament titles were won. Provides both full month names and three-letter abbreviations for display flexibility.

**Values:**

Standard calendar months with numeric values 1-12:

- January (1), February (2), March (3), April (4), May (5), June (6)
- July (7), August (8), September (9), October (10), November (11), December (12)

**Display Formats:**

- **Full name:** `Name` property returns full month name (e.g., "January", "February")
- **Abbreviated:** `ToShortString()` method returns 3-letter abbreviation (e.g., "Jan", "Feb", "Mar")
- **Numeric:** `Value` property returns numeric month (1-12)

**Usage in Domain:**

- **Title tracking:** Records the month when a championship was won
- **Tournament scheduling:** Indicates when tournaments are held (at least one guaranteed per month)
- **Historical records:** Provides consistent month representation across all time periods

**Business Rules:**

- **Multi-day tournaments:** If a tournament spans multiple days across a month boundary (e.g., last Saturday of one month, first Sunday of next month), the month of the championship final/completion is recorded
- **Month-based queries:** Enables filtering titles by month (e.g., "all Singles titles won in January")
- **Immutability:** Month is a SmartEnum; instances are predefined and immutable

**Related Terms:** Title, Tournament

**Code Reference:** `src/shared-kernel/Neba.SharedKernel/Month.cs`

---

## Domain Events

[TODO: Document any domain events related to Awards and Titles]

**Potential Events:**
- `TitleAwarded` - When a bowler wins a tournament
- `SeasonAwardGranted` - When a season award is given
- `BowlerRegistered` - When a new bowler joins
- `SeasonCompleted` - When a season ends and awards are finalized

---

## Aggregate Boundaries

### Bowler Aggregate

**Aggregate Root:** Bowler

**Entities within boundary:**
- Title (child entity)
- SeasonAward (child entity)
- HallOfFameInduction (child entity)

**Invariants:**

- [TODO: Define invariants that must be maintained within the aggregate]
  - Title dates must be valid?
  - Award seasons must be valid?
  - No duplicate titles for same tournament/year/month?

**Why this boundary?**

[TODO: Explain the reasoning for this aggregate design]

- Bowler owns their collection of titles and awards
- Transactional consistency: adding a title/award to a bowler happens atomically
- Website queries: need bowler + titles + awards together frequently

---

## Questions for Discussion

### Awards

1. What are the exact criteria for each award type?
2. How are minimum game/tournament requirements determined?
3. How are ties handled?
4. Can awards be revoked or corrected?
5. What defines a "season" (calendar year, bowling season)?

### Titles

1. What distinguishes each tournament type?
2. How often is each tournament held?
3. For team tournaments (Doubles, Trios), does each team member get a Title record?
4. Can the same bowler win multiple tournaments in the same month?
5. What makes a tournament format "active" vs inactive?

### Bowlers

1. What additional fields will be needed for the full admin functionality?
2. How should privacy be handled (public website vs admin access)?
3. What's the bowler lifecycle (registration, active, inactive, retired)?
4. How are duplicate bowlers prevented/merged?
5. What validation is needed for bowler data?

---

## Related Documentation

- [Hall of Fame]({{ '/ubiquitous-language/hall-of-fame' | relative_url }}) - Hall of Fame inductions and categories
- [Technical Building Blocks]({{ '/ubiquitous-language/technical-building-blocks' | relative_url }}) - Core domain infrastructure and cross-cutting concepts
- [Bowlers Domain]({{ '/ubiquitous-language/bowlers' | relative_url }}) (Coming Soon - when we expand bowler entity)
- [Domain Models]({{ '/domain-models' | relative_url }}) (Coming Soon)
- [Business Rules]({{ '/business-rules' | relative_url }}) (Coming Soon)
- [API Reference]({{ '/reference/api' | relative_url }}) (Coming Soon)
