<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use Muflone\Messages\IMessageHandlerAsync;

interface IIntegrationEventHandlerAsync extends IMessageHandlerAsync
{
    /**
     * Handle the integration event asynchronously
     *
     * @param IIntegrationEvent $event
     * @return void
     */
    public function handleAsync(IIntegrationEvent $event): void;
}
