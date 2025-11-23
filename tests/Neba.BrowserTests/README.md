# NEBA Browser Tests

Playwright-based browser tests for the NEBA Management System.

## Prerequisites

1. **Install Playwright browsers** (first time only):

   ```bash
   # After building the project
   dotnet build tests/Neba.BrowserTests/
   pwsh tests/Neba.BrowserTests/bin/Debug/net10.0/playwright.ps1 install
   ```

2. **Start the Blazor app**:

   ```bash
   # In one terminal, start the app
   dotnet run --project src/frontend/Neba.Web.Server
   ```

   The app must be running at `https://localhost:5001` before running tests.

## Running Tests

### Run all browser tests

```bash
# In a separate terminal (with app running)
dotnet test tests/Neba.BrowserTests/

# Or using dotnet run (xUnit v3 native runner)
dotnet run --project tests/Neba.BrowserTests/
```

### Run specific test category

```bash
# Responsive tests only
dotnet test tests/Neba.BrowserTests/ --filter "FullyQualifiedName~Responsive"

# Navigation tests only
dotnet test tests/Neba.BrowserTests/ --filter "FullyQualifiedName~Navigation"

# Scenarios tests only
dotnet test tests/Neba.BrowserTests/ --filter "FullyQualifiedName~Scenarios"

# Workflows tests only
dotnet test tests/Neba.BrowserTests/ --filter "FullyQualifiedName~Workflows"
```

### Run specific browser only

```bash
# Chromium only
dotnet test tests/Neba.BrowserTests/ --filter "browserType=chromium"

# Firefox only
dotnet test tests/Neba.BrowserTests/ --filter "browserType=firefox"

# WebKit only
dotnet test tests/Neba.BrowserTests/ --filter "browserType=webkit"
```

## Test Organization

```text
/tests/Neba.BrowserTests/
  /Responsive/          - Responsive layout tests (5 tests)
  /Navigation/          - Navigation and menu tests (6 tests)
  /Scenarios/           - Error handling and notification tests (13 tests)
  /Workflows/           - Accessibility workflow tests (4 tests)
  /TestUtils/           - Test infrastructure and helpers
```

## Why Browser Tests Are Excluded from `dotnet test`

Browser tests require the Blazor app to be running, so they're excluded from solution-level test runs via [`.runsettings`](../../.runsettings).

This prevents false failures when running `dotnet test` without the app running.

## Debugging Tests

To debug tests with visible browser windows, set the `HEADLESS` environment variable:

```bash
HEADLESS=false dotnet test tests/Neba.BrowserTests/
```

Or in the test code, the `Headless` property in `PlaywrightTestBase` automatically disables headless mode when a debugger is attached.

## Writing New Tests

See [UI Testing Playwright Instructions](../../.github/instructions/ui-testing-playwright.instructions.md) for guidance on:

- Test organization
- Shouldly assertions
- Selector strategies
- Cross-browser testing
- Responsive testing patterns
