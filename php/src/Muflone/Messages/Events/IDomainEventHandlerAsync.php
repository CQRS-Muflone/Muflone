<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use Muflone\Messages\IMessage;
use Muflone\Messages\IMessageHandlerAsync;

interface IDomainEventHandlerAsync extends IMessageHandlerAsync
{
    /**
     * Handle the domain event asynchronously
     * Note: Accepts IMessage for interface compatibility
     *
     * @param IMessage $event
     * @return void
     */
    public function handleAsync(IMessage $event): void;
}
