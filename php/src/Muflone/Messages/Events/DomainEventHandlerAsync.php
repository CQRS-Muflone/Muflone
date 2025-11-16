<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

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
     * @param TEvent $event
     * @return void
     */
    abstract public function handleAsync(IDomainEvent $event): void;
}
