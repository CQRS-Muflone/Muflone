<?php

declare(strict_types=1);

namespace BrewUp\Sales\Application\CommandHandlers;

use BrewUp\Sales\Domain\SalesOrder;
use BrewUp\Sales\SharedKernel\Commands\PrepareSalesOrder;
use Muflone\Messages\Commands\CommandHandlerAsync;
use Muflone\Messages\IMessage;
use Muflone\Persistence\IRepository;
use Psr\Log\LoggerInterface;

/**
 * Command Handler: Prepare Sales Order
 *
 * Handles the PrepareSalesOrder command by loading the aggregate,
 * calling the prepare method, and persisting the new events.
 */
final class PrepareSalesOrderHandler extends CommandHandlerAsync
{
    public function __construct(
        IRepository $repository,
        LoggerInterface $logger
    ) {
        parent::__construct($repository, $logger);
    }

    public function handleAsync(IMessage $command): void
    {
        assert($command instanceof PrepareSalesOrder);

        $this->logger->info('Preparing sales order', [
            'salesOrderId' => $command->getAggregateId()->getValue(),
        ]);

        // Load the sales order aggregate from event store
        $salesOrder = $this->repository->getById(
            SalesOrder::class,
            $command->getAggregateId()
        );

        if ($salesOrder === null) {
            throw new \RuntimeException(
                "Sales order not found: {$command->getAggregateId()->getValue()}"
            );
        }

        // Execute business logic
        $salesOrder->prepare($command->getWho());

        // Save new events
        $this->repository->save(
            $salesOrder,
            $command->getMessageId()
        );

        $this->logger->info('Sales order prepared successfully', [
            'salesOrderId' => $command->getAggregateId()->getValue(),
        ]);
    }
}
