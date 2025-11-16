<?php

declare(strict_types=1);

namespace BrewUp\Sales\SharedKernel\Events;

use BrewUp\Sales\SharedKernel\CustomTypes\BeerName;
use BrewUp\Sales\SharedKernel\CustomTypes\Quantity;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderNumber;
use Muflone\CustomTypes\Account;
use Muflone\Messages\Events\DomainEvent;

/**
 * Domain Event: Sales Order has been created
 */
final class SalesOrderCreated extends DomainEvent
{
    /**
     * @param SalesOrderId $salesOrderId
     * @param SalesOrderNumber $salesOrderNumber
     * @param BeerName $beerName
     * @param Quantity $quantity
     * @param string $correlationId
     * @param Account|null $who
     */
    public function __construct(
        SalesOrderId $salesOrderId,
        public readonly SalesOrderNumber $salesOrderNumber,
        public readonly BeerName $beerName,
        public readonly Quantity $quantity,
        string $correlationId = '',
        ?Account $who = null
    ) {
        parent::__construct($salesOrderId, $correlationId, $who);
    }
}
