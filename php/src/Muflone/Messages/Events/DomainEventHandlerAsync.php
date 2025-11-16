<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use Muflone\Messages\IMessage;
use Psr\Log\LoggerInterface;

/**
 * Base class for domain event handlers
 *
 * @template TEvent of IDomainEvent
 */
abstract class DomainEventHandlerAsync implements IDomainEventHandlerAsync
{
    public function __construct(
        protected readonly LoggerInterface $logger
    ) {
    }

    /**
     * Handle the domain event asynchronously
     *
     * @param IDomainEvent $event
     * @return void
     */
    abstract public function handleAsync(IMessage $event): void;
}
