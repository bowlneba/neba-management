# GitHub Pages Documentation Skill

This skill helps you create high-quality GitHub Pages documentation for the NEBA Management System, including administrative guides, ubiquitous language definitions, and domain-driven design artifacts.

## When to Use This Skill

The skill will automatically activate when you:

- Write or update documentation for admin procedures
- Document domain terminology and ubiquitous language
- Create or maintain DDD (Domain-Driven Design) documentation
- Write user guides or tutorials for GitHub Pages

## What This Skill Provides

### Documentation Standards

- Consistent formatting and structure guidelines
- Best practices for administrative how-to guides
- Templates for ubiquitous language definitions
- DDD artifact documentation patterns

### Templates

The skill includes three main documentation templates:

1. **Administrative How-To Template** - For step-by-step admin guides
2. **Ubiquitous Language Entry Template** - For domain term definitions
3. **Jekyll Front Matter** - For GitHub Pages configuration

### Examples

The skill includes three example files:

- [`admin-guide-example.md`](./admin-guide-example.md) - Complete example of an admin guide
- [`domain-terms-example.md`](./domain-terms-example.md) - Example of domain terminology documentation
- [`jekyll-config-reference.md`](./jekyll-config-reference.md) - Jekyll and GitHub Pages configuration guide

## Quick Start

### Create an Admin Guide

Ask Claude to create an admin guide:

```block
Create documentation for how to manage tournament awards in the admin portal
```

The skill will:

1. Research the relevant code and features
2. Use the administrative how-to template
3. Include prerequisites, step-by-step instructions, and troubleshooting
4. Add proper Jekyll front matter for GitHub Pages

### Document Domain Terms

Ask Claude to document domain concepts:

```block
Document the ubiquitous language terms for the Awards bounded context
```

The skill will:

1. Search for domain models, entities, and value objects
2. Extract term definitions and usage
3. Link to source code implementations
4. Cross-reference related terms
5. Include error codes from the Error Code Standards

### Create DDD Documentation

Ask Claude to document domain-driven design artifacts:

```block
Create documentation for the Tournament Management bounded context
```

The skill will:

1. Identify aggregates, entities, and value objects
2. Document domain events and relationships
3. Include Mermaid diagrams for visualization
4. Provide code examples from the implementation

## File Structure

The skill suggests this documentation structure:

```block
docs/
├── index.md                    # Documentation home
├── admin/
│   ├── index.md               # Admin guide overview
│   ├── tournament-setup.md
│   ├── member-management.md
│   └── awards-management.md
├── domain/
│   ├── index.md               # Domain documentation overview
│   ├── ubiquitous-language.md
│   ├── bounded-contexts.md
│   ├── aggregates.md
│   └── error-codes.md
├── api/
│   └── index.md               # API reference
└── assets/
    └── images/                # Screenshots and diagrams
```

## Integration with Project Standards

The skill automatically integrates with:

### Error Code Standards

- References [`Error Code Standards.md`](../../../src/backend/Neba.Domain/Error%20Code%20Standards.md)
- Uses the format: `<DomainContext>.<Object>.<Member>.<Rule>`
- Documents ErrorOr library usage
- Links error codes to domain documentation

### Domain Model

- Searches the `Neba.Domain` project for entities and value objects
- Documents aggregates and bounded contexts
- Links documentation to actual source files
- Uses consistent terminology from the codebase

## Allowed Tools

The skill has access to these tools:

- Read, Glob, Grep - To explore source code and extract information
- Write, Edit - To create and update documentation
- Bash - To run git commands and file operations
- WebFetch - To reference external documentation if needed

## Best Practices

When using this skill, Claude will:

1. **Research First** - Read existing source code and documentation before writing
2. **Use Consistent Terminology** - Follow the ubiquitous language from the domain
3. **Link to Source** - Provide links to implementation files
4. **Include Examples** - Show realistic examples from the codebase
5. **Add Front Matter** - Include proper Jekyll YAML front matter
6. **Organize Hierarchically** - Use clear navigation structure
7. **Cross-Reference** - Link related documentation and terms

## Examples of Using This Skill

### Example 1: Document a New Feature

```list
I just added High Block awards to the API. Create documentation for:
1. How admins can manage High Block awards
2. The ubiquitous language terms for High Block
3. API reference for the new endpoints
```

### Example 2: Create Onboarding Guide

```list
Create an admin onboarding guide that explains:
- How to log in
- Overview of admin features
- Where to find help
```

### Example 3: Document Domain Model

```block
Document all the entities and value objects in the Tournaments bounded context
```

## Testing the Documentation

After creating documentation, verify:

1. **Links Work** - All internal and source code links are valid
2. **Front Matter Valid** - YAML front matter is properly formatted
3. **Terminology Consistent** - Uses the same terms as the codebase
4. **Code Examples Accurate** - Examples match actual implementation
5. **Navigation Clear** - nav_order and parent/child relationships are correct

## Publishing to GitHub Pages

Once documentation is created:

1. Commit the docs to the repository
2. Push to GitHub
3. Configure GitHub Pages in repository settings
4. Set source to "Deploy from a branch" and select `/docs` folder
5. Documentation will be published to your GitHub Pages URL

## Support

For questions about:

- **This skill:** Review the [SKILL.md](./SKILL.md) file
- **GitHub Pages:** See [jekyll-config-reference.md](./jekyll-config-reference.md)
- **Examples:** Check [admin-guide-example.md](./admin-guide-example.md) and [domain-terms-example.md](./domain-terms-example.md)

## Contributing

To improve this skill:

1. Update the [SKILL.md](./SKILL.md) with new guidelines
2. Add more examples to the reference files
3. Update templates based on team feedback
4. Commit changes to version control
