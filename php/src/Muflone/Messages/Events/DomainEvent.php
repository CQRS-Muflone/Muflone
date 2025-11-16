<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use DateTimeImmutable;
use Muflone\Core\IDomainId;
use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;

/**
 * Class DomainEvent
 * A domain-event is an indicator to interested parties that 'something has happened'.
 * We expect zero to many receivers as it is one-to-many communication i.e. publish-subscribe
 * A domain-event is usually fire-and-forget, because we do not know it is received.
 */
abstract class DomainEvent extends Event implements IDomainEvent
{
    public function __construct(
        IDomainId $aggregateId,
        string $correlationId = '',
        ?Account $who = null,
        ?When $when = null
    ) {
        parent::__construct(
            $aggregateId,
            $correlationId,
            $who,
            $when
        );
    }
}
