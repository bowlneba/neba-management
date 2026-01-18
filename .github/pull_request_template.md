## Description

<!-- Provide a brief summary of the changes in this PR -->

## Type of Change

<!-- Mark the relevant option(s) with an [x] -->

- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Refactoring (code change that neither fixes a bug nor adds a feature)
- [ ] Documentation update
- [ ] Performance improvement
- [ ] Infrastructure/tooling change

## Related Issues

<!-- Link to related issues using #issue_number -->

Closes #

## Changes Made

<!-- Provide a detailed list of changes -->

-
-
-

## Observability & Telemetry

<!-- Consider the following for code changes that involve new features, external services, or performance-critical paths -->

- [ ] **Metrics added** - Custom metrics added for key operations (counters, histograms, gauges)
- [ ] **Tracing added** - ActivitySource spans created for distributed tracing of operations
- [ ] **Structured logging** - [LoggerMessage] attributes used for logging events
- [ ] **Not applicable** - Changes don't require additional telemetry

**Telemetry Details** (if applicable):
<!-- Describe what metrics/traces/logs were added and why -->

## Testing

- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed
- [ ] No tests required (explain why)

**Test Coverage:**
<!-- Describe what was tested and how -->

## Checklist

- [ ] Code follows the project's coding standards and guidelines (`.github/instructions/*.md`)
- [ ] Self-review of code performed
- [ ] Comments added for complex/non-obvious code
- [ ] Documentation updated (if applicable)
- [ ] No new warnings introduced
- [ ] Dependent changes merged and published
- [ ] Database migrations added (if applicable)
- [ ] Configuration changes documented (if applicable)

## Performance Impact

<!-- Describe any performance implications of these changes -->

- [ ] No performance impact
- [ ] Performance improved
- [ ] Performance implications exist (explain below)

## Security Considerations

<!-- Describe any security implications -->

- [ ] No security impact
- [ ] Security review required
- [ ] Secrets/credentials handled properly

## Screenshots/Recordings

<!-- Add screenshots or recordings for UI changes -->

## Additional Notes

<!-- Any additional information reviewers should know -->

## Reviewer Notes

**For Reviewers:**

When reviewing this PR, please pay special attention to:

1. **Observability**: Are metrics, traces, and logs appropriate for the changes? See [ADR-0050: OpenTelemetry](../docs/architecture/adr-0050-opentelemetry-without-aspire-apphost.md)
   - New features should include metrics for key operations
   - External service calls should have tracing spans
   - Performance-critical paths should have duration metrics
   - Error cases should be logged with structured logging

2. **Code Quality**: Does the code follow established patterns and conventions?

3. **Testing**: Is the test coverage adequate for the changes?

4. **Performance**: Are there any potential performance bottlenecks?

5. **Security**: Are there any security concerns?
