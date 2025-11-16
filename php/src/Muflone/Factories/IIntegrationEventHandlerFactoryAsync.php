<?php

declare(strict_types=1);

namespace Muflone\Factories;

use Muflone\Messages\Events\IIntegrationEvent;
use Muflone\Messages\Events\IIntegrationEventHandlerAsync;

interface IIntegrationEventHandlerFactoryAsync
{
    /**
     * Create integration event handlers for the specified event type
     *
     * @template T of IIntegrationEvent
     * @param class-string<T> $eventClass
     * @return array<IIntegrationEventHandlerAsync>
     */
    public function createIntegrationEventHandlers(string $eventClass): array;
}
