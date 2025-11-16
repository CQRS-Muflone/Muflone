<?php

declare(strict_types=1);

namespace BrewUp\Sales\Domain;

use BrewUp\Sales\SharedKernel\CustomTypes\BeerName;
use BrewUp\Sales\SharedKernel\CustomTypes\Quantity;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderNumber;
use BrewUp\Sales\SharedKernel\Events\SalesOrderClosed;
use BrewUp\Sales\SharedKernel\Events\SalesOrderCreated;
use BrewUp\Sales\SharedKernel\Events\SalesOrderPrepared;
use Muflone\Core\AggregateRoot;
use Muflone\CustomTypes\Account;

/**
 * Sales Order Aggregate Root
 *
 * Represents a customer's beer order throughout its lifecycle:
 * Created -> Prepared -> Closed
 */
class SalesOrder extends AggregateRoot
{
    private SalesOrderNumber $salesOrderNumber;
    private BeerName $beerName;
    private Quantity $quantity;
    private string $status = 'new';

    /**
     * Create a new Sales Order
     */
    public static function create(
        SalesOrderId $salesOrderId,
        SalesOrderNumber $salesOrderNumber,
        BeerName $beerName,
        Quantity $quantity,
        Account $who
    ): self {
        $salesOrder = new self();
        $salesOrder->raiseEvent(
            new SalesOrderCreated(
                $salesOrderId,
                $salesOrderNumber,
                $beerName,
                $quantity,
                '',
                $who
            )
        );
        return $salesOrder;
    }

    /**
     * Mark the order as prepared (ready for fulfillment)
     */
    public function prepare(Account $who): void
    {
        if ($this->status !== 'new') {
            throw new \DomainException("Cannot prepare sales order in status: {$this->status}");
        }

        $this->raiseEvent(
            new SalesOrderPrepared(
                $this->getSalesOrderId(),
                '',
                $who
            )
        );
    }

    /**
     * Close the sales order (completed or cancelled)
     */
    public function close(Account $who): void
    {
        if ($this->status === 'closed') {
            return; // Already closed, idempotent
        }

        if ($this->status !== 'prepared') {
            throw new \DomainException("Cannot close sales order that is not prepared. Current status: {$this->status}");
        }

        $this->raiseEvent(
            new SalesOrderClosed(
                $this->getSalesOrderId(),
                '',
                $who
            )
        );
    }

    /**
     * Event handler: Apply SalesOrderCreated event
     */
    protected function applySalesOrderCreated(SalesOrderCreated $event): void
    {
        $this->id = $event->getAggregateId();
        $this->salesOrderNumber = $event->salesOrderNumber;
        $this->beerName = $event->beerName;
        $this->quantity = $event->quantity;
        $this->status = 'new';
    }

    /**
     * Event handler: Apply SalesOrderPrepared event
     */
    protected function applySalesOrderPrepared(SalesOrderPrepared $event): void
    {
        $this->status = 'prepared';
    }

    /**
     * Event handler: Apply SalesOrderClosed event
     */
    protected function applySalesOrderClosed(SalesOrderClosed $event): void
    {
        $this->status = 'closed';
    }

    // Getters

    public function getSalesOrderId(): SalesOrderId
    {
        return $this->id;
    }

    public function getSalesOrderNumber(): SalesOrderNumber
    {
        return $this->salesOrderNumber;
    }

    public function getBeerName(): BeerName
    {
        return $this->beerName;
    }

    public function getQuantity(): Quantity
    {
        return $this->quantity;
    }

    public function getStatus(): string
    {
        return $this->status;
    }
}
