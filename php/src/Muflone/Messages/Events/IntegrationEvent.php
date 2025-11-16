<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use Muflone\Core\IDomainId;
use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;

abstract class IntegrationEvent extends Event implements IIntegrationEvent
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
