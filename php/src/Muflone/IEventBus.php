<?php

declare(strict_types=1);

namespace Muflone;

use Muflone\Messages\Events\IEvent;

interface IEventBus
{
    /**
     * Publish an event to the event bus
     *
     * @template T of IEvent
     * @param T $event
     * @return void
     */
    public function publish(IEvent $event): void;
}
