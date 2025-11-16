<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use Muflone\Messages\IMessageHandlerAsync;

interface IDomainEventHandlerAsync extends IMessageHandlerAsync
{
    /**
     * Handle the domain event asynchronously
     *
     * @param IDomainEvent $event
     * @return void
     */
    public function handleAsync(IDomainEvent $event): void;
}
