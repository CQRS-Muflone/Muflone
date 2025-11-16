<?php

declare(strict_types=1);

namespace BrewUp\Sales\SharedKernel\Commands;

use BrewUp\Sales\SharedKernel\CustomTypes\BeerName;
use BrewUp\Sales\SharedKernel\CustomTypes\Quantity;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderNumber;
use Muflone\CustomTypes\Account;
use Muflone\Messages\Commands\Command;

/**
 * Command: Create a new Sales Order
 */
final class CreateSalesOrder extends Command
{
    public function __construct(
        SalesOrderId $salesOrderId,
        public readonly SalesOrderNumber $salesOrderNumber,
        public readonly BeerName $beerName,
        public readonly Quantity $quantity,
        ?string $commitId = null,
        ?Account $who = null
    ) {
        parent::__construct($salesOrderId, $commitId, $who);
    }
}
