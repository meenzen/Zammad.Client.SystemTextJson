# Copilot Instructions for Zammad.Client.SystemTextJson

This document provides guidelines for GitHub Copilot when working on this repository.

## Project Overview

This is a .NET client library for interacting with the Zammad helpdesk system API. It uses `System.Text.Json` for JSON
serialization instead of `Newtonsoft.Json`.

## Development Environment

### Target Frameworks
- .NET 8.0
- .NET 9.0
- .NET Standard 2.0

### Build and Test
```bash
dotnet restore
dotnet build
dotnet test test/Zammad.Client.Tests
```

**Note**: Integration tests take a long time to run. During development, build the project and run only the unit tests
to validate changes. The command above only runs the unit test project.

## Code Style and Formatting

### Formatter
- Use **CSharpier** for code formatting (configured as a dotnet tool)
- Run formatting before committing: `dotnet csharpier .`

### EditorConfig
- Follow rules defined in `.editorconfig`
- Use UTF-8 encoding with LF line endings
- Max line length: 120 characters
- Use spaces for indentation
- Insert final newline and trim trailing whitespace

### Analyzers
The project uses the following code analyzers with warnings treated as errors:
- **Roslynator.Analyzers** - C# code quality and style analyzer
- **SonarAnalyzer.CSharp** - Security and code quality analyzer
- **Microsoft.VisualStudio.Threading.Analyzers** - Threading best practices

All analyzer warnings must be fixed before code can be merged.

## Coding Conventions

### Language Features
- Use `var` when the type is evident
- Use latest C# language features (LangVersion set to `latest`)
- Avoid unnecessary `this` qualifiers

### Code Organization
- Main client implementation is in `ZammadClient.cs` with partial classes for different resource types (e.g., `ZammadClient.User.cs`, `ZammadClient.Ticket.cs`)
- Resource models are in the `Zammad.Client.Resources` namespace
- Core utilities and helpers are in the `Zammad.Client.Core` namespace

### Naming Conventions
- Follow standard C# naming conventions
- Use PascalCase for public members
- Use camelCase with underscore prefix for private fields (`_client`)

## JSON Serialization

### System.Text.Json
- This project uses `System.Text.Json`, **NOT** `Newtonsoft.Json`
- Use `JsonSerializer` from `System.Text.Json` namespace
- Use `JsonSerializerOptions` for configuration
- Custom converters should inherit from `JsonConverter<T>`

## Testing

### Test Framework
- Use **xUnit** for all tests
- Unit tests are in `test/Zammad.Client.Tests`
- Integration tests are in `test/Zammad.Client.IntegrationTests`

### Test Patterns
- Follow existing test naming conventions: `MethodName_Scenario_ExpectedBehavior`
- Use theory tests with `[Theory]` and `[InlineData]` for parameterized tests
- Integration tests automatically start required services as docker containers using **Testcontainers for .NET**

### Test Coverage
- Tests should cover new functionality
- The project uses Codecov for coverage tracking
- Aim for meaningful test coverage, not just high percentages

## Dependencies

### Package Management
- Dependencies are centrally managed in `Directory.Packages.props`
- Version information is managed by Nerdbank.GitVersioning

### Adding Dependencies
- Only add dependencies when necessary
- Prefer standard library features when available
- Security scan all new dependencies before adding

## Git and Commits

### Commit Messages
- Use **Conventional Commits** format: `type(scope): description`
- Common types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`
- **Do not** use emoji prefixes in commit messages

### Pre-commit Hooks
- Husky is configured for pre-commit hooks
- Code formatting and validation run automatically

## Documentation

### XML Documentation
- Document public APIs with XML documentation comments
- CS1591 (missing XML comments) is suppressed, but documentation is encouraged for public APIs
- Focus on documenting non-obvious behavior and parameters

### README Updates
- Update README.md when adding new features or changing usage patterns
- Keep examples up-to-date and working

## API Client Patterns

### HTTP Client Usage
- Use the injected `HttpClient` instance (`_client`)
- Don't create new `HttpClient` instances
- Use `HttpRequestMessage` and `HttpResponseMessage` for requests

### Async/Await
- All API methods should be async and return `Task` or `Task<T>`
- Method names should end with `Async` (e.g., `GetUserAsync`)
- Use `ConfigureAwait(false)` is not required (modern .NET best practice)

### Error Handling
- Parse HTTP status codes appropriately
- Throw meaningful exceptions with context
- Use `HttpResponseMessage.EnsureSuccessStatusCode()` where appropriate

## Security

### Authentication
- Support both token-based and basic authentication
- Never hardcode credentials or tokens
- Use `IOptions<ZammadOptions>` for configuration

### Input Validation
- Validate all external inputs
- Use the existing `ThrowIfInvalid()` pattern for option validation

## Extensions Package

The `Zammad.Client.SystemTextJson.Extensions` package provides:
- Dependency injection support via `AddZammadClient()`
- Integration with `Microsoft.Extensions.DependencyInjection`
- `IHttpClientBuilder` support for advanced HTTP client configuration

When making changes to the core library, consider if the extensions package needs updates.
