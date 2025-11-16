# BrewUp Sales - Muflone PHP Example

This is a PHP translation of the [BrewUp DDD-Europe-2025](https://github.com/BrewUp/DDD-Europe-2025) Sales bounded context, demonstrating how to use the **Muflone PHP** library for implementing CQRS and Event Sourcing patterns.

## Overview

BrewUp is a brewery management system. This example implements the **Sales** bounded context, which handles customer beer orders through their lifecycle:

1. **Created** - A new sales order is created
2. **Prepared** - The order is prepared for fulfillment
3. **Closed** - The order is completed or cancelled

## Domain Model

### Aggregates

- **SalesOrder** (`Domain/SalesOrder.php`) - The main aggregate root representing a customer's beer order

### Value Objects

- **SalesOrderId** - Unique identifier for sales orders
- **SalesOrderNumber** - Human-readable order number
- **BeerName** - Name of the beer being ordered
- **Quantity** - Number of beers ordered

### Commands

- **CreateSalesOrder** - Create a new sales order
- **PrepareSalesOrder** - Mark an order as prepared
- **CloseSalesOrder** - Close/complete an order

### Domain Events

- **SalesOrderCreated** - Raised when a new order is created
- **SalesOrderPrepared** - Raised when an order is prepared
- **SalesOrderClosed** - Raised when an order is closed

### Command Handlers

- **CreateSalesOrderHandler** - Handles creating new orders
- **PrepareSalesOrderHandler** - Handles preparing orders
- **CloseSalesOrderHandler** - Handles closing orders

## Directory Structure

```
BrewUp/Sales/
├── Application/
│   └── CommandHandlers/     # Command handlers
├── Domain/
│   └── SalesOrder.php       # Aggregate root
└── SharedKernel/
    ├── Commands/            # Command definitions
    ├── Events/              # Domain event definitions
    └── CustomTypes/         # Value objects and IDs
```

## Example Usage

```php
<?php

use BrewUp\Sales\Application\CommandHandlers\CreateSalesOrderHandler;
use BrewUp\Sales\Application\CommandHandlers\PrepareSalesOrderHandler;
use BrewUp\Sales\Application\CommandHandlers\CloseSalesOrderHandler;
use BrewUp\Sales\SharedKernel\Commands\CreateSalesOrder;
use BrewUp\Sales\SharedKernel\Commands\PrepareSalesOrder;
use BrewUp\Sales\SharedKernel\Commands\CloseSalesOrder;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderNumber;
use BrewUp\Sales\SharedKernel\CustomTypes\BeerName;
use BrewUp\Sales\SharedKernel\CustomTypes\Quantity;
use Muflone\CustomTypes\Account;

// Setup (you would inject these via DI container)
$repository = new YourEventStoreRepository();
$logger = new YourLogger();

// Create command handlers
$createHandler = new CreateSalesOrderHandler($repository, $logger);
$prepareHandler = new PrepareSalesOrderHandler($repository, $logger);
$closeHandler = new CloseSalesOrderHandler($repository, $logger);

// 1. Create a new sales order
$salesOrderId = SalesOrderId::create();
$who = new Account('user-123', 'John Brewer');

$createCommand = new CreateSalesOrder(
    $salesOrderId,
    SalesOrderNumber::create('SO-2025-001'),
    BeerName::create('IPA Delight'),
    Quantity::create(24),
    null,
    $who
);

$createHandler->handleAsync($createCommand);
// Event raised: SalesOrderCreated

// 2. Prepare the order
$prepareCommand = new PrepareSalesOrder($salesOrderId, null, $who);
$prepareHandler->handleAsync($prepareCommand);
// Event raised: SalesOrderPrepared

// 3. Close the order
$closeCommand = new CloseSalesOrder($salesOrderId, null, $who);
$closeHandler->handleAsync($closeCommand);
// Event raised: SalesOrderClosed
```

## Business Rules

The SalesOrder aggregate enforces these business rules:

1. **Order Creation** - Must have valid beer name and positive quantity
2. **Preparation** - Can only prepare orders in 'new' status
3. **Closing** - Can only close orders in 'prepared' status
4. **Idempotency** - Closing an already closed order is a no-op

## Event Sourcing Flow

```
Command -> Handler -> Aggregate -> Events -> Event Store

CreateSalesOrder
    -> CreateSalesOrderHandler
    -> SalesOrder::create()
    -> SalesOrderCreated event
    -> Repository saves events

PrepareSalesOrder
    -> PrepareSalesOrderHandler
    -> Repository loads SalesOrder from events
    -> SalesOrder::prepare()
    -> SalesOrderPrepared event
    -> Repository saves new events
```

## Key Patterns Demonstrated

### 1. **Aggregate Root Pattern**
```php
final class SalesOrder extends AggregateRoot
{
    public static function create(...): self
    {
        $salesOrder = new self();
        $salesOrder->raiseEvent(new SalesOrderCreated(...));
        return $salesOrder;
    }
}
```

### 2. **Event Application**
```php
protected function applySalesOrderCreated(SalesOrderCreated $event): void
{
    $this->id = $event->getAggregateId();
    $this->salesOrderNumber = $event->salesOrderNumber;
    // ... set other fields from event
}
```

### 3. **Command Handler Pattern**
```php
final class CreateSalesOrderHandler extends CommandHandlerAsync
{
    public function handleAsync(ICommand $command): void
    {
        $salesOrder = SalesOrder::create(...);
        $this->repository->save($salesOrder, $command->getMessageId());
    }
}
```

### 4. **Value Objects**
```php
final class BeerName extends ValueObject
{
    public function __construct(private readonly string $value)
    {
        if (empty($value)) {
            throw new \InvalidArgumentException('Beer name cannot be empty');
        }
    }
}
```

## Differences from C# Version

1. **No Async/Await** - PHP methods are synchronous
2. **Type Safety** - Uses PHP 8.2+ type system with readonly properties
3. **Event Routing** - Uses `applyEventName()` method pattern instead of overloaded `apply()` methods
4. **PSR Logging** - Uses PSR-3 LoggerInterface instead of Microsoft.Extensions.Logging
5. **UUIDs** - Uses ramsey/uuid instead of MassTransit's NewId

## Testing

See the main Muflone test suite for examples of testing aggregates and command handlers.

## Integration

To integrate this into your application:

1. **Implement IRepository** - Create an event store implementation
2. **Register Handlers** - Register command handlers in your DI container
3. **Setup Event Bus** - Configure event publishing for projections/read models
4. **Add Read Model** - Create query handlers for the read side (CQRS)

## Original C# Source

This example is based on:
- Repository: https://github.com/BrewUp/DDD-Europe-2025
- Branch: `03-monolith_with_cqrs_and_event_sourcing`
- Path: `src/Sales/`

## Credits

- Original C# BrewUp implementation by the BrewUp team
- Muflone C# library by CQRS-Muflone
- PHP translation and example by Muflone PHP contributors
