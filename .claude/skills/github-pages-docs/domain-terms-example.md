# Example: Domain Terms Documentation

This file provides an example of how to document ubiquitous language terms from the NEBA Management System domain.

---

## Tournament

**Definition:** A competitive bowling event organized under NEBA rules, consisting of one or more games where bowlers compete for prizes and rankings.

**Context:** Tournament Management bounded context

**Examples:**

- "The Monthly Tournament had 45 entries this month"
- "Tournament results must be submitted within 48 hours of completion"
- "Each Tournament has a unique identifier and date range"

**Implementation:** [`Tournament.cs`](../../src/backend/Neba.Domain/Tournaments/Tournament.cs)

**Related Terms:** [Entry](#entry), [Champion](#champion), [Tournament Type](#tournament-type)

---

## Entry

**Definition:** A bowler's registration and participation record for a specific tournament, including their scores and placement.

**Context:** Tournament Management bounded context

**Examples:**

- "John Smith's Entry shows a 235 average across 6 games"
- "Tournament Entries must be validated before the tournament starts"
- "An Entry can have an InvalidState if the bowler hasn't paid"

**Implementation:** [`Entry.cs`](../../src/backend/Neba.Domain/Tournaments/Entry.cs)

**Related Terms:** [Tournament](#tournament), [Bowler](#bowler)

---

## Name

**Definition:** A value object representing a person's full name, consisting of first name and last name with validation rules.

**Context:** Shared Kernel (used across multiple bounded contexts)

**Validation Rules:**

- First name is required
- Last name is required
- First name maximum length: 30 characters
- Last name maximum length: 30 characters

**Examples:**

- Creating a name: `Name.Create("John", "Smith")`
- Error codes: `Name.FirstName.Required`, `Name.LastName.TooLong`

**Implementation:** [`Name.cs`](../../src/backend/Neba.Domain/Bowlers/Name.cs)

**Error Codes:**

- `Name.FirstName.Required` - First name is required
- `Name.FirstName.TooLong` - First name exceeds 30 characters
- `Name.LastName.Required` - Last name is required
- `Name.LastName.TooLong` - Last name exceeds 30 characters

**Related Terms:** [Bowler](#bowler), [Champion](#champion)

---

## Champion

**Definition:** A bowler who has won a specific tournament, recorded with their name, tournament details, and win date.

**Context:** Awards and History bounded context

**Examples:**

- "Jane Doe is a three-time Monthly Champion"
- "The Champions list shows all tournament winners since 1965"
- "A Champion record links to both the Bowler and the Tournament"

**Implementation:** [`Champion.cs`](../../src/backend/Neba.Domain/Awards/Champion.cs) (if exists)

**Related Terms:** [Tournament](#tournament), [Bowler](#bowler), [Award](#award)

---

## High Block

**Definition:** An award category recognizing a bowler's highest cumulative score across a specified number of consecutive games within a tournament or season.

**Context:** Awards and History bounded context

**Examples:**

- "The High Block award goes to the bowler with the highest 6-game total"
- "High Block awards can be filtered by season and tournament type"

**Implementation:** API endpoints in [`AwardsEndpoints.cs`](../../src/backend/Neba.Api/Endpoints/Website/Awards/AwardsEndpoints.cs)

**Related Terms:** [Award](#award), [Tournament](#tournament), [High Average](#high-average)

---

## Error Code

**Definition:** A structured identifier following the format `<DomainContext>.<Object>.<Member>.<Rule>` that uniquely identifies a domain validation error.

**Context:** Cross-cutting concern (all bounded contexts)

**Format:** `DomainContext.Object.Member.Rule`

**Examples:**

- `Name.FirstName.Required` - Missing required first name
- `Tournament.Entry.InvalidState` - Entry is in an invalid state
- `Membership.Id.NotFound` - Membership ID does not exist

**Standard Document:** [`Error Code Standards.md`](../../src/backend/Neba.Domain/Error%20Code%20Standards.md)

**Related Terms:** [Validation](#validation), [ErrorOr Library](#erroror-library)
