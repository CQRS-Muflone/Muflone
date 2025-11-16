<?php

declare(strict_types=1);

namespace Muflone\Core;

use InvalidArgumentException;
use Muflone\IAggregate;
use Muflone\IMemento;
use Muflone\IRouteEvents;

abstract class AggregateRoot implements IAggregate
{
    /** @var array<object> */
    private array $uncommittedEvents = [];

    private IRouteEvents $registeredRoutes;

    protected IDomainId $id;
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

    protected function raiseEvent(object $event): void
    {
        $this->applyEvent($event);
        $this->uncommittedEvents[] = $event;
    }

    protected function createSnapshot(): ?IMemento
    {
        return null;
    }

    public function __hash(): string
    {
        return hash('sha256', $this->id->getValue());
    }
}
