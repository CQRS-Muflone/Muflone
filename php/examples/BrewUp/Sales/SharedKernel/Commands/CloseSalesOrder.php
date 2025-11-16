<?php

declare(strict_types=1);

namespace BrewUp\Sales\SharedKernel\Commands;

use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use Muflone\CustomTypes\Account;
use Muflone\Messages\Commands\Command;

/**
 * Command: Close a Sales Order
 */
final class CloseSalesOrder extends Command
{
    public function __construct(
        SalesOrderId $salesOrderId,
        ?string $commitId = null,
        ?Account $who = null
    ) {
        parent::__construct($salesOrderId, $commitId, $who);
    }
}
