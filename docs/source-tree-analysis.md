# Source Tree Analysis

This document highlights the main folders/files and how to navigate the codebase.

## High-Level Structure

```
Muflone/
├── README.md                      # Project overview + basic usage
├── docs/                          # Generated documentation output (this folder)
├── src/
│   ├── muflone.sln                # Visual Studio solution
│   ├── Muflone/                   # Library project (NuGet package)
│   │   ├── Muflone.csproj
│   │   ├── Core/                  # CQRS/ES core primitives (AggregateRoot, IDs, routing)
│   │   ├── Messages/              # Commands/Events abstractions + subscriptions
│   │   ├── Persistence/           # Repository + serialization abstractions
│   │   ├── Factories/             # Construction helpers/factories
│   │   ├── CustomTypes/           # Strong types/value objects used across the library
│   │   └── HandlerConfiguration.cs, MessageHandlersStarter.cs, ...
│   └── Muflone.Tests/             # Test project (xUnit)
│       ├── Muflone.Tests.csproj
│       ├── Core/                  # Unit tests for core primitives
│       ├── Persistence/           # Tests for persistence/serialization
│       └── Integration/           # Integration-style tests for flows
└── _bmad/                         # BMAD tooling (not part of library runtime)
```

## Key Entry Points (Library)

This is a **library**, so it doesn’t have a traditional executable entry point (`Program.cs`). Instead, typical “entry points” are:

- Public interfaces and abstractions in `src/Muflone/` (e.g., `IAggregate`, `IConsumer`, `IEventBus`, `IRouteEvents`, `IMemento`).
- Core behavior in `src/Muflone/Core/` (especially `AggregateRoot` and routing/detection helpers).
- Message abstractions in `src/Muflone/Messages/` (Commands/Events).

## Where to Look For…

- **Aggregate behavior / event sourcing rules:** `src/Muflone/Core/`
- **Commands and events:** `src/Muflone/Messages/Commands/` and `src/Muflone/Messages/Events/`
- **Persistence / serialization:** `src/Muflone/Persistence/`
- **Usage examples:** `README.md` (points to an external workshop repo)

## Tests

- Unit tests are in `src/Muflone.Tests/` (xUnit).
- These tests are a good “executable spec” for how aggregates/events are expected to behave.
