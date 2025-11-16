<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use Muflone\Messages\IMessage;
use Psr\Log\LoggerInterface;

/**
 * Base class for integration event handlers
 *
 * @template TEvent of IIntegrationEvent
 */
abstract class IntegrationEventHandlerAsync implements IIntegrationEventHandlerAsync
{
    public function __construct(
        protected readonly LoggerInterface $logger
    ) {
    }

    /**
     * Handle the integration event asynchronously
     *
     * @param IIntegrationEvent $event
     * @return void
     */
    abstract public function handleAsync(IMessage $event): void;
}
