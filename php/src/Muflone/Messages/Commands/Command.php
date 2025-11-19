<?php

declare(strict_types=1);

namespace Muflone\Messages\Commands;

use DateTimeImmutable;
use Muflone\Core\IDomainId;
use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;
use Ramsey\Uuid\Uuid;

/**
 * Command Base Class - CQRS Pattern
 *
 * C# TO PHP TRANSLATION NOTES:
 * ============================
 *
 * WHAT IS A COMMAND?
 * ------------------
 * A Command is an imperative instruction to perform an action in the system.
 * It represents an intent to change state.
 *
 * EXAMPLES:
 * - CreateSalesOrder("Create a new sales order for 10 beers")
 * - PrepareSalesOrder("Prepare order SO-2025-001 for shipment")
 * - CloseSalesOrder("Close order SO-2025-001")
 *
 * CQRS PATTERN:
 * -------------
 * CQRS = Command Query Responsibility Segregation
 *
 * Commands:
 * - Write operations (modify state)
 * - Imperative (do something!)
 * - Named with verbs: Create, Update, Delete, Process, etc.
 * - Point-to-point (one sender → one handler)
 * - May succeed or fail
 *
 * Queries:
 * - Read operations (no state changes)
 * - Named with Get/Find/List: GetOrder, FindCustomer, ListProducts
 * - Can have multiple readers
 * - Should always succeed (return empty if nothing found)
 *
 * COMMAND VS EVENT:
 * -----------------
 * Command (Intent):
 * - "Please create a sales order"
 * - Future tense or imperative
 * - Can be rejected
 * - One handler
 *
 * Event (Fact):
 * - "Sales order was created"
 * - Past tense
 * - Already happened (can't be rejected)
 * - Can have multiple handlers
 *
 * C# VS PHP IMPLEMENTATION:
 * -------------------------
 *
 * C# APPROACH (using records):
 * ```csharp
 * public record CreateSalesOrder(
 *     SalesOrderId AggregateId,
 *     SalesOrderNumber Number,
 *     BeerName Beer,
 *     Quantity Quantity
 * ) : Command(AggregateId);
 * ```
 *
 * PHP APPROACH (using readonly class):
 * ```php
 * final readonly class CreateSalesOrder extends Command
 * {
 *     public function __construct(
 *         SalesOrderId $aggregateId,
 *         public readonly SalesOrderNumber $salesOrderNumber,
 *         public readonly BeerName $beerName,
 *         public readonly Quantity $quantity,
 *         ?string $messageId = null,
 *         ?Account $who = null
 *     ) {
 *         parent::__construct($aggregateId, $messageId, $who);
 *     }
 * }
 * ```
 *
 * KEY DIFFERENCES:
 * ----------------
 * 1. **Inheritance**:
 *    - Both: Inherit from Command base class
 *    - C#: Can use records with primary constructor
 *    - PHP: Use readonly class with constructor property promotion
 *
 * 2. **Immutability**:
 *    - C#: Records are immutable by default
 *    - PHP: Use 'readonly' modifier on class or properties
 *
 * 3. **Metadata (Who, When, MessageId)**:
 *    - Both: Stored in base class
 *    - Automatically populated with defaults if not provided
 *
 * COMMAND METADATA:
 * -----------------
 * Every command includes:
 *
 * 1. **AggregateId**: Which aggregate instance to operate on
 * 2. **MessageId**: Unique ID for this command (idempotency, tracing)
 * 3. **Who**: Who issued the command (audit trail)
 * 4. **When**: Timestamp when command was created
 * 5. **UserProperties**: Custom metadata (correlation IDs, tenant IDs, etc.)
 *
 * USAGE PATTERN:
 * --------------
 * ```php
 * // 1. Create command
 * $command = new CreateSalesOrder(
 *     SalesOrderId::create(),
 *     new SalesOrderNumber('SO-2025-001'),
 *     new BeerName('IPA'),
 *     new Quantity(10),
 *     null, // messageId (auto-generated)
 *     new Account('user-123', 'john@example.com')
 * );
 *
 * // 2. Send to handler
 * $handler = new CreateSalesOrderHandler($repository, $logger);
 * $handler->handleAsync($command);
 *
 * // 3. Handler creates aggregate and persists events
 * ```
 *
 * POINT-TO-POINT MESSAGING:
 * --------------------------
 * Commands have exactly ONE handler. This is enforced by design:
 * - One command → One handler → One aggregate
 * - Ensures consistency and clear responsibility
 * - If multiple actions needed, use events (one event → many handlers)
 */
abstract class Command implements ICommand
{
    protected IDomainId $aggregateId;
    protected string $messageId;
    /** @var array<string, mixed> */
    protected array $userProperties = [];
    protected Account $who;
    protected When $when;

    public function __construct(
        IDomainId $aggregateId,
        ?string $commitId = null,
        ?Account $who = null,
        ?When $when = null
    ) {
        $this->aggregateId = $aggregateId;
        $this->messageId = $commitId ?? Uuid::uuid4()->toString();
        $this->who = $who ?? new Account(Uuid::uuid4()->toString(), 'Anonymous');
        $this->when = $when ?? new When(new DateTimeImmutable());
    }

    public function getAggregateId(): IDomainId
    {
        return $this->aggregateId;
    }

    public function getMessageId(): string
    {
        return $this->messageId;
    }

    public function setMessageId(string $messageId): void
    {
        $this->messageId = $messageId;
    }

    /**
     * @return array<string, mixed>
     */
    public function getUserProperties(): array
    {
        return $this->userProperties;
    }

    /**
     * @param array<string, mixed> $userProperties
     */
    public function setUserProperties(array $userProperties): void
    {
        $this->userProperties = $userProperties;
    }

    public function getWho(): Account
    {
        return $this->who;
    }

    public function getWhen(): When
    {
        return $this->when;
    }
}
