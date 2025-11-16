<?php

declare(strict_types=1);

namespace Muflone;

interface IRouteEvents
{
    /**
     * Register a handler for a specific event type
     *
     * @param callable $handler
     */
    public function register(callable $handler): void;

    /**
     * Register an aggregate and discover its Apply methods
     */
    public function registerAggregate(IAggregate $aggregate): void;

    /**
     * Dispatch an event to the appropriate handler
     */
    public function dispatch(object $eventMessage): void;
}
