---
layout: default
title: Ubiquitous Language - Tournaments
---

# Ubiquitous Language: Tournaments

This document defines the core terminology used in the **Tournaments** domain of the NEBA Management application. These terms represent the shared language used by domain experts, developers, and stakeholders to ensure consistent understanding.

> **Note:** This document focuses on tournament entities and their properties. Tournament types and title/champion concepts are documented in the [Awards & Titles]({{ '/ubiquitous-language/awards-and-titles' | relative_url }}) document due to their close relationship with awards and bowler achievements.

---

## Tournament Domain

### Tournament

**Definition:** A tournament represents a competitive bowling event held by NEBA at a specific location and time. Tournaments have a format (type), may use specific lane patterns, and produce champions who earn titles for their victories. The Tournament entity serves as an aggregate root for tournament-specific data.

**Properties:**

- `TournamentId` - Unique identifier for the tournament
- `Name` - Display name of the tournament
- `StartDate` - Date when the tournament begins
- `EndDate` - Date when the tournament concludes
- `TournamentType` - The format/classification of the tournament (Singles, Doubles, Masters, etc.)
- `BowlingCenterId` - Reference to the bowling center where the tournament is held (optional for historical tournaments that may not have center data)
- `LanePattern` - Optional information about the oil pattern used during the tournament
- `EntryCount` - Number of entries/participants in the tournament (optional; may not be tracked for all historical tournaments)
- `ApplicationId` - Legacy identifier from the existing organization management software (used for data migration and historical reference)
- `Champions` - Collection of bowlers who won the tournament (see Champion relationship)

**Tournament Dates:**

- Single-day tournaments have the same StartDate and EndDate
- Multi-day tournaments span multiple dates (e.g., Saturday/Sunday weekend tournaments)
- The EndDate represents when the tournament concluded and champions were determined
- Month-based tournament tracking uses the EndDate (e.g., a tournament finishing on Sunday is counted in that month, even if it started on Saturday of the previous month)

**Tournament Frequency:**

- At least one tournament per month is guaranteed throughout the year
- Some tournament types are held monthly (Singles, Doubles)
- Other tournament types are held less frequently (Tournament of Champions annually in November)
- Major tournaments (Invitational, Masters, Tournament of Champions) are scheduled throughout the year

**Business Rules:**

- **Tournament identity:** Each tournament is a unique event with its own identity, even if the same tournament type is held multiple times per year
- **Champion constraints:** The number of required champions depends on the TournamentType:
  - Singles tournaments have exactly one champion
  - Doubles tournaments have exactly two champions (one per team member)
  - Trios tournaments have exactly three champions (one per team member)
- **Historical preservation:** Tournament records are permanent and are never deleted
- **Bowling center lifecycle:** BowlingCenterId is optional to support the tournament scheduling workflow:
  - **Scheduling phase:** Tournaments are created and published to the schedule before all details are finalized; bowling center assignment may be pending
  - **Execution phase:** By the time a tournament runs (StartDate arrives), it must have a bowling center assigned
  - **Historical data:** Some historical tournaments may also lack bowling center data from legacy system migrations

**Aggregate Boundary:**

The Tournament aggregate owns the business rules for champions, including how many champions are valid for each tournament type. Cross-aggregate business rules (such as age requirements for Senior tournaments) are validated in domain services before adding champions.

**Related Terms:** TournamentType, LanePattern, BowlingCenter, Title/Champion, Bowler

**Code Reference:** `src/backend/Neba.Website.Domain/Tournaments/Tournament.cs`

---

### LanePattern

**Definition:** A value object representing the oil pattern applied to bowling lanes during a tournament. Lane patterns significantly affect ball motion and scoring difficulty. The pattern is categorized by its length and ratio characteristics.

**Properties:**

- `LengthCategory` - Classification of pattern length (Short, Medium, Long)
- `RatioCategory` - Classification of pattern difficulty (Sport, Challenge, Recreation)

**Purpose:**

Lane patterns are recorded to:
- Document tournament conditions for historical reference
- Provide context for scoring and performance comparisons
- Help bowlers understand the difficulty level of different tournaments

**Usage:**

- Lane pattern data is optional on tournaments (not all tournaments have pattern information recorded)
- Pattern information is primarily used for informational and historical purposes
- Major tournaments like Masters may feature more challenging patterns (e.g., Long pattern, Sport ratio)

**Related Terms:** Tournament, PatternLengthCategory, PatternRatioCategory

**Code Reference:** `src/backend/Neba.Website.Domain/Tournaments/LanePattern.cs`

---

### PatternLengthCategory

**Definition:** A SmartEnum classification system for the length of bowling lane oil patterns, measured in feet. Pattern length affects where the ball begins to hook and significantly impacts scoring.

**Values:**

- **ShortPattern** (value: 1, name: "Short") - Patterns less than 38 feet
  - Range: Up to 37 feet (maximum)
  - Characteristics: Ball hooks earlier, requires less aggressive play

- **MediumPattern** (value: 2, name: "Medium") - Patterns from 38 to 42 feet
  - Range: 38 feet (minimum) to 42 feet (maximum)
  - Characteristics: Balanced pattern length, moderate hook zone

- **LongPattern** (value: 3, name: "Long") - Patterns 43 feet or longer
  - Range: 43 feet (minimum) and up
  - Characteristics: Ball travels farther before hooking, requires more aggressive angles

**Pattern Length Impact:**

- **Shorter patterns:** Generally allow for higher scores as the ball hooks earlier and more predictably
- **Longer patterns:** Typically more challenging as the ball must travel farther down the lane before hooking
- **NEBA usage:** Most standard tournaments use Medium patterns; Major tournaments may feature Long patterns for increased difficulty

**Related Terms:** LanePattern, PatternRatioCategory

**Code Reference:** `src/backend/Neba.Domain/Tournaments/PatternLengthCategory.cs`

---

### PatternRatioCategory

**Definition:** A SmartEnum classification system for the oil pattern ratio, which measures the difference in oil volume between the center and outside portions of the lane. Lower ratios indicate more challenging conditions.

**Values:**

- **Sport** (value: 1) - Ratios less than 4.0
  - Range: Up to 4.0 (maximum, exclusive)
  - Characteristics: Most challenging conditions; requires precision and skill
  - Ball reaction: Minimal difference between center and outside of lane

- **Challenge** (value: 2) - Ratios from 4.0 to 8.0
  - Range: 4.0 (minimum) to 8.0 (maximum)
  - Characteristics: Moderate difficulty; balanced scoring opportunity
  - Ball reaction: Noticeable difference between center and outside

- **Recreation** (value: 3) - Ratios 8.0 or higher
  - Range: 8.0 (minimum) and up
  - Characteristics: Higher scoring potential; more forgiving conditions
  - Ball reaction: Significant difference between center and outside creates a "channel"

**Pattern Ratio Impact:**

- **Sport ratios:** Most difficult; errors are penalized more severely
- **Challenge ratios:** Moderate difficulty; typical for competitive tournaments
- **Recreation ratios:** Most forgiving; errors are less costly

**USBC Pattern Classifications:**

- Sport patterns: Ratios under 3:1
- Challenge patterns: Ratios between 3:1 and 10:1
- Recreational patterns: Ratios over 10:1

**NEBA Usage:**

- Standard tournaments typically use Challenge ratios
- Major tournaments (Masters, Invitational, Tournament of Champions) may feature Sport ratios for increased difficulty
- Pattern ratio, combined with length, determines overall pattern difficulty

**Related Terms:** LanePattern, PatternLengthCategory

**Code Reference:** `src/backend/Neba.Domain/Tournaments/PatternRatioCategory.cs`

---

### BowlingCenter

**Definition:** An entity representing a physical bowling center facility where tournaments are held. Bowling centers have identity, name, address, contact information, and operational status.

**Properties:**

- `BowlingCenterId` - Unique identifier for the bowling center
- `Name` - Display name of the bowling center
- `Address` - Physical address of the bowling center (value object with street, city, state, postal code)
- `PhoneNumber` - Primary contact phone number (value object with country code, digits, and optional extension)
- `IsClosed` - Indicator of whether the bowling center has permanently closed
- `WebsiteId` - Legacy identifier from the existing NEBA website database (used for data migration)

**Business Rules:**

- **Historical centers:** Bowling centers that have closed (IsClosed = true) are retained in the system for historical tournament records
- **Tournament reference:** Tournaments reference bowling centers to record where events were held
- **Optional during scheduling:** Tournaments may be created without a bowling center assignment to enable early schedule publication; the bowling center must be assigned before the tournament runs
- **Required for execution:** A tournament must have a bowling center assigned by its StartDate (when it actually runs)

**Use Cases:**

- Display tournament locations on the website
- Filter tournaments by bowling center
- Track which centers have hosted NEBA events
- Provide contact information for tournament locations

**Privacy and Public Data:**

- Bowling center information (name, address, phone) is public data displayed on the website
- Centers are business entities, not individuals, so privacy considerations are minimal

**Related Terms:** Tournament, Address, PhoneNumber

**Code Reference:** `src/backend/Neba.Website.Domain/BowlingCenters/BowlingCenter.cs`

---

## Champion / Title Relationship

For documentation on the Champion/Title concept (the many-to-many relationship between Tournament and Bowler), see the [Title / Champion section in Awards & Titles]({{ '/ubiquitous-language/awards-and-titles' | relative_url }}#title--champion).

Key points:

- **Champion** (Tournament perspective): The bowler(s) who won a tournament
- **Title** (Bowler perspective): A tournament championship earned by a bowler
- **Implementation:** Many-to-many relationship stored in `tournament_champions` join table
- **Business rules:** Owned by the Tournament aggregate (e.g., Singles has one champion, Doubles has two)

---

## TournamentType

For documentation on TournamentType (Singles, Doubles, Trios, Masters, etc.), see the [TournamentType section in Awards & Titles]({{ '/ubiquitous-language/awards-and-titles' | relative_url }}#tournamenttype).

TournamentType is a SmartEnum that classifies tournament formats and includes:
- Individual tournaments (Singles, Masters, Tournament of Champions, etc.)
- Team tournaments (Doubles, Trios, Over/Under 50 Doubles)
- Active vs inactive tournament formats
- Team size information
- Stat-eligibility implications

---

## Questions for Discussion

### Tournament Scheduling

1. How are tournament dates determined and published?
2. What is the notice period for tournament registration?
3. How are schedule conflicts handled?
4. Are there blackout periods or guaranteed tournament-free dates?

### Tournament Structure

1. What are the qualification round formats (number of games, squad structure)?
2. How are finals structured for each tournament type?
3. What are the advancement criteria from qualifying to finals?
4. How are tie-breakers determined in qualifying and finals?

### Entry Management

1. How is EntryCount tracked and maintained?
2. Are there minimum/maximum entry requirements?
3. How are entry fees determined and tracked?
4. How are refunds and cancellations handled?

### Historical Data

1. Which historical tournaments are missing bowling center data?
2. Which historical tournaments are missing lane pattern data?
3. How far back does the tournament data go?
4. Are there known gaps or inconsistencies in historical records?

---

## Related Documentation

- [Awards & Titles]({{ '/ubiquitous-language/awards-and-titles' | relative_url }}) - Tournament types, titles, and champion relationships
- [Technical Building Blocks]({{ '/ubiquitous-language/technical-building-blocks' | relative_url }}) - Core domain infrastructure and cross-cutting concepts
- [Architecture Decision Records]({{ '/architecture' | relative_url }}) - Technical decisions affecting tournament implementation
- [Domain Models]({{ '/domain-models' | relative_url }}) (Coming Soon)
- [Business Rules]({{ '/business-rules' | relative_url }}) (Coming Soon)

---

<!-- Navigation -->
[← Back to Ubiquitous Language]({{ '/ubiquitous-language' | relative_url }})
