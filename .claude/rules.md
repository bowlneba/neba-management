# Project-Specific Instructions

This project contains custom instruction files in `.github/instructions/` that provide domain-specific guidance for code generation, review, and architecture decisions.

## Instruction Files Usage

**IMPORTANT**: Before working on any task, read the relevant instruction files from `.github/instructions/` based on the file types and technologies involved:

### General Instructions (Always Apply)

- [.github/instructions/instructions.instructions.md](.github/instructions/instructions.instructions.md) - Guidelines for instruction files
- [.github/instructions/self-explanatory-code-commenting.instructions.md](.github/instructions/self-explanatory-code-commenting.instructions.md) - Code commenting standards
- [.github/instructions/security-and-owasp.instructions.md](.github/instructions/security-and-owasp.instructions.md) - Security best practices
- [.github/instructions/ai-prompt-engineering-safety-best-practices.instructions.md](.github/instructions/ai-prompt-engineering-safety-best-practices.instructions.md) - AI prompt safety

### .NET/C# Development

- [.github/instructions/csharp.instructions.md](.github/instructions/csharp.instructions.md) - C# coding standards
- [.github/instructions/dotnet-architecture-good-practices.instructions.md](.github/instructions/dotnet-architecture-good-practices.instructions.md) - .NET architecture patterns
- [.github/instructions/aspnet-rest-apis.instructions.md](.github/instructions/aspnet-rest-apis.instructions.md) - ASP.NET REST API development


### Blazor Development

- [.github/instructions/blazor.instructions.md](.github/instructions/blazor.instructions.md) - Blazor component development
- [.github/instructions/ui-notifications.instructions.md](.github/instructions/ui-notifications.instructions.md) - UI notification patterns
- [.github/instructions/ui-loading.instructions.md](.github/instructions/ui-loading.instructions.md) - UI loading state patterns
- [.github/instructions/tailwindcss.instructions.md](.github/instructions/tailwindcss.instructions.md) - TailwindCSS usage

### UI Testing

- [.github/instructions/ui-testing-playwright.instructions.md](.github/instructions/ui-testing-playwright.instructions.md) - Use for Playwright-based UI tests
- [.github/instructions/ui-testing.instructions.md](.github/instructions/ui-testing.instructions.md) - Use for general UI testing guidance

### Infrastructure & DevOps

- [.github/instructions/bicep-code-best-practices.instructions.md](.github/instructions/bicep-code-best-practices.instructions.md) - Azure Bicep templates
- [.github/instructions/github-actions.ci-cd-best-practices.instructions.md](.github/instructions/github-actions.ci-cd-best-practices.instructions.md) - GitHub Actions workflows

### Documentation

- [.github/instructions/markdown.instructions.md](.github/instructions/markdown.instructions.md) - Markdown formatting
- [.github/instructions/prompt.instructions.md](.github/instructions/prompt.instructions.md) - Prompt file standards

## Usage Guidelines

1. **Context-Aware Reading**: Read only the instruction files relevant to the current task
2. **Follow Patterns**: Apply the patterns, conventions, and best practices defined in these files
3. **Consistency**: Ensure all code follows the project's established standards
4. **Security First**: Always consider security guidelines from the security instructions
5. **Documentation**: Follow the commenting and documentation standards

## Custom Commands

Use the slash commands in `.claude/commands/` for specialized tasks and code generation patterns.
