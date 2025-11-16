# Muflone PHP

A PHP port of [Muflone](https://github.com/CQRS-Muflone/Muflone) - A CQRS and Event Sourcing library based on the great work of Jonathan Oliver with [CommonDomain as part of NEventStore](https://github.com/NEventStore/NEventStore).

## Requirements

- PHP 8.2 or higher
- Composer

## Installation

```bash
composer require muflone/muflone-php
```

## Features

This library provides the core building blocks for implementing CQRS and Event Sourcing patterns in PHP:

### Core Components

- **DomainId**: Strongly-typed identifier for domain entities
- **ValueObject**: Base class for implementing value objects with equality comparison
- **Entity**: Base class for domain entities
- **AggregateRoot**: Base class for aggregate roots with event sourcing support

### Messaging

- **Commands**: Point-to-point messages for executing actions
  - `ICommand` interface
  - `Command` abstract base class
  - `ICommandHandlerAsync` for command handlers
  - `CommandHandlerAsync` abstract base class

- **Domain Events**: Events internal to a bounded context
  - `IDomainEvent` interface
  - `DomainEvent` abstract base class
  - `IDomainEventHandlerAsync` for event handlers
  - `DomainEventHandlerAsync` abstract base class

- **Integration Events**: Events for cross-bounded-context communication
  - `IIntegrationEvent` interface
  - `IntegrationEvent` abstract base class
  - `IIntegrationEventHandlerAsync` for event handlers
  - `IntegrationEventHandlerAsync` abstract base class

### Persistence

- **IRepository**: Interface for aggregate persistence
- **ISerializer**: Interface for object serialization/deserialization
- **Serializer**: JSON-based implementation with type information

### Event Routing

- **ConventionEventRouter**: Automatically routes events to `apply()` methods on aggregates
- **IRouteEvents**: Interface for custom event routing implementations

## Basic Usage

### 1. Define a Domain ID

```php
use Muflone\Core\DomainId;

final class OrderId extends DomainId
{
    public static function create(): self
    {
        return new self(\Ramsey\Uuid\Uuid::uuid4()->toString());
    }

    public static function fromString(string $id): self
    {
        return new self($id);
    }
}
```

### 2. Create a Value Object

```php
use Muflone\Core\ValueObject;

final class Money extends ValueObject
{
    public function __construct(
        private readonly float $amount,
        private readonly string $currency
    ) {
    }

    protected function getEqualityComponents(): array
    {
        return [$this->amount, $this->currency];
    }
}
```

### 3. Define Domain Events

```php
use Muflone\Messages\Events\DomainEvent;
use Muflone\CustomTypes\Account;

final class OrderCreated extends DomainEvent
{
    public function __construct(
        OrderId $orderId,
        public readonly Money $totalAmount,
        string $correlationId = '',
        ?Account $who = null
    ) {
        parent::__construct($orderId, $correlationId, $who);
    }
}
```

### 4. Create an Aggregate Root

```php
use Muflone\Core\AggregateRoot;

final class Order extends AggregateRoot
{
    private Money $totalAmount;
    private string $status;

    public static function create(OrderId $id, Money $totalAmount): self
    {
        $order = new self();
        $order->raiseEvent(new OrderCreated($id, $totalAmount));
        return $order;
    }

    protected function apply(OrderCreated $event): void
    {
        $this->id = $event->getAggregateId();
        $this->totalAmount = $event->totalAmount;
        $this->status = 'created';
    }
}
```

### 5. Implement a Command Handler

```php
use Muflone\Messages\Commands\CommandHandlerAsync;
use Muflone\Persistence\IRepository;
use Psr\Log\LoggerInterface;

final class CreateOrderHandler extends CommandHandlerAsync
{
    public function __construct(
        IRepository $repository,
        LoggerInterface $logger
    ) {
        parent::__construct($repository, $logger);
    }

    public function handleAsync(ICommand $command): void
    {
        assert($command instanceof CreateOrder);

        $order = Order::create(
            $command->getAggregateId(),
            $command->totalAmount
        );

        $this->repository->save($order, $command->getMessageId());
    }
}
```

### 6. Implement an Event Handler

```php
use Muflone\Messages\Events\DomainEventHandlerAsync;
use Psr\Log\LoggerInterface;

final class OrderCreatedHandler extends DomainEventHandlerAsync
{
    public function __construct(LoggerInterface $logger)
    {
        parent::__construct($logger);
    }

    public function handleAsync(IDomainEvent $event): void
    {
        assert($event instanceof OrderCreated);

        $this->logger->info('Order created', [
            'orderId' => $event->getAggregateId()->getValue(),
            'amount' => $event->totalAmount
        ]);
    }
}
```

## Architecture

Muflone PHP follows the same architecture as the original C# version:

```
┌─────────────────────────────────────────────────┐
│                   Application                    │
│  ┌──────────────┐         ┌──────────────┐     │
│  │   Commands   │────────▶│   Handlers   │     │
│  └──────────────┘         └──────────────┘     │
│         │                        │              │
│         ▼                        ▼              │
│  ┌──────────────┐         ┌──────────────┐     │
│  │  Aggregates  │────────▶│    Events    │     │
│  └──────────────┘         └──────────────┘     │
│         │                        │              │
│         ▼                        ▼              │
│  ┌──────────────┐         ┌──────────────┐     │
│  │  Repository  │────────▶│  Event Bus   │     │
│  └──────────────┘         └──────────────┘     │
└─────────────────────────────────────────────────┘
```

## Key Differences from C# Version

1. **No Async/Await**: PHP doesn't have native async/await, so methods don't return promises
2. **Type Hinting**: Uses PHP 8.2+ type system with generics via PHPDoc annotations
3. **PSR Standards**: Uses PSR-3 for logging instead of Microsoft.Extensions.Logging
4. **UUID Generation**: Uses ramsey/uuid package instead of MassTransit's NewId
5. **Serialization**: Custom JSON serializer with type information instead of Newtonsoft.Json

## Testing

```bash
composer install
vendor/bin/phpunit
```

## Code Quality

```bash
# Static analysis
vendor/bin/phpstan analyse

# Code style check
vendor/bin/phpcs
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the same license as the original Muflone library.

## Credits

- Original C# Muflone library: [CQRS-Muflone/Muflone](https://github.com/CQRS-Muflone/Muflone)
- Based on Jonathan Oliver's CommonDomain: [NEventStore](https://github.com/NEventStore/NEventStore)
- PHP port by the Muflone PHP contributors

## Sample Usage

For a complete working example, see the [CQRS-ES testing workshop](https://github.com/CQRS-Muflone/CQRS-ES_testing_workshop) and adapt it to PHP.
