---
description: 'Infrastructure as Code with Bicep'
applyTo: '**/*.bicep'
---

## Naming Conventions

-   When writing Bicep code, use lowerCamelCase for all names (variables, parameters, resources)
-   Use resource type descriptive symbolic names (e.g., 'storageAccount' not 'storageAccountName')
-   Avoid using 'name' in a symbolic name as it represents the resource, not the resource's name
-   Avoid distinguishing variables and parameters by the use of suffixes

## Structure and Declaration

-   Always declare parameters at the top of files with @description decorators
-   Use latest stable API versions for all resources
-   Use descriptive @description decorators for all parameters
-   Specify minimum and maximum character length for naming parameters

## Parameters

-   Set default values that are safe for test environments (use low-cost pricing tiers)
-   Use @allowed decorator sparingly to avoid blocking valid deployments
-   Use parameters for settings that change between deployments

## Variables

-   Variables automatically infer type from the resolved value
-   Use variables to contain complex expressions instead of embedding them directly in resource properties

## Resource References

-   Use symbolic names for resource references instead of reference() or resourceId() functions
-   Create resource dependencies through symbolic names (resourceA.id) not explicit dependsOn
-   For accessing properties from other resources, use the 'existing' keyword instead of passing values through outputs

## Resource Names

-   Use template expressions with uniqueString() to create meaningful and unique resource names
-   Add prefixes to uniqueString() results since some resources don't allow names starting with numbers

## Child Resources

-   Avoid excessive nesting of child resources
-   Use parent property or nesting instead of constructing resource names for child resources

## Security

-   Never include secrets or keys in outputs
-   Use resource properties directly in outputs (e.g., storageAccount.properties.primaryEndpoints)

## Documentation

-   Include helpful // comments within your Bicep files to improve readability

## Resource Property Order

-   To avoid static analysis issues (for example Sonar rule S6975) and keep resources consistent, declare Bicep resource and decorator members in the following recommended order:

	1. `@description` (decorators on params/outputs)
	2. `@batchSize` (if used)
	3. `resource` symbolic declaration (the resource block itself)
	   - `parent` (if a child resource)
	   - `scope` (if scoped to a resource/group/subscription/management group)
	   - `name`
	   - `location` / `extendedLocation`
	   - `zones`
	   - `sku`
	   - `kind`
	   - `scale`
	   - `plan`
	   - `identity`
	   - `dependsOn`
	   - `tags`
	   - `properties`

	Keep properties grouped logically (e.g., `sku` object before `kind`), and prefer explicit ordering rather than arbitrary placement to make diffs and reviews easier.

-   Any other decorated elements not listed here should be placed before the resource object and after the other decorators. Any other elements not listed here should be placed before the `properties` object for the resource. This keeps resource blocks predictable for linters and static analysis tools.
