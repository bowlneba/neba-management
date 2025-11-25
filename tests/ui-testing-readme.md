# NEBA UI Testing Overview

This README provides a high-level overview of the UI testing strategy for the NEBA Management System. It explains how Playwright, bUnit, and xUnit work together to ensure correctness, maintainability, and architectural integrity across the UI and domain layers.

This file acts as the entry point for understanding **what to test**, **where to test it**, and **why each tool is used**.

---

# 1. Testing Philosophy

The NEBA Management System follows Clean Architecture + DDD principles. UI testing is layered to match those boundaries:

- **Playwright** → real browser UI testing  
- **bUnit** → Blazor component logic testing  
- **xUnit** → domain logic testing  

Each layer tests different responsibilities. No single tool should perform all testing. This prevents duplication, flakiness, performance issues, and architectural violations.

---

# 2. Test Project Structure

All test projects live under the `tests` directory:

```
/tests
  /Neba.UnitTests              # Domain logic tests (C# + xUnit)
  /Neba.ComponentTests         # bUnit component tests (C# + bUnit)
  /browser                     # Playwright browser tests (TypeScript + npm)
```

### Rationale

- Domain should not reference UI assemblies
- UI tests should not pollute domain tests
- Playwright and bUnit serve different roles and must not be mixed
- Playwright uses TypeScript for better type safety and modern test patterns

This structure preserves boundaries and ensures long-term maintainability.

---

# 3. What Each Tool Is Responsible For

## 3.1 Playwright – Real Browser Testing

Playwright validates all behaviors requiring:

- actual layout and CSS rendering
- responsive behavior and breakpoints
- real browser events (hover, focus, scroll, touch)
- multi-step workflows
- cross-browser correctness

Examples include:

- navbar collapse and overflow prevention
- toast placement (desktop vs mobile)
- score entry workflows
- modal, drawer, and navigation interactions

Playwright is the only tool that can test how the UI behaves **in a real browser**.

### Setup and Running

```bash
# Initial setup
cd tests/browser
npm install
npx playwright install

# Run tests
npx playwright test                    # Run all tests
npx playwright test --headed           # See the browser
npx playwright test --ui              # Interactive UI mode
npx playwright show-report            # View HTML report
```

Full details:
See `ui-testing-playwright.instructions.md`.

---

## 3.2 bUnit – Blazor Component Logic Testing

bUnit validates component-level logic:

- branching and conditional rendering  
- input validation  
- parameter handling  
- EventCallback behavior  
- internal component state transitions  
- interactions with mocked services  

Examples include:

- button disabled → enabled transitions  
- validation message rules  
- correct markup fragments for component states  

bUnit does **not** evaluate CSS, layout, or browser behavior.

Full details:  
See `ui-testing-bunit.instructions.md`.

---

## 3.3 xUnit – Domain Logic Testing

xUnit covers logic inside the domain layer:

- entities  
- aggregates  
- value objects  
- validation rules  
- factory behavior  
- domain services  

No Blazor dependencies. No UI logic. Pure domain.

---

# 4. Deciding Where a Test Belongs

Use this chart:

| Scenario | Tool |
|---------|------|
| Responsive layout | Playwright |
| Hamburger menu collapse | Playwright |
| Toast positioning | Playwright |
| Multi-step registration flow | Playwright |
| Scroll/overflow rules | Playwright |
| Component parameter logic | bUnit |
| Component validation logic | bUnit |
| EventCallback behavior | bUnit |
| Component shows/hides sections | bUnit |
| Domain entity invariants | xUnit |
| Domain validation rules | xUnit |

If the behavior depends on **browser rendering**, it belongs to Playwright.  
If it depends on **component logic**, it belongs to bUnit.  
If it depends on **domain rules**, it belongs to xUnit.

---

# 5. Test Naming & Structure Standards

All tests follow the naming pattern:

```
Method_ShouldExpectedResult_WhenCondition
```

Examples:

- `Render_ShouldShowError_WhenNameMissing`
- `Submit_ShouldNavigateToDashboard_WhenCredentialsValid`
- `CreateTournament_ShouldThrow_WhenDateInvalid`

This ensures clarity and consistency across test layers.

---

# 6. CI Strategy

CI runs tests in the following order:

1. **xUnit domain tests** – fastest feedback  
2. **bUnit component tests** – fast and isolated  
3. **Playwright browser tests** – slower, scenario-based  

PR pipeline:
- Chromium + desktop viewport  
- No mobile by default  

Nightly pipeline:
- Chromium  
- Firefox  
- WebKit  
- Mobile emulation  
- Full scenario suite  

Playwright failures automatically generate:
- traces  
- videos  
- console logs  
- network logs  

---

# 7. Additional Resources

- `ui-testing.instructions.md`: Full strategy document  
- `ui-testing-playwright.instructions.md`: Playwright-specific rules  
- `ui-testing-bunit.instructions.md`: bUnit-specific rules  

---

This README serves as the central reference for all UI test strategy decisions in the NEBA Management System.
