<?php

declare(strict_types=1);

namespace Muflone\Factories;

use Muflone\Messages\Events\IDomainEvent;
use Muflone\Messages\Events\IDomainEventHandlerAsync;

interface IDomainEventHandlerFactoryAsync
{
    /**
     * Create domain event handlers for the specified event type
     *
     * @template T of IDomainEvent
     * @param class-string<T> $eventClass
     * @return array<IDomainEventHandlerAsync>
     */
    public function createDomainEventHandlers(string $eventClass): array;
}
