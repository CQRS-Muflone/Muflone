<?php

declare(strict_types=1);

namespace BrewUp\Sales\Domain;

use BrewUp\Sales\SharedKernel\CustomTypes\BeerName;
use BrewUp\Sales\SharedKernel\CustomTypes\Quantity;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderNumber;
use BrewUp\Sales\SharedKernel\Events\SalesOrderClosed;
use BrewUp\Sales\SharedKernel\Events\SalesOrderCreated;
use BrewUp\Sales\SharedKernel\Events\SalesOrderPrepared;
use Muflone\Core\AggregateRoot;
use Muflone\CustomTypes\Account;

/**
 * Sales Order Aggregate Root - COMPLETE EXAMPLE
 *
 * C# TO PHP TRANSLATION NOTES:
 * ============================
 *
 * This is a complete, working example of an event-sourced aggregate translated from C# to PHP.
 * It demonstrates all the key patterns from the Muflone framework.
 *
 * DOMAIN CONCEPT:
 * ---------------
 * A SalesOrder represents a customer's beer order. It has three states:
 * 1. NEW: Just created, awaiting preparation
 * 2. PREPARED: Ready for delivery
 * 3. CLOSED: Completed or cancelled
 *
 * STATE TRANSITIONS (enforced by business rules):
 * NEW → PREPARED → CLOSED
 *
 * EVENT SOURCING PATTERN:
 * -----------------------
 * Instead of UPDATE sales_orders SET status = 'prepared', we:
 * 1. Raise event: new SalesOrderPrepared(id)
 * 2. Apply event: status = 'prepared'
 * 3. Store event: INSERT INTO events (type, data, aggregate_id)
 *
 * RECONSTRUCTION:
 * To load aggregate from event store:
 * - Read all events for this aggregate ID
 * - Replay events: applySalesOrderCreated → applySalesOrderPrepared → etc.
 * - Final state matches last known state
 *
 * C# VS PHP KEY DIFFERENCES:
 * ---------------------------
 *
 * 1. STATIC FACTORY PATTERN:
 *    C#: public static SalesOrder Create(...)
 *    PHP: public static function create(...): self
 *    (Same pattern, PHP requires explicit return type)
 *
 * 2. EVENT HANDLER METHODS:
 *    C#: protected void Apply(SalesOrderCreated event) { }
 *    PHP: protected function applySalesOrderCreated(SalesOrderCreated $event): void { }
 *    (PHP uses named methods due to no method overloading)
 *
 * 3. VALUE OBJECTS:
 *    C#: readonly record BeerName(string Value)
 *    PHP: final readonly class BeerName with constructor property promotion
 *    (PHP 8.1+ supports readonly, but no record types)
 *
 * 4. EXCEPTION TYPES:
 *    C#: throw new DomainException(...)
 *    PHP: throw new \DomainException(...) - note the backslash for global namespace
 *
 * 5. CLASS MODIFIER:
 *    C#: Usually final/sealed to prevent inheritance
 *    PHP: Can be 'class' (not final) for testability - allows mocking/anonymous subclasses
 *
 * BUSINESS LOGIC LOCATION:
 * ------------------------
 * Business rules live in the command methods (prepare, close), NOT in event handlers.
 *
 * Command methods:
 * - Validate business rules
 * - Raise events if validation passes
 * - Example: prepare() checks status is 'new' before raising SalesOrderPrepared
 *
 * Event handlers (applyXxx methods):
 * - No validation, just state mutation
 * - Must be deterministic (same event = same state change)
 * - Example: applySalesOrderPrepared() just sets status = 'prepared'
 *
 * WHY?
 * - Events represent what happened in the past (immutable facts)
 * - When replaying events, we don't re-validate business rules
 * - If event is in the store, it means validation already passed
 */
class SalesOrder extends AggregateRoot
{
    // Domain properties - using type hints for safety
    // C# would use private set properties, PHP uses private properties with public getters
    private SalesOrderNumber $salesOrderNumber;
    private BeerName $beerName;
    private Quantity $quantity;
    private string $status = 'new';

    /**
     * Create a new Sales Order (Static Factory Method)
     *
     * C# TRANSLATION NOTE:
     * This static factory is preferred over using 'new SalesOrder()' directly because:
     * 1. Encapsulates creation logic
     * 2. Makes event sourcing explicit (you can see the SalesOrderCreated event being raised)
     * 3. Allows for complex creation logic if needed
     *
     * PATTERN EXPLANATION:
     * 1. Create empty aggregate: new self()
     * 2. Raise creation event: raiseEvent(new SalesOrderCreated(...))
     * 3. Event gets applied automatically, setting all properties
     * 4. Event is recorded for persistence
     *
     * @param SalesOrderId $salesOrderId Unique identifier (strongly-typed)
     * @param SalesOrderNumber $salesOrderNumber Order number from business (e.g., "SO-2025-0001")
     * @param BeerName $beerName Name of the beer being ordered
     * @param Quantity $quantity How many units
     * @param Account $who Who is creating this order (for audit trail)
     * @return self New SalesOrder instance in 'new' status
     */
    public static function create(
        SalesOrderId $salesOrderId,
        SalesOrderNumber $salesOrderNumber,
        BeerName $beerName,
        Quantity $quantity,
        Account $who
    ): self {
        // Create aggregate without calling constructor
        $salesOrder = new self();

        // Raise the creation event - this will:
        // 1. Call applySalesOrderCreated() to set initial state
        // 2. Add event to uncommitted events for persistence
        $salesOrder->raiseEvent(
            new SalesOrderCreated(
                $salesOrderId,
                $salesOrderNumber,
                $beerName,
                $quantity,
                '', // Correlation ID (empty for now)
                $who
            )
        );
        return $salesOrder;
    }

    /**
     * Mark the order as prepared (ready for fulfillment)
     *
     * BUSINESS RULE: Can only prepare orders in 'new' status
     *
     * C# TRANSLATION NOTE:
     * - C# would throw InvalidOperationException or custom DomainException
     * - PHP uses \DomainException from SPL (Standard PHP Library)
     * - Both follow the same pattern: validate → raise event
     *
     * @param Account $who Who is preparing this order (for audit trail)
     * @throws \DomainException if order is not in 'new' status
     */
    public function prepare(Account $who): void
    {
        // BUSINESS RULE VALIDATION (only in command methods, never in apply methods)
        if ($this->status !== 'new') {
            throw new \DomainException("Cannot prepare sales order in status: {$this->status}");
        }

        // If validation passes, raise the event
        $this->raiseEvent(
            new SalesOrderPrepared(
                $this->getSalesOrderId(),
                '', // Correlation ID
                $who
            )
        );
    }

    /**
     * Close the sales order (completed or cancelled)
     *
     * BUSINESS RULES:
     * 1. Can only close orders in 'prepared' status
     * 2. Closing an already-closed order is idempotent (does nothing, no error)
     *
     * IDEMPOTENCY:
     * If this method is called twice, the second call does nothing.
     * This is important for distributed systems where commands might be retried.
     *
     * @param Account $who Who is closing this order (for audit trail)
     * @throws \DomainException if order is not prepared and not already closed
     */
    public function close(Account $who): void
    {
        // Idempotent check - if already closed, just return
        // This allows safe retries without raising duplicate events
        if ($this->status === 'closed') {
            return;
        }

        // BUSINESS RULE VALIDATION
        if ($this->status !== 'prepared') {
            throw new \DomainException("Cannot close sales order that is not prepared. Current status: {$this->status}");
        }

        // Raise the closed event
        $this->raiseEvent(
            new SalesOrderClosed(
                $this->getSalesOrderId(),
                '', // Correlation ID
                $who
            )
        );
    }

    /**
     * Event handler: Apply SalesOrderCreated event
     *
     * C# TRANSLATION NOTE - CRITICAL:
     * --------------------------------
     * C# would have: protected void Apply(SalesOrderCreated event)
     * PHP requires: protected function applySalesOrderCreated(SalesOrderCreated $event): void
     *
     * WHY THE DIFFERENT NAME?
     * - C# supports method overloading (multiple methods with same name, different parameters)
     * - PHP does NOT support method overloading
     * - Solution: Use convention-based naming (applyEventName)
     *
     * WHAT THIS METHOD DOES:
     * - Sets all initial state from the event
     * - NO business rule validation (event is already fact)
     * - Must be deterministic (same event = same result every time)
     * - Called both when creating new orders AND when replaying events from storage
     *
     * @param SalesOrderCreated $event The creation event containing initial data
     */
    protected function applySalesOrderCreated(SalesOrderCreated $event): void
    {
        // Set aggregate ID from event
        // In C#: this.Id = event.AggregateId
        // In PHP: $this->id = $event->getAggregateId()
        $this->id = $event->getAggregateId();

        // Set all domain properties from event
        $this->salesOrderNumber = $event->salesOrderNumber;
        $this->beerName = $event->beerName;
        $this->quantity = $event->quantity;
        $this->status = 'new';
    }

    /**
     * Event handler: Apply SalesOrderPrepared event
     *
     * SIMPLICITY EXAMPLE:
     * Some event handlers are very simple - just update one property.
     * That's fine! Events represent granular state changes.
     *
     * @param SalesOrderPrepared $event The prepared event
     */
    protected function applySalesOrderPrepared(SalesOrderPrepared $event): void
    {
        $this->status = 'prepared';
    }

    /**
     * Event handler: Apply SalesOrderClosed event
     *
     * @param SalesOrderClosed $event The closed event
     */
    protected function applySalesOrderClosed(SalesOrderClosed $event): void
    {
        $this->status = 'closed';
    }

    // Getters

    public function getSalesOrderId(): SalesOrderId
    {
        return $this->id;
    }

    public function getSalesOrderNumber(): SalesOrderNumber
    {
        return $this->salesOrderNumber;
    }

    public function getBeerName(): BeerName
    {
        return $this->beerName;
    }

    public function getQuantity(): Quantity
    {
        return $this->quantity;
    }

    public function getStatus(): string
    {
        return $this->status;
    }
}
