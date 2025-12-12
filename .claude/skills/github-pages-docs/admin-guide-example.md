# Example: Admin Guide Documentation

This file provides an example of how to document an administrative task for the NEBA Management System.

---

# Managing Tournament Awards

## Overview
This guide explains how to manage tournament awards in the NEBA Management System, including viewing award recipients, filtering by season and tournament type, and updating award information.

## Prerequisites
- Admin access to the NEBA Management System
- Understanding of NEBA award categories (High Block, High Average, Bowler of the Year, etc.)
- Familiarity with tournament types (Monthly, Open, Senior, etc.)

## Award Categories

The system supports several award categories:

### High Block
The highest cumulative score across a specified number of consecutive games (typically 6 games) within a tournament.

### High Average
The highest average score across all games in a season or tournament.

### Bowler of the Year
Awarded annually to the top bowler in each category (Open, Senior, Super Senior, Woman, Youth, Rookie).

### Hall of Fame
Recognition for long-term achievement and contribution to NEBA.

## Viewing Awards

### 1. Access the Awards Dashboard
1. Log in to the admin portal at `/admin`
2. Navigate to **Awards** in the left sidebar
3. Select the award category you want to manage

### 2. Filter Awards

You can filter awards by:

- **Season/Year:** Select from the dropdown to view awards for a specific year
- **Tournament Type:** Filter by Monthly, Open, Senior, Super Senior, etc.
- **Award Category:** High Block, High Average, Bowler of the Year

**Example:**

To view all High Block winners from 2023 Monthly tournaments:

1. Select **High Block** from the award category menu
2. Choose **2023** from the year dropdown
3. Select **Monthly** from the tournament type filter

### 3. Review Award Details

The awards list displays:

- Bowler name (clickable to view full profile)
- Tournament or season
- Score or average
- Date awarded
- Award type

## Managing Award Records

### Adding a New Award

1. Navigate to the specific award category
2. Click the **Add Award** button
3. Fill in the required fields:
   - **Bowler Name:** Start typing to search existing bowlers
   - **Tournament/Season:** Select from dropdown
   - **Score/Average:** Enter the qualifying score
   - **Date:** Award date
   - **Award Type:** Select the specific award
4. Click **Save**

### Editing an Award

1. Locate the award in the list
2. Click the **Edit** icon (pencil) next to the award
3. Update the necessary fields
4. Click **Save Changes**

### Deleting an Award

> **Warning:** Deleting an award is permanent and cannot be undone. Ensure you have verified this action is necessary.

1. Locate the award in the list
2. Click the **Delete** icon (trash can)
3. Confirm the deletion in the popup dialog
4. The award will be removed immediately

## Verification

After adding or editing an award, verify the changes:

1. Navigate to the public website at `/history/awards`
2. Filter to the relevant category and year
3. Confirm the award appears correctly
4. Click on the bowler name to ensure the profile link works

## Troubleshooting

### Issue: Bowler Not Found When Adding Award

**Cause:** The bowler hasn't been added to the system yet
**Solution:**

1. Navigate to **Bowlers** > **Add Bowler**
2. Create the bowler profile first
3. Return to awards and search for the bowler again

### Issue: Score Doesn't Save

**Cause:** The score format is incorrect or out of range
**Solution:**

- Ensure the score is a whole number
- Verify the score is within valid bowling score range (0-300 per game)
- For high block awards, ensure the total is reasonable for the number of games

### Issue: Award Doesn't Appear on Public Website

**Cause:** Caching delay or data sync issue
**Solution:**
1. Wait 5-10 minutes for cache to refresh
2. Try clearing browser cache and refreshing
3. Check the API endpoint `/api/website/awards/high-block` to verify data is present
4. Contact technical support if issue persists

## Related Tasks

- [Managing Bowler Profiles](./bowler-management.md)
- [Setting Up Tournaments](./tournament-setup.md)
- [Generating Award Reports](./award-reports.md)
- [Understanding Ubiquitous Language](../domain/ubiquitous-language.md)

## API Reference

For developers integrating with the awards system:

**List High Block Awards:**

```http
GET /api/website/awards/high-block
```

**Response Example:**

```json
{
  "awards": [
    {
      "bowlerName": "John Smith",
      "score": 1425,
      "season": "2023",
      "tournamentType": "Monthly",
      "date": "2023-12-15"
    }
  ]
}
```

See the [API documentation](../api/awards-endpoints.md) for complete reference.
