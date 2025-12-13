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

**Definition:** [TODO: Define what season awards represent - yearly recognition for achievement during a bowling season]

**Properties:**
- `SeasonAwardId` - Unique identifier
- `AwardType` - Type of award (BowlerOfTheYear, HighAverage, High5GameBlock)
- `Season` - Season year (e.g., "2025")
- `BowlerId` - The bowler receiving the award
- `BowlerOfTheYearCategory` - Category for BOTY awards (if applicable)
- `HighBlockScore` - Score for high block awards (if applicable)
- `Average` - Average for high average awards (if applicable)
- `SeasonTotalGames` - Number of games bowled in the season
- `Tournaments` - Number of tournaments participated in

**Business Rules:**
- [TODO: Define key business rules]
  - Can a bowler win multiple awards in the same season?
  - What are the minimum requirements (games, tournaments) for eligibility?
  - How are ties handled?

**Related Terms:** Bowler, SeasonAwardType, BowlerOfTheYearCategory

**Code Reference:** `src/backend/Neba.Domain/Awards/SeasonAward.cs`

---

### SeasonAwardType

**Definition:** [TODO: Define the types of season awards]

**Values:**
- **BowlerOfTheYear** (value: 1) - [TODO: Define criteria - best overall performance?]
- **HighAverage** (value: 2) - [TODO: Define criteria - highest average across the season with minimum games?]
- **High5GameBlock** (value: 3) - [TODO: Define criteria - highest total across any 5 consecutive games?]

**Business Rules:**
- [TODO: Define rules about award types]
  - Are these awarded per season?
  - Can there be multiple winners per type?
  - What's the qualification criteria for each?

**Code Reference:** `src/backend/Neba.Domain/Awards/SeasonAwardType.cs`

---

### BowlerOfTheYearCategory

**Definition:** [TODO: Define BOTY category classifications - how bowlers are grouped for BOTY awards]

**Values:**
- **Open** (value: 1) - [TODO: Define eligibility - any bowler?]
- **Woman** (value: 2) - [TODO: Define eligibility]
- **Senior** (value: 50) - [TODO: Define eligibility - age requirement?]
- **SuperSenior** (value: 60) - [TODO: Define eligibility - age requirement?]
- **Rookie** (value: 10) - [TODO: Define eligibility - first year?]
- **Youth** (value: 20) - [TODO: Define eligibility - age requirement?]

**Business Rules:**
- [TODO: Define rules about categories]
  - Can a bowler qualify for multiple categories in the same season?
  - What are the exact age/experience requirements?
  - How is the category determined/selected?

**Code Reference:** `src/backend/Neba.Domain/Awards/BowlerOfTheYearCategory.cs`

---

## Titles Domain

### Title

**Definition:** [TODO: Define what a Title represents - a championship win in a NEBA tournament]

**Properties:**
- `TitleId` - Unique identifier
- `BowlerId` - The bowler who won the title
- `TournamentType` - Type of tournament (Singles, Doubles, etc.)
- `Month` - Month the title was won
- `Year` - Year the title was won

**Business Rules:**
- [TODO: Define key business rules]
  - Can a bowler win the same tournament type multiple times in one year?
  - How are team tournaments handled (Doubles, Trios)?
  - Is there a "title defense" concept?

**Related Terms:** Bowler, TournamentType, Month

**Code Reference:** `src/backend/Neba.Domain/Tournaments/Title.cs`

---

### TournamentType

**Definition:** [TODO: Define tournament type classification system - different formats of NEBA tournaments]

**Values:**

#### Individual Tournaments (1 player)
- **Singles** (value: 10, team size: 1) - [TODO: Define - standard individual tournament?]
- **Non-Champions** (value: 11, team size: 1) - [TODO: Define - for bowlers who haven't won previously?]
- **Tournament of Champions** (value: 12, team size: 1) - [TODO: Define - only previous winners?]
- **Invitational** (value: 13, team size: 1) - [TODO: Define - by invitation only?]
- **Masters** (value: 14, team size: 1) - [TODO: Define]
- **High Roller** (value: 15, team size: 1) - [TODO: Define - higher entry fee?]
- **Senior** (value: 16, team size: 1) - [TODO: Define - age requirement?]
- **Women** (value: 17, team size: 1) - [TODO: Define]
- **Over Forty** (value: 18, team size: 1) - [TODO: Define - 40+ age requirement?]
- **40-49** (value: 19, team size: 1) - [TODO: Define - specific age range?]

#### Team Tournaments
- **Doubles** (value: 20, team size: 2) - [TODO: Define - 2-person teams]
- **Trios** (value: 30, team size: 3) - [TODO: Define - 3-person teams]
- **Over/Under 50 Doubles** (value: 21, team size: 2) - [TODO: Define - mixed age teams?]
- **Over/Under 40 Doubles** (value: 22, team size: 2) - [TODO: Define - mixed age teams?]

**Properties:**
- `Name` - Display name of the tournament type
- `Value` - Numeric identifier
- `TeamSize` - Number of bowlers per team (1 for individual, 2+ for teams)
- `ActiveFormat` - Whether this format is currently active

**Business Rules:**
- [TODO: Define rules about tournament types]
  - How often are these tournaments held (monthly, yearly)?
  - Can tournament formats be retired (ActiveFormat = false)?
  - For team tournaments, how are titles awarded (to each team member)?

**Code Reference:** `src/backend/Neba.Domain/Tournaments/TournamentType.cs`

---

## Shared Concepts

### Bowler

**Definition:** [TODO: Define what a Bowler represents - a NEBA member who participates in tournaments]

> **Note:** Bowler is a core entity used across multiple features (awards, titles, website display, admin management). Currently implemented in minimal form for the website schema.

**Properties (Current - Minimal):**
- `BowlerId` - Unique identifier
- `Name` - The bowler's full name (value object)
- `WebsiteId` - Legacy database reference
- `ApplicationId` - Legacy database reference
- `Titles` - Collection of championships won
- `SeasonAwards` - Collection of awards earned

**Potential Future Properties:**
[TODO: Discuss what additional fields might be needed]
- Date of birth (for age-restricted tournaments/awards)?
- Gender (for gender-specific tournaments/awards)?
- Join date / years of membership?
- Contact information (admin side)?
- Active/inactive status?

**Business Rules:**
- [TODO: Define key business rules for Bowler]
  - What makes a bowler "active"?
  - How are bowler merges handled (duplicates)?
  - Privacy considerations for public website vs admin?

**Related Terms:** Title, SeasonAward, Name

**Code Reference:** `src/backend/Neba.Domain/Bowlers/Bowler.cs`

---

## Value Objects

### Name

**Definition:** [TODO: Define how bowler names are structured]

**Properties:**
- [TODO: List properties - first name, last name, middle initial, suffix, nickname?]

**Validation Rules:**
- [TODO: Define validation rules]
  - Required fields?
  - Character limits?
  - Special character handling?

**Display Format:**
- [TODO: Define how names are displayed]
  - Full name format?
  - Abbreviated format?
  - Sorting/alphabetization rules?

**Code Reference:** `src/backend/Neba.Domain/Bowlers/Name.cs`

---

### Month

**Definition:** [TODO: Define how months are represented in tournament context - when a title was won]

**Values:**
- January (1) through December (12)

**Format:**
- `ToShortString()` - Returns 3-letter abbreviation (e.g., "Jan", "Feb")

**Business Rules:**
- [TODO: Any special rules about months in tournament context]
  - Are tournaments held every month?
  - How are multi-day tournaments spanning months handled?

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

- [Bowlers Domain]({{ '/ubiquitous-language/bowlers' | relative_url }}) (Coming Soon - when we expand bowler entity)
- [Domain Models]({{ '/domain-models' | relative_url }}) (Coming Soon)
- [Business Rules]({{ '/business-rules' | relative_url }}) (Coming Soon)
- [API Reference]({{ '/reference/api' | relative_url }}) (Coming Soon)
