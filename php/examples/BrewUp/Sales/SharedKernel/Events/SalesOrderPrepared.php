<?php

declare(strict_types=1);

namespace BrewUp\Sales\SharedKernel\Events;

use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use Muflone\CustomTypes\Account;
use Muflone\Messages\Events\DomainEvent;

/**
 * Domain Event: Sales Order has been prepared
 */
final class SalesOrderPrepared extends DomainEvent
{
    public function __construct(
        SalesOrderId $salesOrderId,
        string $correlationId = '',
        ?Account $who = null
    ) {
        parent::__construct($salesOrderId, $correlationId, $who);
    }
}
