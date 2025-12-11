# Admin Layout Specification

> **Status**: Planning - Not Yet Implemented
> **Last Updated**: December 2024
> **Related**: [UI-DESIGN-GUIDELINES.md](UI-DESIGN-GUIDELINES.md), [LAYOUT-DISCUSSION.md](LAYOUT-DISCUSSION.md)

This document provides the complete specification for implementing the Admin layout system for NEBA.

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture Decision](#architecture-decision)
3. [Authentication & Authorization](#authentication--authorization)
4. [Navigation Structure](#navigation-structure)
5. [Layout Specifications](#layout-specifications)
6. [Route Structure](#route-structure)
7. [Technical Implementation](#technical-implementation)
8. [Real-time Updates](#real-time-updates)

---

## Overview

### Two-Layout System

**MainLayout** (Public-facing, mobile-first)

- Public website content
- Viewing stats, history, news, tournaments
- User profile features (future)
- Routes: `/`, `/tournaments`, `/stats`, `/history/*`, etc.

**AdminLayout** (Management, desktop-first)

- Tournament management software
- Content management system
- Bowler/membership management
- Routes: `/admin/*`

### Key Principles

1. **Single unified API** - No separate public/admin APIs
2. **Single login endpoint** - JWT/cookie-based authentication
3. **Role-based access control** - Granular roles, policy-based permissions when needed
4. **Seamless navigation** - Users with admin roles can navigate between public and admin seamlessly

---

## Architecture Decision

**Selected: Option A - Single Admin Layout**

One unified `AdminLayout.razor` for all administrative tasks (tournament management, CMS, etc.).

**Rationale**:

- Simplest to implement and maintain
- Same users often perform both tournament and content tasks
- Role-based visibility provides sufficient separation
- Route organization (`/admin/tournaments/*`, `/admin/content/*`) keeps concerns organized
- Can always split later if workflows diverge significantly

---

## Authentication & Authorization

### Authentication Strategy

**Single Login Page**: `/login`

- Lives in `MainLayout` (public layout)
- Single API endpoint for authentication
- Returns JWT token and/or cookie
- Redirects based on:
  - Intended destination (if navigated from deep link)
  - User roles (admin users might auto-redirect to `/admin`)
  - Default: redirect to `/` (home)

### User Roles (Initial Set)

| Role | Access Level | Permissions |
|------|-------------|-------------|
| `Member` | Public + Personal | Future: Update profile, register for tournaments, view personal stats |
| `TournamentDirector` | Public + Tournament Management | Tournaments, scoring, bowler management, awards |
| `ContentEditor` | Public + Content Management | News articles, sponsors, centers, general content |
| `WebMaster` | Public + Full Content + Config | All content management, site settings (not tournament operations) |
| `GlobalAdministrator` | Full System Access | Everything - all admin sections, all operations |


**Role Hierarchy** (for default authorization policies):

```
GlobalAdministrator > WebMaster + TournamentDirector > ContentEditor > Member
```


**Note**: Roles are not mutually exclusive. A user can have multiple roles:
- Example: User with both `TournamentDirector` and `ContentEditor` sees all tournament and content sections


### Policy-Based Permissions (Future)

For fine-grained access control (e.g., financial reporting, sensitive data):

```csharp
[Authorize(Policy = "CanViewFinancialReports")]
[Authorize(Policy = "CanManageUserRoles")]
[Authorize(Policy = "CanDeleteTournaments")]
```

Create policies as needed when role-based access is too coarse.

### Authorization Scenarios

**Scenario 1: Public User (No Login)**
- Views public site with `MainLayout`
- No "Admin" option visible
- `/admin/*` routes redirect to `/login?returnUrl=/admin/...`

**Scenario 2: Authenticated Member (No Admin Roles)**
- Views public site with `MainLayout`
- Sees user dropdown with profile options (future)
- No "Admin" menu item
- `/admin/*` routes return 403 Forbidden

**Scenario 3: Authenticated User with Admin Role(s)**
- Views public site with `MainLayout`
- Sees "Admin Panel" in user dropdown
- Can navigate to `/admin` → switches to `AdminLayout`
- Sees only admin sections permitted by their roles
- Can navigate back to public site via "Back to Public Site" link

**Scenario 4: Direct Admin Navigation**
- User navigates to `/admin/tournaments` (bookmark, etc.)
- If not authenticated → redirect to `/login?returnUrl=/admin/tournaments`
- If authenticated but no admin roles → 403 Forbidden page
- If authenticated with appropriate role → `AdminLayout` with appropriate sections visible

---

## Navigation Structure

### MainLayout - Public Navigation

**Top Navigation Bar** (Current implementation in `MainLayout.razor`):

```
[NEBA Logo] Tournaments | Stats | About | News | History▼ | Hall of Fame | Sponsors | Centers
                                                                         [Search] [User▼]
```

**User Dropdown** (Not yet implemented):


**When NOT logged in**:

```
[Login]
```


**When logged in (Member role only)**:

```
👤 John Smith ▼
  ├─ My Profile (future)
  ├─ My Stats (future)
  ├─ My Tournaments (future)
  ├─ ─────────────
  └─ Logout
```


**When logged in (with any admin role)**:

```
👤 John Smith ▼
  ├─ My Profile (future)
  ├─ My Stats (future)
  ├─ My Tournaments (future)
  ├─ ─────────────
  ├─ Admin Panel ← navigates to /admin
  ├─ ─────────────
  └─ Logout
```

### AdminLayout - Sidebar Navigation

**Layout Structure**:

```
┌──────────────────────────────────────────────────────────┐
│ NEBA Admin                   [User: John Smith ▼] [🔔]   │ ← Top bar
├─────────────┬────────────────────────────────────────────┤
│             │                                            │
│  Sidebar    │         Main Content Area                  │
│  Navigation │         (page content here)                │
│             │                                            │
│             │                                            │
└─────────────┴────────────────────────────────────────────┘
```


**Sidebar Navigation** (Role-based visibility):

```
≡ NEBA Admin

📊 Dashboard
  (visible to all admin roles)

🎳 Tournaments
  (TournamentDirector, GlobalAdministrator)
  ├─ All Tournaments
  ├─ Create Tournament
  ├─ Live Scoring
  └─ Bowler Management

✏️ Content
  (ContentEditor, WebMaster, GlobalAdministrator)
  ├─ News Articles
  ├─ Sponsors
  └─ Bowling Centers

🏆 Awards
  (TournamentDirector, GlobalAdministrator)
  ├─ Season Awards
  └─ Hall of Fame

⚙️ Settings
  (WebMaster, GlobalAdministrator)
  ├─ Site Settings
  └─ User Management (GlobalAdministrator only)

─────────────

← Back to Public Site
```

**Visibility Rules**:
- Items are **hidden** if user lacks required role(s)
- If user has no permissions for a top-level section, the entire section is hidden
- Dashboard is always visible to anyone with any admin role
- "Back to Public Site" is always visible


**Example: ContentEditor Role Sees**:

```
≡ NEBA Admin

📊 Dashboard

✏️ Content
  ├─ News Articles
  ├─ Sponsors
  └─ Bowling Centers

─────────────

← Back to Public Site
```


**Top Bar (Admin)**:

```
NEBA Admin                               [Search?] [Notifications] [User: John Smith ▼]
```


**User Dropdown (in Admin)**:

```
👤 John Smith
  Role: Tournament Director
  ─────────────
  ├─ My Profile
  ├─ ─────────────
  ├─ Back to Public Site
  └─ Logout
```

---

## Layout Specifications

### AdminLayout.razor Structure

**File Location**: `/src/frontend/Neba.Web.Server/Layout/AdminLayout.razor`

**Key Components**:

1. **Top Bar**
   - NEBA Admin branding/logo
   - Search (optional, future)
   - Notification bell (admin-specific notifications)
   - User dropdown

2. **Sidebar Navigation**
   - Collapsible (desktop: expanded by default, can collapse)
   - Fixed position on desktop
   - Off-canvas on tablet/mobile
   - Role-based section visibility
   - Active route highlighting
   - Icon + label navigation items

3. **Main Content Area**
   - Breadcrumbs (for deep navigation)
   - Page title
   - Content outlet (`@Body`)
   - Toast/alert notifications (admin-specific)

4. **Desktop-First Responsive Behavior**
   - **Desktop (1024px+)**: Sidebar visible, ~250px width
   - **Tablet (768px-1023px)**: Sidebar collapsible, ~200px when open
   - **Mobile (<768px)**: Sidebar off-canvas (hamburger menu)

### Design Characteristics

**Desktop-First Optimized**:
- Dense information display
- Multi-column layouts
- Data tables with sorting/filtering
- Keyboard shortcuts support
- Quick actions/toolbars
- Inline editing
- Drag-and-drop support

**Color Scheme**:
- Slightly darker/different theme than public site to differentiate
- Use NEBA blue (`--neba-blue-700`) as accent
- Admin-specific grays for sidebar/chrome
- Clear visual distinction: "I'm in the admin panel"

---

## Route Structure

### Public Routes (MainLayout)

```
/                              → Home page
/login                         → Login page (serves both public and admin users)
/tournaments                   → Tournament listings (public view)
/tournaments/{id}              → Tournament details (public view)
/tournaments/{id}/live         → Live scoring display (public, SSE)
/stats                         → Statistics
/history/champions             → Champions history
/history/bowler-of-the-year    → BOTY awards
/history/high-average          → High average awards
/history/high-block            → High block awards
/hall-of-fame                  → Hall of Fame
/news                          → News articles
/news/{slug}                   → Article detail
/about                         → About NEBA
/sponsors                      → Sponsors
/centers                       → Bowling centers
/contact                       → Contact
/privacy                       → Privacy policy
/terms                         → Terms of service

# Future: User portal
/my/dashboard                  → Personal dashboard
/my/profile                    → Edit profile
/my/stats                      → Personal stats
/my/tournaments                → My tournaments
/my/registrations              → Tournament registrations
```

### Admin Routes (AdminLayout)

```
/admin                         → Admin dashboard
                                  [Authorize(Roles = "TournamentDirector,ContentEditor,WebMaster,GlobalAdministrator")]

# Tournament Management
/admin/tournaments             → Tournament list
                                  [Authorize(Roles = "TournamentDirector,GlobalAdministrator")]
/admin/tournaments/create      → Create tournament
/admin/tournaments/{id}        → Edit tournament
/admin/tournaments/{id}/overview
/admin/tournaments/{id}/entries
/admin/tournaments/{id}/settings

# Live Scoring
/admin/scoring                 → Active tournaments for scoring
                                  [Authorize(Roles = "TournamentDirector,GlobalAdministrator")]
/admin/scoring/{id}            → Live scoring interface (SignalR)
/admin/scoring/{id}/entries    → Manage entries for scoring

# Bowler Management
/admin/bowlers                 → Bowler list
                                  [Authorize(Roles = "TournamentDirector,GlobalAdministrator")]
/admin/bowlers/create          → Create bowler
/admin/bowlers/{id}            → Edit bowler
/admin/bowlers/import          → Import bowlers (CSV, etc.)

# Content Management
/admin/content/news            → News articles list
                                  [Authorize(Roles = "ContentEditor,WebMaster,GlobalAdministrator")]
/admin/content/news/create     → Create news article
/admin/content/news/{id}       → Edit news article

/admin/content/sponsors        → Sponsors list
                                  [Authorize(Roles = "ContentEditor,WebMaster,GlobalAdministrator")]
/admin/content/sponsors/create → Create sponsor
/admin/content/sponsors/{id}   → Edit sponsor

/admin/content/centers         → Bowling centers list
                                  [Authorize(Roles = "ContentEditor,WebMaster,GlobalAdministrator")]
/admin/content/centers/create  → Create center
/admin/content/centers/{id}    → Edit center

# Awards
/admin/awards                  → Awards overview
                                  [Authorize(Roles = "TournamentDirector,GlobalAdministrator")]
/admin/awards/season/{year}    → Season awards for year
/admin/awards/hall-of-fame     → Hall of Fame management

# Settings
/admin/settings                → Site settings
                                  [Authorize(Roles = "WebMaster,GlobalAdministrator")]
/admin/settings/users          → User management
                                  [Authorize(Roles = "GlobalAdministrator")]
/admin/settings/roles          → Role management
                                  [Authorize(Roles = "GlobalAdministrator")]
```

---

## Technical Implementation

### Layout Assignment

**Option 1: Explicit Layout Declaration** (Recommended for clarity)

Each admin page explicitly declares its layout and authorization:

```razor
@page "/admin/tournaments"
@layout AdminLayout
@attribute [Authorize(Roles = "TournamentDirector,GlobalAdministrator")]

<h1>Tournament Management</h1>
```

**Option 2: Route-Based Layout Assignment**

Configure in `App.razor`:

```razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@GetLayoutForRoute(routeData)">
            <NotAuthorized>
                @if (context.User.Identity?.IsAuthenticated == true)
                {
                    <p>You do not have permission to access this page.</p>
                }
                else
                {
                    <RedirectToLogin />
                }
            </NotAuthorized>
        </AuthorizeRouteView>
    </Found>
    <NotFound>
        <PageTitle>Not Found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <NotFound />
        </LayoutView>
    </NotFound>
</Router>

@code {
    private Type GetLayoutForRoute(RouteData routeData)
    {
        var template = routeData.PageType.GetCustomAttribute<RouteAttribute>()?.Template ?? "";
        return template.StartsWith("/admin") ? typeof(AdminLayout) : typeof(MainLayout);
    }
}
```

### Role-Based Navigation Visibility

**Component: `AdminSidebar.razor`**

```razor
@inject AuthenticationStateProvider AuthStateProvider

@if (HasRole("TournamentDirector", "GlobalAdministrator"))
{
    <div class="nav-section">
        <h3>🎳 Tournaments</h3>
        <ul>
            <li><a href="/admin/tournaments">All Tournaments</a></li>
            <li><a href="/admin/tournaments/create">Create Tournament</a></li>
            <li><a href="/admin/scoring">Live Scoring</a></li>
            <li><a href="/admin/bowlers">Bowler Management</a></li>
        </ul>
    </div>
}

@if (HasRole("ContentEditor", "WebMaster", "GlobalAdministrator"))
{
    <div class="nav-section">
        <h3>✏️ Content</h3>
        <ul>
            <li><a href="/admin/content/news">News Articles</a></li>
            <li><a href="/admin/content/sponsors">Sponsors</a></li>
            <li><a href="/admin/content/centers">Bowling Centers</a></li>
        </ul>
    </div>
}

@code {
    private ClaimsPrincipal? user;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        user = authState.User;
    }

    private bool HasRole(params string[] roles)
    {
        if (user == null) return false;
        return roles.Any(role => user.IsInRole(role));
    }
}
```

### Authentication Service (Conceptual)

```csharp
public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    bool IsAuthenticated();
    bool IsInRole(string role);
    bool HasAnyRole(params string[] roles);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public UserInfo? User { get; set; }
    public string? ErrorMessage { get; set; }
}

public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
```

### API Authentication Endpoint

```
POST /api/auth/login
Content-Type: application/json

{
  "username": "jsmith",
  "password": "********"
}

Response 200 OK:
{
  "success": true,
  "token": "eyJhbGc...",
  "user": {
    "id": 123,
    "username": "jsmith",
    "displayName": "John Smith",
    "email": "john@example.com",
    "roles": ["TournamentDirector", "ContentEditor"]
  }
}

Response 401 Unauthorized:
{
  "success": false,
  "errorMessage": "Invalid username or password"
}
```

**Authentication Method**: JWT token and/or secure HTTP-only cookie
- Decision to be made during implementation
- JWT for API calls
- Cookie for session persistence

---

## Real-time Updates

### Admin Scoring (SignalR)

**Use Case**: Multiple tournament directors entering scores simultaneously

**Technology**: SignalR (bi-directional, WebSocket-based)

**Hub**: `ScoringHub`

**Features**:
- Real-time score updates across all connected clients
- Presence awareness (see who else is scoring)
- Optimistic updates with conflict resolution
- Edit locks (optional: "User X is editing entry Y")
- Notifications for completed games, errors, etc.

**Client Operations**:
- `UpdateScore(entryId, gameNumber, score)` → broadcasts to all admin clients
- `JoinTournament(tournamentId)` → subscribe to tournament updates
- `LeaveTournament(tournamentId)` → unsubscribe

**Server Events**:
- `OnScoreUpdated(entryId, gameNumber, score, updatedBy)`
- `OnEntryCompleted(entryId)`
- `OnUserJoined(username, tournamentId)`
- `OnUserLeft(username, tournamentId)`

### Public Scoring Display (SSE)

**Use Case**: Many viewers watching live tournament scores

**Technology**: Server-Sent Events (one-way, HTTP-based)

**Endpoint**: `GET /api/tournaments/{id}/live-scores`

**Features**:
- One-way updates from server to clients
- Lightweight for many concurrent viewers
- Automatic reconnection
- Lower overhead than SignalR for read-only scenarios

**Event Stream**:
```
event: score-updated
data: {"entryId": 123, "gameNumber": 1, "score": 245}

event: entry-completed
data: {"entryId": 123, "totalPins": 735}

event: standings-updated
data: {"standings": [...]}
```

**Data Flow**:
```
Admin (SignalR) → ScoringHub → Database
                       ↓
                  SSE Stream → Public Viewers
```

---

## File Structure

```
src/frontend/Neba.Web.Server/
├── Layout/
│   ├── MainLayout.razor           # Existing public layout
│   ├── AdminLayout.razor          # NEW: Admin layout
│   ├── AdminLayout.razor.css      # NEW: Admin layout styles
│   └── ReconnectModal.razor       # Existing
├── Components/
│   ├── Admin/                     # NEW: Admin-specific components
│   │   ├── AdminSidebar.razor
│   │   ├── AdminTopBar.razor
│   │   ├── AdminUserDropdown.razor
│   │   └── Breadcrumbs.razor
│   ├── Auth/                      # NEW: Auth components
│   │   ├── LoginForm.razor
│   │   ├── UserDropdown.razor     # For MainLayout
│   │   └── RequireRole.razor      # Conditional rendering by role
│   └── [existing components...]
├── Pages/
│   ├── Login.razor                # NEW: Login page
│   ├── Admin/                     # NEW: Admin pages
│   │   ├── Index.razor            # Dashboard
│   │   ├── Tournaments/
│   │   │   ├── Index.razor
│   │   │   ├── Create.razor
│   │   │   └── Edit.razor
│   │   ├── Scoring/
│   │   │   ├── Index.razor
│   │   │   └── Live.razor
│   │   ├── Content/
│   │   │   ├── News/
│   │   │   ├── Sponsors/
│   │   │   └── Centers/
│   │   └── Settings/
│   └── [existing pages...]
├── Services/
│   ├── AuthenticationService.cs  # NEW: Auth service
│   └── [existing services...]
└── wwwroot/
    ├── neba_theme.css            # Existing theme
    └── admin_theme.css           # NEW: Admin-specific theme additions
```

---

## Implementation Phases

### Phase 1: Foundation
1. Create `AdminLayout.razor` with basic structure
2. Implement authentication service
3. Create login page (`/login`)
4. Set up role-based authorization
5. Add user dropdown to `MainLayout` with "Admin Panel" link

### Phase 2: Admin Shell
1. Build admin sidebar navigation with role-based visibility
2. Create admin dashboard (`/admin`)
3. Implement breadcrumb navigation
4. Add admin-specific styling/theme

### Phase 3: Tournament Management
1. Tournament list page (`/admin/tournaments`)
2. Create/edit tournament forms
3. Bowler management pages
4. Awards management

### Phase 4: Live Scoring
1. Set up SignalR hub for admin scoring
2. Build live scoring interface (`/admin/scoring/{id}`)
3. Implement SSE endpoint for public scoring display
4. Create public live scoring page (`/tournaments/{id}/live`)

### Phase 5: Content Management
1. News article management
2. Sponsor management
3. Bowling center management

### Phase 6: Settings & User Management
1. Site settings page
2. User management (GlobalAdministrator only)
3. Role management

---

## Design Mockup Notes

### Admin Sidebar Styling

- **Background**: Slightly darker than main content (`--neba-gray-050` or custom admin gray)
- **Active item**: NEBA blue (`--neba-blue-700`) left border + background tint
- **Hover**: Subtle background change
- **Icons**: Consistent icon set (consider using Heroicons or Bootstrap Icons)
- **Typography**: Clear hierarchy, section headers in uppercase/small text

### Admin Top Bar

- **Height**: 60px (consistent with public top banner height)
- **Background**: White or light gray, subtle bottom border
- **Right side**: Notification bell + user dropdown
- **Left side**: "NEBA Admin" branding + collapse sidebar button (mobile)

### Responsive Breakpoints (Desktop-First)

```css
/* Desktop default (1024px+) */
.admin-layout {
  display: grid;
  grid-template-columns: 250px 1fr;
}

.admin-sidebar {
  position: fixed;
  left: 0;
  top: 60px;
  width: 250px;
  height: calc(100vh - 60px);
  overflow-y: auto;
}

/* Tablet (768px - 1023px) */
@media (max-width: 1023px) {
  .admin-layout {
    grid-template-columns: 200px 1fr;
  }

  .admin-sidebar {
    width: 200px;
  }

  .admin-sidebar.collapsed {
    transform: translateX(-200px);
  }
}

/* Mobile (<768px) */
@media (max-width: 767px) {
  .admin-layout {
    grid-template-columns: 1fr;
  }

  .admin-sidebar {
    position: fixed;
    left: 0;
    top: 60px;
    width: 280px;
    transform: translateX(-280px);
    transition: transform 0.3s ease;
    z-index: 100;
  }

  .admin-sidebar.open {
    transform: translateX(0);
  }

  .sidebar-backdrop {
    /* Overlay for mobile when sidebar open */
  }
}
```

---

## Security Considerations

### Authorization Layers

1. **Route-level**: `[Authorize(Roles = "...")]` on pages
2. **Component-level**: Role checks in `AdminSidebar` for visibility
3. **API-level**: Authorization on all API endpoints (separate from UI auth)

### Best Practices

- **Never trust client-side authorization** - Always enforce on API
- **JWT tokens**: Short expiration (15-30 min), refresh token pattern
- **HTTPS only**: Enforce in production
- **CSRF protection**: Anti-forgery tokens for state-changing operations
- **Audit logging**: Log admin actions (who did what, when)
- **Role changes**: Require re-authentication if user roles change

---

## Future Enhancements

### Potential Features (Not Initial Scope)

1. **Activity Feed**: Recent actions in admin dashboard
2. **Notifications**: In-app notifications for admins (new tournament entries, etc.)
3. **Search**: Global admin search for bowlers, tournaments, content
4. **Keyboard Shortcuts**: Power user shortcuts (e.g., `Ctrl+K` for command palette)
5. **Dark Mode**: Admin theme variant
6. **Multi-language**: i18n support
7. **Audit Log Viewer**: UI for viewing admin action history
8. **Batch Operations**: Bulk edit bowlers, tournaments, etc.
9. **Advanced Permissions**: Field-level permissions, custom policies per organization

---

## Questions / Decisions Pending

- [ ] JWT vs. Cookie vs. Both for authentication?
- [ ] Session timeout duration?
- [ ] Refresh token strategy?
- [ ] Audit logging requirements?
- [ ] Profile image/avatar support?
- [ ] Email notifications for admin actions?
- [ ] Two-factor authentication (2FA) for admin users?

---

## References

- [UI-DESIGN-GUIDELINES.md](UI-DESIGN-GUIDELINES.md) - Overall UI design principles
- [LAYOUT-DISCUSSION.md](LAYOUT-DISCUSSION.md) - Layout options discussion
- [NOTIFICATIONS.md](NOTIFICATIONS.md) - Notification system (applies to admin too)
- [LOADING.md](LOADING.md) - Loading states (applies to admin too)

---

**This document is a living specification. Update as requirements evolve or implementation details are decided.**
