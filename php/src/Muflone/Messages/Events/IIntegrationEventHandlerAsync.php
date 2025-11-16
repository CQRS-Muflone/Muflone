<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use Muflone\Messages\IMessage;
use Muflone\Messages\IMessageHandlerAsync;

interface IIntegrationEventHandlerAsync extends IMessageHandlerAsync
{
    /**
     * Handle the integration event asynchronously
     * Note: Accepts IMessage for interface compatibility
     *
     * @param IMessage $event
     * @return void
     */
    public function handleAsync(IMessage $event): void;
}
