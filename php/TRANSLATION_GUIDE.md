# Muflone C# to PHP Translation Guide

## Table of Contents

1. [Introduction](#introduction)
2. [Architecture Overview](#architecture-overview)
3. [Key Translation Challenges](#key-translation-challenges)
4. [Language Feature Mapping](#language-feature-mapping)
5. [Core Library Translation](#core-library-translation)
6. [BrewUp Example Translation](#brewup-example-translation)
7. [Testing Strategy](#testing-strategy)
8. [Usage Guide](#usage-guide)

---

## Introduction

### Project Overview

This document explains the complete translation of the **Muflone** CQRS/Event Sourcing library from C# to PHP. Muflone is a framework for building event-sourced domain models using Command Query Responsibility Segregation (CQRS) and Domain-Driven Design (DDD) patterns.

### Goals

1. **Architectural Parity**: Maintain the same design patterns and architecture as the C# version
2. **Idiomatic PHP**: Use PHP-specific features and conventions where appropriate
3. **Type Safety**: Leverage PHP 8.2+ type system for compile-time safety
4. **Developer Experience**: Make the PHP version easy to understand for developers familiar with either language

### What Was Translated

- **Muflone Core Library**: Complete CQRS/Event Sourcing framework
- **BrewUp Sales Example**: Real-world demonstration of the framework
- **Test Suite**: 99 comprehensive tests validating all functionality

---

## Architecture Overview

### CQRS (Command Query Responsibility Segregation)

**Concept**: Separate write operations (Commands) from read operations (Queries).

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Command   │────────▶│    Handler   │────────▶│  Aggregate  │
└─────────────┘         └──────────────┘         └─────────────┘
                                                         │
                                                         ▼
                                                  ┌─────────────┐
                                                  │Event Store  │
                                                  └─────────────┘
```

**Both C# and PHP**: Same pattern, same interfaces, same flow.

### Event Sourcing

**Concept**: Store all state changes as a sequence of events instead of current state.

**Traditional Approach** (State-based):
```sql
UPDATE sales_orders SET status = 'prepared' WHERE id = 123
```

**Event Sourcing Approach**:
```sql
INSERT INTO events (aggregate_id, event_type, data)
VALUES (123, 'SalesOrderPrepared', '{"timestamp": "..."}')
```

**Advantages**:
- Complete audit trail
- Time travel (reconstruct state at any point)
- Event replay for debugging
- Event-driven architecture integration

### Domain-Driven Design (DDD)

**Key Concepts**:

1. **Aggregate Root**: Cluster of domain objects with consistency boundary
   - Example: `SalesOrder` (contains order details, status, history)

2. **Value Objects**: Immutable objects defined by their values
   - Example: `BeerName`, `Quantity`, `SalesOrderNumber`

3. **Domain Events**: Facts about what happened in the domain
   - Example: `SalesOrderCreated`, `SalesOrderPrepared`, `SalesOrderClosed`

4. **Commands**: Requests to perform an action
   - Example: `CreateSalesOrder`, `PrepareSalesOrder`, `CloseSalesOrder`

**Translation Note**: These patterns are language-agnostic. Both C# and PHP implementations use identical architectural patterns.

---

## Key Translation Challenges

### 1. Method Overloading (MAJOR DIFFERENCE)

**Challenge**: C# supports method overloading; PHP does not.

**C# Pattern**:
```csharp
public class SalesOrder : AggregateRoot
{
    protected void Apply(SalesOrderCreated @event) { ... }
    protected void Apply(SalesOrderPrepared @event) { ... }
    protected void Apply(SalesOrderClosed @event) { ... }
}
```

**PHP Solution**:
```php
class SalesOrder extends AggregateRoot
{
    protected function applySalesOrderCreated(SalesOrderCreated $event): void { ... }
    protected function applySalesOrderPrepared(SalesOrderPrepared $event): void { ... }
    protected function applySalesOrderClosed(SalesOrderClosed $event): void { ... }
}
```

**Implementation**: `ConventionEventRouter` uses reflection to discover methods starting with `apply` and registers them based on their parameter type.

**Files Affected**:
- `src/Muflone/Core/ConventionEventRouter.php` (discovery logic)
- All aggregate root implementations (naming convention)

---

### 2. Readonly Properties and Serialization

**Challenge**: PHP's `readonly` properties can only be set in the constructor, making deserialization difficult.

**Problem**:
```php
final readonly class SalesOrderCreated
{
    public function __construct(
        public readonly SalesOrderId $aggregateId,
        // ... more readonly properties
    ) {}
}
```

When deserializing from event store:
- Can't call constructor (don't have original parameters)
- Can't set readonly properties normally

**Solution**: Use reflection to bypass readonly restrictions.

```php
class Serializer implements ISerializer
{
    public function deserialize(string $serializedData, string $className): ?object
    {
        $reflection = new \ReflectionClass($className);

        // Create instance WITHOUT calling constructor
        $instance = $reflection->newInstanceWithoutConstructor();

        foreach ($data as $key => $value) {
            $property = $reflection->getProperty($key);

            // Bypass readonly restriction using reflection
            if (!$property->isInitialized($instance) || !$property->isReadOnly()) {
                $property->setAccessible(true);
                $property->setValue($instance, $value);
            }
        }

        return $instance;
    }
}
```

**C# Comparison**: C# serializers (Json.NET, System.Text.Json) handle readonly/init properties automatically.

**File**: `src/Muflone/Persistence/Serializer.php`

---

### 3. Interface Method Signature Compatibility

**Challenge**: PHP requires exact parameter type matching in interface hierarchies.

**Problem**:
```php
interface IMessageHandlerAsync {
    public function handleAsync(IMessage $message): void;
}

interface ICommandHandlerAsync extends IMessageHandlerAsync {
    // ❌ This would fail in PHP:
    public function handleAsync(ICommand $command): void; // Different parameter type!
}
```

**C# Allows**: Covariant/contravariant parameter types in some cases.

**PHP Solution**: Use base type in interface, assert specific type in implementation.

```php
interface ICommandHandlerAsync extends IMessageHandlerAsync {
    public function handleAsync(IMessage $command): void; // Use base type
}

class CreateSalesOrderHandler extends CommandHandlerAsync {
    public function handleAsync(IMessage $command): void {
        assert($command instanceof CreateSalesOrder); // Runtime type check

        // Now use as CreateSalesOrder
        $salesOrder = SalesOrder::create(
            $command->getAggregateId(),
            $command->salesOrderNumber,
            // ...
        );
    }
}
```

**Files Affected**:
- `src/Muflone/Messages/IMessageHandlerAsync.php`
- `src/Muflone/Messages/Commands/ICommandHandlerAsync.php`
- `src/Muflone/Messages/Events/IDomainEventHandlerAsync.php`
- `src/Muflone/Messages/Events/IIntegrationEventHandlerAsync.php`
- All handler implementations

---

### 4. Collections and Generics

**C# Feature**: Generic collections with type safety.

```csharp
private List<object> uncommittedEvents = new List<object>();
```

**PHP Equivalent**: Native arrays with PHPDoc type hints.

```php
/** @var array<object> */
private array $uncommittedEvents = [];
```

**Translation Strategy**:
- `List<T>` → `array` (PHP arrays are flexible, ordered collections)
- `Dictionary<TKey, TValue>` → `array` (PHP arrays are also associative)
- `IEnumerable<T>` → `iterable` or `array`
- PHPDoc annotations for IDE support and static analysis

**Trade-offs**:
- ✅ Simpler code (no need for collection classes)
- ✅ Better performance (native arrays)
- ❌ Less type safety at runtime (PHPStan/Psalm help at static analysis time)

---

### 5. Properties vs Fields

**C# Pattern**: Properties with private setters for encapsulation.

```csharp
public class SalesOrder : AggregateRoot
{
    public SalesOrderId Id { get; private set; }
    public string Status { get; private set; }
}
```

**PHP Pattern**: Protected fields with public getters.

```php
class SalesOrder extends AggregateRoot
{
    protected SalesOrderId $id;
    private string $status;

    public function getSalesOrderId(): SalesOrderId {
        return $this->id;
    }

    public function getStatus(): string {
        return $this->status;
    }
}
```

**Reason**: PHP doesn't have property accessors (get/set). We use traditional methods.

**Convention**:
- Protected for properties that event handlers need to set (like `$id`)
- Private for internal state (like `$status`)

---

## Language Feature Mapping

### Type System

| C# Feature | PHP Equivalent | Notes |
|------------|----------------|-------|
| `int`, `string`, `bool` | `int`, `string`, `bool` | Same primitive types |
| `object` | `object` | PHP 7.2+ has `object` type |
| `void` | `void` | Same |
| `string?` (nullable) | `?string` | Nullable types in both |
| `T[]` (array) | `array<T>` (PHPDoc) | PHPDoc for IDE support |
| `List<T>` | `array` | Native PHP arrays |
| `Dictionary<K,V>` | `array<K,V>` | Associative arrays |
| Generics `class Foo<T>` | `@template T` (PHPDoc) | Static analysis only |

### Class Features

| C# Feature | PHP Equivalent | Notes |
|------------|----------------|-------|
| `sealed class` | `final class` | Prevent inheritance |
| `abstract class` | `abstract class` | Same concept |
| `interface` | `interface` | Same concept |
| `record` | `final readonly class` | PHP 8.1+ readonly classes |
| Constructor property promotion (C# 9+) | Constructor property promotion (PHP 8.0+) | Same feature! |
| `readonly` modifier | `readonly` modifier | PHP 8.1+ |
| Private setter: `{ get; private set; }` | Protected field + getter method | No property accessors in PHP |

### Code Examples

**C# Constructor Property Promotion**:
```csharp
public record BeerName(string Value);
```

**PHP Constructor Property Promotion**:
```php
final readonly class BeerName {
    public function __construct(
        public readonly string $value
    ) {}
}
```

**Both produce**: A class with a public readonly property set via constructor.

---

## Core Library Translation

### File-by-File Guide

#### 1. Domain Identifiers

**C# File**: `Core/DomainId.cs`
**PHP File**: `src/Muflone/Core/DomainId.php`

**Purpose**: Base class for strongly-typed aggregate identifiers.

**Key Differences**:
- C# uses `record` type for simplicity
- PHP uses `abstract class` with readonly string value

**Translation Decisions**:
```php
abstract class DomainId implements IDomainId
{
    public function __construct(
        private readonly string $value  // PHP 8.1 readonly
    ) {}

    public function getValue(): string {
        return $this->value;
    }

    public function equals(?self $other): bool {
        if ($other === null) {
            return false;
        }
        return get_class($this) === get_class($other)
            && $this->value === $other->value;
    }
}
```

**Usage Example**:
```php
final readonly class SalesOrderId extends DomainId {
    public static function create(): self {
        return new self(\Ramsey\Uuid\Uuid::uuid4()->toString());
    }
}
```

---

#### 2. Value Objects

**C# File**: `Core/ValueObject.cs`
**PHP File**: `src/Muflone/Core/ValueObject.php`

**Purpose**: Base class for immutable value objects with value-based equality.

**Pattern**: Template Method pattern - child classes override `getEqualityComponents()`.

**Key Translation**:
```php
abstract class ValueObject
{
    abstract protected function getEqualityComponents(): array;

    public function equals(?self $other): bool {
        if ($other === null || get_class($this) !== get_class($other)) {
            return false;
        }

        return $this->getEqualityComponents() === $other->getEqualityComponents();
    }
}
```

**Hash Calculation Difference**:
- **C#**: Uses `GetHashCode()` with XOR operations
- **PHP**: Uses `crc32()` instead of SHA-256 for performance (hash codes are 32-bit integers)

```php
public function __hash(): string {
    $hash = 0;
    foreach ($this->getEqualityComponents() as $component) {
        $hash = $hash * 23 + ($component !== null ? crc32(serialize($component)) : 0);
    }
    return (string) $hash;
}
```

---

#### 3. Aggregate Root

**C# File**: `Core/AggregateRoot.cs`
**PHP File**: `src/Muflone/Core/AggregateRoot.php`

**Purpose**: Base class for event-sourced aggregates.

**Extensive inline documentation added** - see file for detailed C# to PHP translation notes.

**Key Methods**:

1. **`raiseEvent(object $event)`**: Raises and records a domain event
2. **`applyEvent(object $event)`**: Dispatches event to appropriate apply method
3. **`getUncommittedEvents()`**: Returns events pending persistence

**Translation Decisions**:
- Protected `$id` field (C# would use private set property)
- Array for `$uncommittedEvents` (C# uses `List<object>`)
- Convention-based event routing (C# uses overloaded `Apply` methods)

---

#### 4. Convention Event Router

**C# File**: `Core/ConventionEventRouter.cs`
**PHP File**: `src/Muflone/Core/ConventionEventRouter.php`

**Purpose**: Routes events to aggregate apply methods using reflection.

**CRITICAL TRANSLATION CHALLENGE**: Method overloading workaround.

**Extensive inline documentation added** - see file for complete explanation.

**Discovery Algorithm**:
1. Get all public/protected methods via reflection
2. Filter methods starting with "apply" that have one parameter
3. Extract parameter type hint
4. Register closure that invokes method for that event type

```php
foreach ($methods as $method) {
    if (str_starts_with($method->getName(), 'apply')
        && $method->getNumberOfParameters() === 1) {
        $type = $parameters[0]->getType();
        $typeName = $type->getName();

        $this->handlers[$typeName] = function($event) use ($aggregate, $method) {
            $method->setAccessible(true);
            $method->invoke($aggregate, $event);
        };
    }
}
```

---

#### 5. Messages (Commands/Events)

**C# Files**: `Messages/Commands/Command.cs`, `Messages/Events/DomainEvent.cs`
**PHP Files**: `src/Muflone/Messages/Commands/Command.php`, `src/Muflone/Messages/Events/DomainEvent.php`

**Purpose**: Base classes for commands and events with metadata.

**Key Features**:
- Message ID (unique identifier)
- Aggregate ID (which aggregate instance)
- Timestamp (when it occurred)
- Who (user/account performing action)
- Correlation ID (for distributed tracing)

**Translation**: Nearly identical structure in both languages.

**Example**:
```php
abstract class Command implements ICommand
{
    public function __construct(
        protected readonly IDomainId $aggregateId,
        protected readonly string $messageId = '',
        protected readonly Account $who = new Account('')
    ) {
        if ($this->messageId === '') {
            $this->messageId = \Ramsey\Uuid\Uuid::uuid4()->toString();
        }
    }
}
```

---

#### 6. Repository

**C# File**: `Persistence/Repository.cs`
**PHP File**: `src/Muflone/Persistence/Repository.php`

**Purpose**: Persist and retrieve aggregates from event store.

**Key Methods**:

1. **`save(IAggregate $aggregate, string $commitId)`**:
   - Get uncommitted events
   - Serialize to JSON
   - Store in event store
   - Clear uncommitted events

2. **`getById(string $className, IDomainId $id)`**:
   - Load events from store
   - Create new aggregate instance
   - Replay events to reconstitute state

**Translation Notes**:
- Same pattern in both languages
- PHP version uses `IEventStore` interface (in-memory implementation for testing)
- C# version might use EventStoreDB or SQL Server

---

## BrewUp Example Translation

### Domain: Sales Order Management

**Context**: Managing beer sales orders through their lifecycle.

**Aggregate**: `SalesOrder`
**States**: NEW → PREPARED → CLOSED
**Commands**: `CreateSalesOrder`, `PrepareSalesOrder`, `CloseSalesOrder`
**Events**: `SalesOrderCreated`, `SalesOrderPrepared`, `SalesOrderClosed`

### File Structure Comparison

**C# Structure** (from BrewUp/DDD-Europe-2025):
```
Sales/
├── Domain/
│   └── SalesOrder.cs
├── SharedKernel/
│   ├── Commands/
│   │   ├── CreateSalesOrder.cs
│   │   ├── PrepareSalesOrder.cs
│   │   └── CloseSalesOrder.cs
│   ├── Events/
│   │   ├── SalesOrderCreated.cs
│   │   ├── SalesOrderPrepared.cs
│   │   └── SalesOrderClosed.cs
│   └── CustomTypes/
│       ├── SalesOrderId.cs
│       ├── SalesOrderNumber.cs
│       ├── BeerName.cs
│       └── Quantity.cs
└── Application/
    └── CommandHandlers/
        ├── CreateSalesOrderHandler.cs
        ├── PrepareSalesOrderHandler.cs
        └── CloseSalesOrderHandler.cs
```

**PHP Structure**: Identical directory structure!

---

### SalesOrder Aggregate

**Extensive inline documentation added** - see `examples/BrewUp/Sales/Domain/SalesOrder.php` for complete C# to PHP comparison.

**Business Rules**:

1. ✅ Can only prepare orders in 'new' status
2. ✅ Can only close orders in 'prepared' status
3. ✅ Closing already-closed order is idempotent

**Event Sourcing Pattern**:

```php
class SalesOrder extends AggregateRoot
{
    // Command method: Validates business rules, raises event
    public function prepare(Account $who): void {
        if ($this->status !== 'new') {
            throw new \DomainException("Cannot prepare order in status: {$this->status}");
        }

        $this->raiseEvent(new SalesOrderPrepared($this->getSalesOrderId(), '', $who));
    }

    // Event handler: Updates state (no validation)
    protected function applySalesOrderPrepared(SalesOrderPrepared $event): void {
        $this->status = 'prepared';
    }
}
```

**Key Pattern**: Business rules in command methods, state mutation in event handlers.

---

### Value Objects

**Example: BeerName**

**C# Version**:
```csharp
public record BeerName(string Value) : ValueObject
{
    public BeerName(string value) : this(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Beer name cannot be empty");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**PHP Version**:
```php
final class BeerName extends ValueObject
{
    public function __construct(
        private readonly string $value
    ) {
        if (trim($this->value) === '') {
            throw new \InvalidArgumentException('Beer name cannot be empty');
        }
    }

    public function getValue(): string {
        return $this->value;
    }

    protected function getEqualityComponents(): array {
        return [$this->value];
    }
}
```

**Differences**:
- C# uses `record` with primary constructor
- PHP uses `readonly class` with constructor property promotion
- C# uses `yield return` for equality components
- PHP returns array directly
- Both validate in constructor
- Both inherit value-based equality from `ValueObject`

---

### Commands

**Example: CreateSalesOrder**

**PHP Version**:
```php
final readonly class CreateSalesOrder extends Command
{
    public function __construct(
        SalesOrderId $aggregateId,
        public readonly SalesOrderNumber $salesOrderNumber,
        public readonly BeerName $beerName,
        public readonly Quantity $quantity,
        string $messageId = '',
        Account $who = new Account('')
    ) {
        parent::__construct($aggregateId, $messageId, $who);
    }
}
```

**Translation Notes**:
- Uses constructor property promotion for all properties
- Inherits message infrastructure from `Command` base class
- `readonly` ensures immutability
- Same structure as C# version

---

### Command Handlers

**Example: CreateSalesOrderHandler**

**Extensive inline documentation added** - see file for details.

**Pattern**:
```php
final class CreateSalesOrderHandler extends CommandHandlerAsync
{
    public function handleAsync(IMessage $command): void {
        assert($command instanceof CreateSalesOrder);

        // Create new aggregate
        $salesOrder = SalesOrder::create(
            $command->getAggregateId(),
            $command->salesOrderNumber,
            $command->beerName,
            $command->quantity,
            $command->getWho()
        );

        // Persist to repository
        $this->repository->save($salesOrder, $command->getMessageId());
    }
}
```

**Translation Notes**:
- Accepts `IMessage` for interface compatibility
- Uses `assert()` for runtime type checking
- Same pattern as C#: load/create → execute → save

---

## Testing Strategy

### Test Suite Overview

**Total**: 99 tests, 215 assertions
**Coverage**: All core library components + BrewUp example

### Test Categories

1. **Unit Tests** (Core Library):
   - `AggregateRootTest.php` - Event sourcing mechanics
   - `ConventionEventRouterTest.php` - Event routing
   - `DomainIdTest.php` - Identity equality
   - `ValueObjectTest.php` - Value equality
   - `SerializerTest.php` - JSON serialization

2. **Integration Tests**:
   - `CommandEventFlowTest.php` - End-to-end command processing
   - `CommandHandlersTest.php` - Handler execution with repository

3. **Example Tests** (BrewUp):
   - `SalesOrderTest.php` - Aggregate behavior and business rules
   - `ValueObjectsTest.php` - Value object validation
   - `CommandHandlersTest.php` - Handler integration

### Testing Approach

**Pattern**: Arrange-Act-Assert

```php
public function testCanPrepareSalesOrder(): void
{
    // Arrange
    $salesOrder = SalesOrder::create(
        SalesOrderId::create(),
        new SalesOrderNumber('SO-001'),
        new BeerName('IPA'),
        new Quantity(10),
        new Account('test@example.com')
    );

    // Act
    $salesOrder->prepare(new Account('test@example.com'));

    // Assert
    $this->assertEquals('prepared', $salesOrder->getStatus());
    $this->assertCount(2, $salesOrder->getUncommittedEvents());
}
```

### Test Helpers

**InMemoryEventStore**: Simple event store for testing
```php
class InMemoryEventStore implements IEventStore {
    private array $events = [];

    public function append(string $stream, array $events): void {
        $this->events[$stream] = array_merge(
            $this->events[$stream] ?? [],
            $events
        );
    }

    public function read(string $stream): array {
        return $this->events[$stream] ?? [];
    }
}
```

**TestLogger**: PSR-3 logger that captures log messages
```php
class TestLogger extends AbstractLogger {
    public array $logs = [];

    public function log($level, $message, array $context = []): void {
        $this->logs[] = [
            'level' => $level,
            'message' => $message,
            'context' => $context
        ];
    }
}
```

---

## Usage Guide

### Installation

```bash
cd php
composer install
```

### Running Tests

```bash
vendor/bin/phpunit
```

### Example: Creating a Sales Order

```php
use BrewUp\Sales\Domain\SalesOrder;
use BrewUp\Sales\SharedKernel\CustomTypes\*;
use BrewUp\Sales\SharedKernel\Commands\CreateSalesOrder;
use BrewUp\Sales\Application\CommandHandlers\CreateSalesOrderHandler;

// 1. Create the command
$command = new CreateSalesOrder(
    SalesOrderId::create(),
    new SalesOrderNumber('SO-2025-0001'),
    new BeerName('Belgian Tripel'),
    new Quantity(24),
    '', // message ID (auto-generated)
    new Account('customer@brewery.com')
);

// 2. Create handler with repository and logger
$handler = new CreateSalesOrderHandler($repository, $logger);

// 3. Handle the command
$handler->handleAsync($command);

// 4. Events are now persisted in the event store
```

### Example: Event Sourcing Reconstruction

```php
// Load aggregate from event store
$salesOrder = $repository->getById(
    SalesOrder::class,
    $salesOrderId
);

// Aggregate state is rebuilt by replaying all events:
// 1. SalesOrderCreated → sets initial state
// 2. SalesOrderPrepared → status = 'prepared'
// 3. SalesOrderClosed → status = 'closed'

echo $salesOrder->getStatus(); // "closed"
```

### Example: Testing Business Rules

```php
public function testCannotPrepareAlreadyPreparedOrder(): void
{
    $salesOrder = $this->createSalesOrder();
    $salesOrder->prepare(new Account('test'));

    $this->expectException(\DomainException::class);
    $this->expectExceptionMessage('Cannot prepare sales order in status: prepared');

    $salesOrder->prepare(new Account('test')); // Should throw
}
```

---

## Best Practices

### 1. Event Handler Naming

✅ **Good**: Explicit event type in method name
```php
protected function applySalesOrderCreated(SalesOrderCreated $event): void
```

❌ **Bad**: Generic name (won't be discovered by router)
```php
protected function apply(SalesOrderCreated $event): void
```

### 2. Business Rule Location

✅ **Good**: Validate in command methods
```php
public function close(Account $who): void {
    if ($this->status !== 'prepared') {
        throw new \DomainException("Cannot close unprepared order");
    }
    $this->raiseEvent(new SalesOrderClosed(...));
}
```

❌ **Bad**: Validate in event handlers
```php
protected function applySalesOrderClosed(SalesOrderClosed $event): void {
    if ($this->status !== 'prepared') { // NO! Event handlers must be deterministic
        throw new \DomainException("...");
    }
}
```

### 3. Immutability

✅ **Good**: Use readonly for events and value objects
```php
final readonly class SalesOrderCreated extends DomainEvent {
    public function __construct(
        public readonly SalesOrderId $aggregateId,
        public readonly SalesOrderNumber $salesOrderNumber,
        // ...
    ) {}
}
```

✅ **Good**: Use constructor property promotion
```php
public function __construct(
    private readonly string $value
) {}
```

### 4. Type Safety

✅ **Good**: Use type hints everywhere
```php
public function create(
    SalesOrderId $id,
    SalesOrderNumber $number,
    BeerName $beer,
    Quantity $quantity
): self
```

✅ **Good**: Use assert for runtime type checking
```php
public function handleAsync(IMessage $command): void {
    assert($command instanceof CreateSalesOrder);
    // Now IDE knows $command is CreateSalesOrder
}
```

---

## Conclusion

### Translation Success Criteria

✅ **Architectural Parity**: All CQRS/ES patterns preserved
✅ **Type Safety**: Leveraged PHP 8.2+ features
✅ **Idiomatic PHP**: Used PHP conventions where appropriate
✅ **Comprehensive Tests**: 99 tests, 100% passing
✅ **Documentation**: Extensive inline documentation explaining C# to PHP translation

### Key Takeaways

1. **Method Overloading**: Solved with naming convention (`applyEventName`)
2. **Readonly Properties**: Solved with reflection-based deserialization
3. **Interface Compatibility**: Solved with base types + runtime assertions
4. **Collections**: Native PHP arrays work well for this use case
5. **Properties**: Methods replace C# property accessors

### For C# Developers

If you know C# CQRS/ES, this PHP version should feel familiar:
- Same architectural patterns
- Same event sourcing flow
- Same aggregate lifecycle
- Main difference: method naming for event handlers

### For PHP Developers

If you know PHP but not C#:
- Focus on the patterns (CQRS, Event Sourcing, DDD)
- These patterns are language-agnostic
- The inline documentation explains C# concepts
- Tests demonstrate expected behavior

---

## Further Reading

### Event Sourcing
- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html) - Martin Fowler
- [CQRS Journey](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj554200(v=pandp.10)) - Microsoft Patterns & Practices

### Domain-Driven Design
- "Domain-Driven Design" - Eric Evans
- "Implementing Domain-Driven Design" - Vaughn Vernon

### PHP Specific
- [PHP 8.1 readonly properties](https://www.php.net/manual/en/language.oop5.properties.php#language.oop5.properties.readonly-properties)
- [PHP 8.0 Constructor Property Promotion](https://www.php.net/manual/en/language.oop5.decon.php#language.oop5.decon.constructor.promotion)
- [Reflection API](https://www.php.net/manual/en/book.reflection.php)

---

**Document Version**: 1.0
**Last Updated**: 2025-11-16
**Translation Status**: Complete (99 tests passing)
