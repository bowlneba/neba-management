# NEBA Layout Architecture Discussion

> **Status**: ✅ Decision Made - See [ADMIN-LAYOUT-SPEC.md](ADMIN-LAYOUT-SPEC.md) for full implementation spec
> **Decision**: Option A - Single Admin Layout with role-based navigation

## Summary

The UI guidelines have been updated to reflect the dual-audience approach:

- **Public-Facing** (mobile-first): Current website functionality - viewing stats, history, news
- **Management/Admin** (desktop-first): Tournament management software and content management system

## Layout Options

### Option A: Single Admin Layout ⭐ **RECOMMENDED**

**One unified `AdminLayout.razor` for all management tasks.**

#### Why This Makes Sense

1. **Same Users**: People running tournaments are likely the same people updating website content
2. **Start Simple**: Can always split later if workflows diverge significantly
3. **Consistency**: Unified admin experience, same navigation paradigm
4. **Route Organization**: Use URL structure to separate concerns:
   - `/admin/tournaments/` - Tournament management
   - `/admin/scoring/` - Live scoring entry
   - `/admin/content/` - CMS (news, sponsors, etc.)
   - `/admin/bowlers/` - Bowler management

#### Admin Layout Features

- **Sidebar navigation** (desktop-optimized)
- **Top bar** with user info, quick actions
- **Breadcrumbs** for deep navigation
- **Dense layouts** - more information on screen
- **Keyboard shortcuts** for power users
- **Notification center** for admin alerts

---

### Option B: Separate Tournament & CMS Layouts

**Two layouts: `TournamentLayout.razor` and `ContentManagementLayout.razor`**

#### When to Consider This

- If tournament software needs specialized UI (live timers, scoring displays)
- If workflows are drastically different between tournament day and content editing
- If different user roles access each system (tournament directors vs. web admins)

#### Trade-offs

- ✅ Each layout optimized for specific workflow
- ✅ Tournament layout can be more specialized
- ❌ More code to maintain
- ❌ Users switching contexts see different UIs
- ❌ Harder to maintain consistency

---

### Option C: Dynamic Sidebar Layout

**One `AdminLayout.razor` with contextual sidebar navigation**

#### How It Works

- Base admin layout stays consistent
- Sidebar navigation changes based on route:
  - In `/admin/tournaments/*` → Shows tournament-specific nav
  - In `/admin/content/*` → Shows CMS-specific nav

#### Trade-offs

- ✅ Maintains layout consistency
- ✅ Contextual navigation
- ✅ Easy to add new admin sections
- ❌ More complex layout logic
- ❌ Could be confusing if sections feel disconnected

---

## Proposed Route Structure

### Public Routes (MainLayout)

```text
/                           → Home page
/tournaments                → Tournament listings
/tournaments/{id}           → Tournament details
/stats                      → Statistics
/history/champions          → Champions history
/history/bowler-of-the-year → BOTY awards
/history/high-average       → High average awards
/history/high-block         → High block awards
/hall-of-fame               → Hall of Fame
/news                       → News articles
/news/{slug}                → Article detail
/about                      → About NEBA
/sponsors                   → Sponsors
/centers                    → Bowling centers
```

### Admin Routes (AdminLayout)

```text
/admin                      → Admin dashboard
/admin/tournaments          → Tournament list/management
/admin/tournaments/create   → Create tournament
/admin/tournaments/{id}     → Edit tournament
/admin/scoring              → Live scoring (replaces WinForms)
/admin/scoring/{id}         → Active tournament scoring
/admin/content/news         → Manage news articles
/admin/content/sponsors     → Manage sponsors
/admin/content/centers      → Manage bowling centers
/admin/bowlers              → Bowler management
/admin/bowlers/{id}         → Edit bowler
/admin/awards               → Awards management
/admin/settings             → System settings
```

### Future: User Portal Routes (MainLayout or UserPortalLayout)

```text
/my/dashboard               → Personal dashboard
/my/profile                 → Edit profile
/my/stats                   → Personal stats
/my/tournaments             → My tournaments
/my/registrations           → Tournament registrations
```

---

## Design Philosophy by Audience

### Public-Facing (Mobile-First)

**Philosophy**: Content consumption on any device

- Large touch targets (44px minimum)
- Generous spacing
- Single column layouts on mobile
- Simple navigation
- Fast loading
- Optimized for reading

**Responsive Breakpoints**:

```css
/* Mobile (default) */
.card { padding: 1rem; }

/* Tablet (768px+) */
@media (min-width: 768px) {
  .card { padding: 1.5rem; }
}

/* Desktop (1024px+) */
@media (min-width: 1024px) {
  .card { padding: 2rem; }
}
```

### Admin/Management (Desktop-First)

**Philosophy**: Productivity and efficiency for desktop users

- Dense information display
- Multi-column layouts by default
- Keyboard shortcuts
- Drag-and-drop
- Quick actions
- Inline editing
- Data tables with sorting/filtering

**Responsive Breakpoints**:

```css
/* Desktop (default) - optimized for 1024px+ */
.admin-grid {
  display: grid;
  grid-template-columns: 250px 1fr;
}

/* Tablet (768px-1023px) */
@media (max-width: 1023px) {
  .admin-grid {
    grid-template-columns: 200px 1fr;
  }
}

/* Mobile (optional fallback) */
@media (max-width: 767px) {
  .admin-grid {
    grid-template-columns: 1fr;
  }
}
```

---

## Next Steps

1. **Confirm layout strategy**: Which option (A, B, or C)?
2. **Define admin navigation structure**: Sidebar? Top nav? Hybrid?
3. **Create AdminLayout.razor**: Basic structure
4. **Design admin theme**: Desktop-optimized styling
5. **Plan authentication/authorization**: Who can access admin routes?

---

## Questions to Consider

1. **User Roles**: Are there different admin roles (tournament director vs. web editor)?
2. **Navigation Style**: Sidebar (persistent, collapsible) or top navigation tabs?
3. **Tournament Software Needs**: Does live scoring need full-screen mode? Special displays?
4. **Mobile Admin Access**: Will admins ever need to manage content from mobile devices?
5. **Route Prefix**: Prefer `/admin/*` or `/manage/*` or something else?

---

## Recommendation

**Start with Option A (Single Admin Layout)** because:

1. It's the simplest to implement and maintain
2. You can always split later if needed
3. The same users will likely use both tournament and CMS features
4. Route-based organization provides enough separation
5. Consistent UI reduces cognitive load

**Implementation Priority**:

1. Create basic `AdminLayout.razor` with sidebar navigation
2. Add authentication/authorization
3. Build tournament management pages first (highest value)
4. Add CMS pages for content management
5. Iterate based on real usage patterns

If you find that tournament day operations need a completely different UI (e.g., full-screen scoring displays), you can create a specialized `TournamentScoringLayout.razor` just for that while keeping everything else in the unified admin layout.
