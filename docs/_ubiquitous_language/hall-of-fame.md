---
layout: default
title: Ubiquitous Language - Hall of Fame
---

# Ubiquitous Language: Hall of Fame

This document defines the core terminology used in the **Hall of Fame** feature of the NEBA Management application. These terms represent the shared language used by domain experts, developers, and stakeholders to ensure consistent understanding.

> **Note:** The Bowler entity is documented separately as it's a core concept used across multiple bounded contexts (website public pages, admin, awards, titles, etc.). The StoredFile value object is documented in the Technical Building Blocks as it's used across multiple domains.

---

## Hall of Fame Domain

### HallOfFameInduction

**Definition:** A Hall of Fame induction represents the formal recognition and honor of a NEBA member inducted into the Hall of Fame. Inductees are recognized for superior bowling performance, meritorious service to the organization, or friendship and support of NEBA. An induction can be awarded for one or more categories, and includes the year of induction and an optional photo of the inductee.

**Properties:**

- `HallOfFameId` - Unique identifier for the induction record
- `Year` - The year the induction took place (e.g., 2024, 2025)
- `Bowler` - Reference to the inducted bowler
- `Photo` - Optional photo of the inductee (StoredFile value object containing file location, content type, and metadata)
- `Categories` - Collection of one or more Hall of Fame categories for this induction (Superior Performance, Meritorious Service, Friend of NEBA)

**Business Rules:**

- **Multiple categories allowed:** A single induction can be awarded for multiple categories simultaneously
  - **Example:** A bowler could be inducted for both Superior Performance and Meritorious Service in the same year
- **Category requirement:** Every induction must have at least one category assigned
- **Photo optional:** Photos are encouraged but not required for inductions; historical inductees may not have photos available
- **Year format:** Year is stored as a numeric value (YYYY format)
- **Permanent record:** Inductions are permanent and are never deleted or revoked
- **Multiple inductions possible:** A bowler could theoretically be inducted in multiple years for different categories, though this would be rare

**Induction Timing:**

- Hall of Fame inductions are typically announced annually
- The specific timing and ceremony details are determined by NEBA leadership
- The Year property represents the formal induction year, which may differ from when achievements occurred

**Privacy Considerations:**

- Hall of Fame inductions are public information displayed on the website
- Photos, when provided, are displayed publicly on the Hall of Fame page
- Inductees consent to public recognition when inducted

**Related Terms:** Bowler, HallOfFameCategory, StoredFile

**Code Reference:** `src/backend/Neba.Website.Domain/Awards/HallOfFameInduction.cs`

---

### HallOfFameCategory

**Definition:** The types of recognition for which a NEBA member can be inducted into the Hall of Fame. Categories represent different aspects of contribution and achievement within the organization. This is a flag enumeration, meaning an induction can be awarded for multiple categories simultaneously using bitwise operations.

**Values:**

- **None** (value: 0) - No categories set. This value represents the absence of any Hall of Fame category and should not be used for actual inductions.

- **Superior Performance** (value: 1, bit 0) - Recognition for exceptional bowling achievement and competitive excellence.
  - **Criteria:** Outstanding tournament results, titles won, sustained high performance, significant statistical achievements
  - **Examples:** Multiple major tournament wins, exceptional career statistics, dominant performance over multiple seasons
  - **Note:** Specific eligibility criteria and selection process are determined by NEBA leadership

- **Meritorious Service** (value: 2, bit 1) - Recognition for significant contributions to NEBA through volunteer service, leadership, and organizational support.
  - **Criteria:** Substantial volunteer work, leadership roles, contributions to NEBA's growth and success
  - **Examples:** Long-term board service, tournament organizing, significant volunteer contributions, advancing NEBA's mission
  - **Note:** Recognizes those who have dedicated time and effort to strengthen the organization beyond their competitive participation

- **Friend of NEBA** (value: 4, bit 2) - Recognition for individuals who have supported and promoted NEBA through friendship, advocacy, and assistance.
  - **Criteria:** External supporters, sponsors, advocates, or allies who have significantly helped NEBA
  - **Examples:** Bowling center partnerships, sponsors, community advocates, supporters who promote NEBA's mission
  - **Note:** This category can include non-bowlers who have supported the organization
  - **Eligibility consideration:** May or may not require the inductee to be a Bowler entity (future consideration as Friend of NEBA inductees might not be NEBA members)

**Flag Enum Behavior:**

- **Multiple categories:** Categories can be combined using bitwise OR operations
  - **Example in code:** `SuperiorPerformance | MeritoriousService` (value: 3)
  - **Example scenario:** A champion bowler who also served on the board could be inducted for both Superior Performance and Meritorious Service
- **Category checking:** Use bitwise AND operations or SmartFlagEnum methods to test category membership
  - **Example:** Check if an induction includes Superior Performance: `categories.HasFlag(SuperiorPerformance)`
- **Display:** Multiple categories are displayed together (e.g., "Superior Performance, Meritorious Service")

**Selection Process:**

- Hall of Fame inductees and their categories are selected by NEBA leadership
- Specific selection criteria and nomination processes are organizational procedures, not enforced by the domain model
- The domain model records the decision; the business logic for eligibility exists outside the system

**Business Rules:**

- **Category requirement:** Every induction must have at least one category (cannot use None for actual inductions)
- **Valid combinations:** Any combination of the three categories is valid
- **Immutability:** Once an induction is recorded with specific categories, those categories should not change (corrections would be handled through administrative processes if errors occur)

**Related Terms:** HallOfFameInduction

**Code Reference:** `src/backend/Neba.Domain/Awards/HallOfFameCategory.cs`

---

## Aggregate Boundaries

### Bowler Aggregate (Extended for Hall of Fame)

**Aggregate Root:** Bowler

**Entities within boundary:**
- Title (child entity)
- SeasonAward (child entity)
- HallOfFameInduction (child entity) - **Added for Hall of Fame feature**

**Invariants:**

- All Hall of Fame inductions belong to this specific bowler
- Induction years must be valid (cannot be in the future, though this is not currently enforced)
- Every induction must have at least one category assigned
- Bowler identity is immutable once created

**Why this boundary?**

- The Bowler aggregate owns the collection of Hall of Fame inductions
- Transactional consistency: adding an induction to a bowler happens atomically
- Website queries: need bowler + titles + awards + Hall of Fame inductions together for comprehensive achievement displays
- The Hall of Fame is fundamentally about recognizing individual bowlers (with potential future consideration for non-bowler "Friend of NEBA" inductees)

---

## Domain Events

[TODO: Document any domain events related to Hall of Fame]

**Potential Events:**
- `BowlerInductedToHallOfFame` - When a bowler is inducted into the Hall of Fame
- `HallOfFamePhotoAdded` - When a photo is added to an induction record
- `HallOfFamePhotoUpdated` - When a photo is updated for an induction

---

## Future Considerations

### Friend of NEBA Non-Bowler Inductees

Currently, all Hall of Fame inductions are linked to a Bowler entity. However, the "Friend of NEBA" category may include individuals who are not NEBA members or bowlers:

- **Current design:** All inductees must be a Bowler
- **Future consideration:** May need to support non-bowler inductees for Friend of NEBA category
- **Potential solutions:**
  - Create a separate entity for non-bowler inductees
  - Make the Bowler reference optional and add alternative person information
  - Use a polymorphic design with a base Inductee concept

This decision will be made when a Friend of NEBA inductee who is not a bowler needs to be added.

### Induction Ceremony Details

The current model captures only the year of induction. Future enhancements might include:

- Specific induction date
- Ceremony location
- Induction speech or citation text
- Video or multimedia content from the ceremony

These details would be added as requirements are clarified.

### Selection and Nomination Process

The domain model records Hall of Fame inductions but does not enforce or manage the selection process:

- Nomination workflows
- Selection committee voting
- Eligibility criteria enforcement
- Approval processes

If these processes need to be managed in the system, they would be added to a future admin bounded context.

---

## Questions for Discussion

1. Should Friend of NEBA category support non-bowler inductees? If so, how should the model be adjusted?
2. Are there specific eligibility criteria or business rules that should be enforced by the domain model, or should all selection remain an external process?
3. Should there be limits on how many times a bowler can be inducted (e.g., once per lifetime vs. multiple times)?
4. What photo requirements exist (size, format, resolution)?
5. Should the Year property support a broader date/time value to capture specific induction dates, or is the year sufficient?
6. Can inductions be corrected or modified after they're recorded? What's the process?

---

## Related Documentation

- [Awards & Titles]({{ '/ubiquitous-language/awards-and-titles' | relative_url }}) - Related recognition and achievement concepts
- [Technical Building Blocks]({{ '/ubiquitous-language/technical-building-blocks' | relative_url }}) - Core domain infrastructure including StoredFile
- [Domain Models]({{ '/domain-models' | relative_url }}) (Coming Soon)
- [API Reference]({{ '/reference/api' | relative_url }}) (Coming Soon)
