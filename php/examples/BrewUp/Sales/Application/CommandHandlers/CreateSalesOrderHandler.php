<?php

declare(strict_types=1);

namespace BrewUp\Sales\Application\CommandHandlers;

use BrewUp\Sales\Domain\SalesOrder;
use BrewUp\Sales\SharedKernel\Commands\CreateSalesOrder;
use Muflone\Messages\Commands\CommandHandlerAsync;
use Muflone\Messages\Commands\ICommand;
use Muflone\Persistence\IRepository;
use Psr\Log\LoggerInterface;

/**
 * Command Handler: Create Sales Order
 *
 * Handles the CreateSalesOrder command by creating a new SalesOrder aggregate
 * and persisting it to the event store.
 */
final class CreateSalesOrderHandler extends CommandHandlerAsync
{
    public function __construct(
        IRepository $repository,
        LoggerInterface $logger
    ) {
        parent::__construct($repository, $logger);
    }

    public function handleAsync(ICommand $command): void
    {
        assert($command instanceof CreateSalesOrder);

        $this->logger->info('Creating sales order', [
            'salesOrderId' => $command->getAggregateId()->getValue(),
            'salesOrderNumber' => $command->salesOrderNumber->getValue(),
            'beerName' => $command->beerName->getValue(),
            'quantity' => $command->quantity->getValue(),
        ]);

        // Create the sales order aggregate
        $salesOrder = SalesOrder::create(
            $command->getAggregateId(),
            $command->salesOrderNumber,
            $command->beerName,
            $command->quantity,
            $command->getWho()
        );

        // Save to repository (will persist events to event store)
        $this->repository->save(
            $salesOrder,
            $command->getMessageId()
        );

        $this->logger->info('Sales order created successfully', [
            'salesOrderId' => $command->getAggregateId()->getValue(),
        ]);
    }
}
