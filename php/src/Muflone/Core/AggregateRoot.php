<?php

declare(strict_types=1);

namespace Muflone\Core;

use InvalidArgumentException;
use Muflone\IAggregate;
use Muflone\IMemento;
use Muflone\IRouteEvents;

/**
 * Event-Sourced Aggregate Root
 *
 * C# TO PHP TRANSLATION NOTES:
 * ============================
 *
 * WHAT IS AN AGGREGATE ROOT?
 * --------------------------
 * In Domain-Driven Design (DDD), an Aggregate Root is the entry point to a cluster of related
 * domain objects that must maintain consistency. All external interactions go through the root.
 *
 * WHAT IS EVENT SOURCING?
 * -----------------------
 * Instead of storing current state, Event Sourcing persists all state changes as a sequence of events.
 * To rebuild the aggregate's state, we replay all events from the beginning.
 *
 * Example in our BrewUp Sales domain:
 *   Event 1: SalesOrderCreated(id, number, beer, quantity)
 *   Event 2: SalesOrderPrepared(id)
 *   Event 3: SalesOrderClosed(id)
 *
 * To get current state: Apply events 1, 2, 3 in order → final state is "closed"
 *
 * KEY PATTERN: COMMAND → EVENT → STATE
 * -------------------------------------
 * 1. Command arrives (e.g., "Create Sales Order")
 * 2. Aggregate validates business rules
 * 3. If valid, raise event (e.g., SalesOrderCreated)
 * 4. Event is applied to update state
 * 5. Event is saved to event store
 *
 * IMPLEMENTATION DIFFERENCES:
 * ---------------------------
 *
 * C# VERSION:
 * - Uses properties with private setters for encapsulation
 * - Apply methods use method overloading (same name, different types)
 * - Can use record types for events (immutability by default)
 *
 * PHP VERSION:
 * - Uses protected properties (PHP doesn't have private setters)
 * - Apply methods use naming convention: applySalesOrderCreated, applySalesOrderPrepared, etc.
 * - Uses readonly properties (PHP 8.1+) for immutability
 * - Constructor property promotion for cleaner syntax
 *
 * CRITICAL PHP FEATURES USED:
 * ---------------------------
 * 1. Abstract class inheritance (same as C#)
 * 2. Protected properties accessible to child classes only
 * 3. Readonly properties to ensure events can't be modified after creation
 * 4. Type hints for strict typing
 * 5. Arrays instead of .NET collections (List<T> → array)
 */
abstract class AggregateRoot implements IAggregate
{
    /**
     * Events that have been raised but not yet persisted to the event store
     *
     * C# TRANSLATION NOTE:
     * In C#, this would be List<object>. In PHP, we use a native array.
     * Arrays in PHP are more flexible than C# List<T> and can store mixed types.
     *
     * @var array<object>
     */
    private array $uncommittedEvents = [];

    /**
     * The event router that dispatches events to apply methods
     *
     * C# TRANSLATION NOTE:
     * Both C# and PHP versions use the same pattern: an event router that uses
     * reflection to discover and invoke handler methods.
     */
    private IRouteEvents $registeredRoutes;

    /**
     * Unique identifier for this aggregate instance
     *
     * C# TRANSLATION NOTE:
     * Protected allows child classes to set the ID in their event handlers.
     * In C#, you'd use a private setter: public DomainId Id { get; private set; }
     * PHP doesn't support property accessors, so we use protected directly.
     */
    protected IDomainId $id;

    /**
     * Version number for optimistic concurrency control
     *
     * Incremented each time an event is applied. Used to detect conflicting updates
     * when saving to the event store. If two clients load version 5 and both try to
     * save new events, only one will succeed - the other gets a concurrency exception.
     *
     * C# TRANSLATION NOTE:
     * Same implementation in both languages - simple integer counter.
     */
    protected int $version = 0;

    public function __construct(?IRouteEvents $handler = null)
    {
        if ($handler !== null) {
            $this->setRegisteredRoutes($handler);
            $this->registeredRoutes->registerAggregate($this);
        }
    }

    protected function getRegisteredRoutes(): IRouteEvents
    {
        if (!isset($this->registeredRoutes)) {
            $this->registeredRoutes = new ConventionEventRouter(true, $this);
        }
        return $this->registeredRoutes;
    }

    protected function setRegisteredRoutes(IRouteEvents $routes): void
    {
        if ($routes === null) {
            throw new InvalidArgumentException('AggregateRoot must have an event router to function');
        }
        $this->registeredRoutes = $routes;
    }

    public function getId(): IDomainId
    {
        return $this->id;
    }

    public function getVersion(): int
    {
        return $this->version;
    }

    public function applyEvent(object $event): void
    {
        $this->getRegisteredRoutes()->dispatch($event);
        $this->version++;
    }

    /**
     * @return array<object>
     */
    public function getUncommittedEvents(): array
    {
        return $this->uncommittedEvents;
    }

    public function clearUncommittedEvents(): void
    {
        $this->uncommittedEvents = [];
    }

    public function getSnapshot(): ?IMemento
    {
        $snapshot = $this->createSnapshot();

        if ($snapshot === null) {
            return null;
        }

        $snapshot->setId($this->id->getValue());
        $snapshot->setVersion($this->version);
        return $snapshot;
    }

    public function equals(?IAggregate $other): bool
    {
        if ($other === null) {
            return false;
        }

        return get_class($this) === get_class($other)
            && get_class($other->getId()) === get_class($this->id)
            && $other->getId()->getValue() === $this->id->getValue();
    }

    protected function registerHandler(callable $handler): void
    {
        $this->getRegisteredRoutes()->register($handler);
    }

    /**
     * Raise a domain event (called from business logic methods)
     *
     * THIS IS THE HEART OF EVENT SOURCING:
     * -------------------------------------
     * When your business logic wants to change state, you don't modify properties directly.
     * Instead, you raise an event that describes what happened.
     *
     * EXAMPLE from SalesOrder aggregate:
     *   public static function create(...): self {
     *       $salesOrder = new self();
     *       $salesOrder->raiseEvent(new SalesOrderCreated(...));  // ← raises event
     *       return $salesOrder;
     *   }
     *
     * WHAT HAPPENS:
     * 1. applyEvent() is called to update the aggregate's state
     * 2. Event is added to uncommittedEvents array
     * 3. Repository will later persist these events to the event store
     *
     * C# TRANSLATION NOTE:
     * Identical implementation in both languages. The pattern is the same:
     * - Protected method, only accessible within the aggregate
     * - Takes an event object (any type)
     * - Applies the event and records it for persistence
     *
     * @param object $event The domain event to raise
     */
    protected function raiseEvent(object $event): void
    {
        // First apply the event to update internal state
        $this->applyEvent($event);

        // Then add to uncommitted events for persistence
        // C# uses List<object>.Add(), PHP uses array append
        $this->uncommittedEvents[] = $event;
    }

    /**
     * Create a snapshot for performance optimization (optional)
     *
     * EVENT SOURCING PERFORMANCE:
     * ---------------------------
     * If an aggregate has 10,000 events, replaying all of them to get current state is slow.
     * Snapshots solve this: periodically save current state, then only replay events after snapshot.
     *
     * Example: Snapshot at event 9,000 → only replay events 9,001-10,000 instead of all 10,000
     *
     * C# TRANSLATION NOTE:
     * Default implementation returns null (no snapshot). Aggregates can override if needed.
     * Same pattern in both C# and PHP.
     *
     * @return IMemento|null Snapshot of current state, or null if not supported
     */
    protected function createSnapshot(): ?IMemento
    {
        return null;
    }

    /**
     * Generate a hash for this aggregate (used for equality comparisons)
     *
     * C# TRANSLATION NOTE:
     * C# uses GetHashCode() override. PHP uses __hash() magic method.
     * Both use the aggregate ID as the basis for the hash.
     *
     * @return string SHA-256 hash of the aggregate ID
     */
    public function __hash(): string
    {
        return hash('sha256', $this->id->getValue());
    }
}
