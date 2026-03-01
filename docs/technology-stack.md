# Technology Stack

## Summary

Muflone is a .NET library (distributed as a NuGet package) focused on CQRS + Event Sourcing patterns, with a separate xUnit-based test project.

## Languages & Runtimes

- **Runtime / Target Framework:** .NET `net10.0`
- **Primary language:** C#

## Solution Structure

- **Solution:** `src/muflone.sln`
- **Library project:** `src/Muflone/Muflone.csproj`
- **Test project:** `src/Muflone.Tests/Muflone.Tests.csproj`

## NuGet Packaging (Library)

From `src/Muflone/Muflone.csproj`:

- **Package ID:** `Muflone`
- **Package Version:** `10.0.2`
- **Generate package on build:** `true`
- **License:** MIT (`PackageLicenseExpression: MIT`)

## Runtime Dependencies (Library)

- `Microsoft.Extensions.Hosting.Abstractions` `10.0.1`
- `Microsoft.Extensions.Logging` `10.0.1`
- `NewId` `4.0.1`
- `Newtonsoft.Json` `13.0.3`

## Testing Tooling

From `src/Muflone.Tests/Muflone.Tests.csproj`:

- `Microsoft.NET.Test.Sdk` `18.0.1`
- `xunit` `2.9.3`
- `xunit.runner.visualstudio` `3.1.5`
- `coverlet.collector` `6.0.2`

## Architecture Pattern (High-Level)

- **Domain:** CQRS + Event Sourcing library
- **Likely style:** library-centric core abstractions + adapters/integrations (inferred from folder structure such as `Core/`, `Messages/`, `Persistence/`, and interfaces at project root)

## Build / Test Commands (Quick Reference)

- Build: `dotnet build src/muflone.sln -c Release`
- Test: `dotnet test src/muflone.sln -c Release`
- Pack: `dotnet pack src/Muflone/Muflone.csproj -c Release`
