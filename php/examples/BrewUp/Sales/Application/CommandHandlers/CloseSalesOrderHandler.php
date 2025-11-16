<?php

declare(strict_types=1);

namespace BrewUp\Sales\Application\CommandHandlers;

use BrewUp\Sales\Domain\SalesOrder;
use BrewUp\Sales\SharedKernel\Commands\CloseSalesOrder;
use Muflone\Messages\Commands\CommandHandlerAsync;
use Muflone\Messages\IMessage;
use Muflone\Persistence\IRepository;
use Psr\Log\LoggerInterface;

/**
 * Command Handler: Close Sales Order
 *
 * Handles the CloseSalesOrder command by loading the aggregate,
 * calling the close method, and persisting the new events.
 */
final class CloseSalesOrderHandler extends CommandHandlerAsync
{
    public function __construct(
        IRepository $repository,
        LoggerInterface $logger
    ) {
        parent::__construct($repository, $logger);
    }

    public function handleAsync(IMessage $command): void
    {
        assert($command instanceof CloseSalesOrder);

        $this->logger->info('Closing sales order', [
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
        $salesOrder->close($command->getWho());

        // Save new events
        $this->repository->save(
            $salesOrder,
            $command->getMessageId()
        );

        $this->logger->info('Sales order closed successfully', [
            'salesOrderId' => $command->getAggregateId()->getValue(),
        ]);
    }
}
