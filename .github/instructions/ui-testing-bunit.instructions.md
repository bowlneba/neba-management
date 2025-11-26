# bUnit Testing Instructions

This document defines how bUnit is used within the NEBA Management System under the **Neba.ComponentTests** project. bUnit tests validate Blazor component behavior, ensuring correctness of component logic, branching, event wiring, and UI contracts—without crossing into layout or browser-level testing.

---

# 1. Purpose

bUnit provides deterministic, isolated, fast component testing for Blazor. Unlike Playwright, bUnit does not execute layout, CSS, rendering engines, or real browser events. Instead, it focuses on validating:

- component semantics
- branching and conditional rendering
- event callbacks
- parameters and cascading values
- component-level validation logic
- interaction with mocked services

bUnit is the right tool when testing *logic* inside components, not layout or responsive behavior.

---

# 2. Project Location and Structure

All bUnit tests live under:

```
/tests/Neba.ComponentTests
```

This project references only:

- Blazor UI assemblies
- shared UI libraries
- mocks and helpers specific to UI components

It must **not** reference domain-only assemblies unless they are part of the component contract. It must never be mixed with:

- domain unit tests  
- Playwright tests  
- integration tests  

Structure example:

```
/tests/Neba.ComponentTests
  /Components
    /Shared
    /Navigation
    /Forms
    /Dialogs
  /TestUtils
```

---

# 3. What bUnit Is Responsible For

## 3.1 Component Branching Logic

bUnit tests all conditional behaviors that change a component’s rendered structure:

- show/hide sections
- render different markup depending on parameters
- enabling/disabling buttons
- toggling states
- verifying accessible markup (aria labels, roles, required attributes)

bUnit is ideal for exhaustive permutations that would be too slow in a browser environment.

---

## 3.2 Input and Validation Logic

Component-level validation belongs here:

- required fields
- error messages
- help text
- per-field validation state
- input-to-output mappings

These logic paths do not require real browser execution.

---

## 3.3 Parameter and Cascading Value Behavior

bUnit tests cover:

- correct handling of `Parameter` and `ParameterAttribute`
- correct propagation of `CascadingValue`
- correct default parameter behavior
- correct rerendering when parameters change

This ensures UI components behave consistently as reusable pieces.

---

## 3.4 Event Callback Behavior

bUnit validates:

- `EventCallback` invocations
- parent-child communication
- cascading notifications
- handler logic sequencing
- state changes triggered by events

This layer is optimized for event semantics—not browser interaction mechanics.

---

## 3.5 Service Integration Inside Components

bUnit can test components that:

- call services
- consume injected dependencies
- react to async workflows
- use mocked interfaces internally

The behavior is validated without requiring backend services or browser execution.

---

# 4. What bUnit Should *Not* Test

Avoid using bUnit for:

- CSS layout rules
- responsive behavior
- toast placement (mobile/desktop logic)
- menu collapse rules
- scroll/overflow behavior
- animations
- keyboard navigation
- pointer/mouse/touch events
- browser-dependent rendering

These belong to Playwright, which executes a real rendering engine.

Also avoid:

- testing domain logic (belongs in Neba.UnitTests)
- duplicating tests that Playwright already covers
- overly-complex component tests that attempt to simulate browser behavior

---

# 5. Test Design Principles

### 5.1 One Test Method = One Logical Behavior
Do not pack multiple unrelated behaviors into a single test.

### 5.2 Prefer Readable Setup Over Over-Mocking
Avoid complex fixture setups when verifying UI logic.

### 5.3 Enforce Component Contracts
Every reusable component should have tests that enforce:

- states
- rendering contracts
- required parameters
- expected markup fragments

### 5.4 Don't Mirror Playwright Coverage
bUnit should focus on permutations and internal logic, not scenarios.

### 5.5 Keep Tests Fast and Deterministic
The entire bUnit suite should run in milliseconds.

---

# 6. Test Organization Rules

- group tests by component name  
- include “ShouldExpectedResult_WhenCondition” naming  
- use folders to organize by feature areas  
- place cross-component helpers in `/TestUtils`  
- maintain deterministic selector usage (avoid relying on runtime-generated ids)  

Tests should reflect **component responsibilities**, not application workflows.

---

# 7. CI Strategy for bUnit

bUnit tests run as part of the standard unit test pipeline:

- no browser dependencies
- fast execution
- suitable for every PR
- required to pass before merging

Because these tests are nearly instantaneous, they act as the first line of defense for UI logic regressions.

---

# 8. Future Enhancements

Potential expansions include:

- component contract test suites for reusable components
- golden markup comparison tests for shared components
- snapshot-like HTML validation (strict, not layout-based)
- lint-like validation of component accessibility attributes

---

bUnit tests in Neba.ComponentTests guarantee that Blazor components behave correctly and consistently at the logic and markup level without relying on a browser. This ensures stability, high coverage of permutations, and rapid feedback during development.
