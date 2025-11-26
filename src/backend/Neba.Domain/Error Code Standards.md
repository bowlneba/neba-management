# NEBA Error Code Standards

This document defines the **domain error code conventions** used across
the NEBA Management System.\
All errors must follow these rules to ensure consistency,
maintainability, and machine readability.


## ErrorOr Library Usage

The system uses the **ErrorOr** library for all domain and application errors

------------------------------------------------------------------------

## Purpose

Error codes serve three use cases:

1. **Developers** --- unambiguous identifiers for debugging and telemetry
2. **Clients** --- stable codes that can be mapped to UI behaviors
3. **Automation** --- structured format for pattern matching and
    monitoring

Error descriptions are not stable.\
**Error codes *are* stable and must never be changed once published.**

------------------------------------------------------------------------

## Error Code Format

All error codes MUST follow this structure:

    <DomainContext>.<Object>.<Member>.<Rule>

Examples:

- `Name.FirstName.Required`
- `Name.LastName.TooLong`
- `Membership.Id.NotFound`
- `Tournament.Entry.InvalidState`
- `Payment.CardNumber.InvalidFormat`

### Definitions

- **DomainContext** -- the bounded context or major domain grouping
- **Object** -- entity or value object
- **Member** -- field/property involved
- **Rule** -- violated constraint (Required, TooLong, Invalid, etc.)

------------------------------------------------------------------------

## Error Types

Error type is **NOT** part of the code
Instead use ErrorOr factory methods:

- `Error.Validation(...)`
- `Error.Conflict(...)`
- `Error.NotFound(...)`
- `Error.Unexpected(...)`

Example:

    ```csharp
    Error.Validation(
        code: "Name.FirstName.Required",
        description: "First name is required."
    );
    ```

------------------------------------------------------------------------

## Metadata Guidelines

Metadata MUST:

- Use `camelCase` keys
- Contain simple JSON-serializable values
- Avoid PII unless explicitly safe

### Use metadata when

- Validating lengths
- Enforcing ranges
- Reporting invalid states

### Examples

**Length validation:**

    ```csharp
    Error.Validation(
        code: "Name.FirstName.TooLong",
        description: "First name exceeds maximum length.",
        metadata: new()
        {
            ["maxLength"] = 30,
            ["actualLength"] = firstName.Length,
        }
    );
    ```

**Range:**

    ``` json
    {
    "minValue": 1,
    "maxValue": 10,
    "actualValue": value
    }
    ```

------------------------------------------------------------------------

## Error Organization in Code

    ```csharp
    public static class NameErrors
    {
        public static Error FirstNameRequired => Error.Validation(
            code: "Name.FirstName.Required",
            description: "First name is required."
        );

        public static Error FirstNameTooLong(int max, string value) => Error.Validation(
            code: "Name.FirstName.TooLong",
            description: $"First name exceeds {max} characters.",
            metadata: new()
            {
                ["maxLength"] = max,
                ["actualLength"] = value.Length
            });
    }
    ```

------------------------------------------------------------------------

## Naming Rules

1. Be specific
2. Use PascalCase
3. Rule names must describe the constraint
4. Never change a code once released

------------------------------------------------------------------------

## Example Error Codes (Summary)

- `Name.FirstName.Required`
- `Name.FirstName.TooLong`
- `Tournament.Entry.InvalidState`
- `Membership.Id.NotFound`
- `System.UnexpectedFailure`

------------------------------------------------------------------------

## Metadata Usage Summary

Use metadata when values or constraints matter; omit for simple boolean
rules.

------------------------------------------------------------------------

## Summary

Error codes MUST follow:

    <DomainContext>.<Object>.<Member>.<Rule>

Metadata MAY be included as appropriate.

Consistent error codes produce a predictable and maintainable domain
model.
