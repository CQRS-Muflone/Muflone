<?php

declare(strict_types=1);

namespace Muflone\Tests\Examples\BrewUp;

use BrewUp\Sales\Domain\SalesOrder;
use BrewUp\Sales\SharedKernel\CustomTypes\BeerName;
use BrewUp\Sales\SharedKernel\CustomTypes\Quantity;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderNumber;
use BrewUp\Sales\SharedKernel\Events\SalesOrderClosed;
use BrewUp\Sales\SharedKernel\Events\SalesOrderCreated;
use BrewUp\Sales\SharedKernel\Events\SalesOrderPrepared;
use Muflone\CustomTypes\Account;
use PHPUnit\Framework\TestCase;

/**
 * Tests for the SalesOrder aggregate to validate C# to PHP translation
 */
class SalesOrderTest extends TestCase
{
    private Account $testUser;

    protected function setUp(): void
    {
        $this->testUser = new Account('test-user-123', 'Test User');
    }

    public function testCanCreateSalesOrder(): void
    {
        $salesOrderId = SalesOrderId::create();
        $salesOrderNumber = SalesOrderNumber::create('SO-2025-001');
        $beerName = BeerName::create('IPA Delight');
        $quantity = Quantity::create(24);

        $salesOrder = SalesOrder::create(
            $salesOrderId,
            $salesOrderNumber,
            $beerName,
            $quantity,
            $this->testUser
        );

        // Verify aggregate state
        $this->assertEquals($salesOrderId, $salesOrder->getSalesOrderId());
        $this->assertEquals('SO-2025-001', $salesOrder->getSalesOrderNumber()->getValue());
        $this->assertEquals('IPA Delight', $salesOrder->getBeerName()->getValue());
        $this->assertEquals(24, $salesOrder->getQuantity()->getValue());
        $this->assertEquals('new', $salesOrder->getStatus());
        $this->assertEquals(1, $salesOrder->getVersion());
    }

    public function testCreateRaisesSalesOrderCreatedEvent(): void
    {
        $salesOrderId = SalesOrderId::create();
        $salesOrder = SalesOrder::create(
            $salesOrderId,
            SalesOrderNumber::create('SO-001'),
            BeerName::create('Stout'),
            Quantity::create(12),
            $this->testUser
        );

        $events = $salesOrder->getUncommittedEvents();

        $this->assertCount(1, $events);
        $this->assertInstanceOf(SalesOrderCreated::class, $events[0]);

        /** @var SalesOrderCreated $event */
        $event = $events[0];
        $this->assertEquals('SO-001', $event->salesOrderNumber->getValue());
        $this->assertEquals('Stout', $event->beerName->getValue());
        $this->assertEquals(12, $event->quantity->getValue());
    }

    public function testCanPrepareSalesOrder(): void
    {
        $salesOrder = $this->createTestSalesOrder();

        $salesOrder->prepare($this->testUser);

        $this->assertEquals('prepared', $salesOrder->getStatus());
        $this->assertEquals(2, $salesOrder->getVersion());
    }

    public function testPrepareRaisesSalesOrderPreparedEvent(): void
    {
        $salesOrder = $this->createTestSalesOrder();
        $salesOrder->clearUncommittedEvents(); // Clear creation event

        $salesOrder->prepare($this->testUser);

        $events = $salesOrder->getUncommittedEvents();
        $this->assertCount(1, $events);
        $this->assertInstanceOf(SalesOrderPrepared::class, $events[0]);
    }

    public function testCannotPrepareNonNewOrder(): void
    {
        $salesOrder = $this->createTestSalesOrder();
        $salesOrder->prepare($this->testUser);

        $this->expectException(\DomainException::class);
        $this->expectExceptionMessage('Cannot prepare sales order in status: prepared');

        $salesOrder->prepare($this->testUser);
    }

    public function testCanClosePreparedOrder(): void
    {
        $salesOrder = $this->createTestSalesOrder();
        $salesOrder->prepare($this->testUser);

        $salesOrder->close($this->testUser);

        $this->assertEquals('closed', $salesOrder->getStatus());
        $this->assertEquals(3, $salesOrder->getVersion());
    }

    public function testCloseRaisesSalesOrderClosedEvent(): void
    {
        $salesOrder = $this->createTestSalesOrder();
        $salesOrder->prepare($this->testUser);
        $salesOrder->clearUncommittedEvents();

        $salesOrder->close($this->testUser);

        $events = $salesOrder->getUncommittedEvents();
        $this->assertCount(1, $events);
        $this->assertInstanceOf(SalesOrderClosed::class, $events[0]);
    }

    public function testCannotCloseUnpreparedOrder(): void
    {
        $salesOrder = $this->createTestSalesOrder();

        $this->expectException(\DomainException::class);
        $this->expectExceptionMessage('Cannot close sales order that is not prepared');

        $salesOrder->close($this->testUser);
    }

    public function testClosingAlreadyClosedOrderIsIdempotent(): void
    {
        $salesOrder = $this->createTestSalesOrder();
        $salesOrder->prepare($this->testUser);
        $salesOrder->close($this->testUser);
        $salesOrder->clearUncommittedEvents();

        $versionBefore = $salesOrder->getVersion();

        // Should not throw exception
        $salesOrder->close($this->testUser);

        $this->assertEquals($versionBefore, $salesOrder->getVersion());
        $this->assertCount(0, $salesOrder->getUncommittedEvents());
    }

    public function testEventSourcingReconstruction(): void
    {
        // Create and modify order
        $salesOrder = $this->createTestSalesOrder();
        $salesOrder->prepare($this->testUser);
        $salesOrder->close($this->testUser);

        $originalNumber = $salesOrder->getSalesOrderNumber()->getValue();
        $originalBeer = $salesOrder->getBeerName()->getValue();
        $originalQuantity = $salesOrder->getQuantity()->getValue();
        $originalStatus = $salesOrder->getStatus();
        $originalVersion = $salesOrder->getVersion();

        // Get all events
        $events = $salesOrder->getUncommittedEvents();

        // Reconstruct from events
        $reconstructedOrder = new class extends SalesOrder {
            public function __construct() {
                parent::__construct();
            }
        };

        foreach ($events as $event) {
            $reconstructedOrder->applyEvent($event);
        }

        // Verify reconstructed state matches original
        $this->assertEquals($originalNumber, $reconstructedOrder->getSalesOrderNumber()->getValue());
        $this->assertEquals($originalBeer, $reconstructedOrder->getBeerName()->getValue());
        $this->assertEquals($originalQuantity, $reconstructedOrder->getQuantity()->getValue());
        $this->assertEquals($originalStatus, $reconstructedOrder->getStatus());
        $this->assertEquals($originalVersion, $reconstructedOrder->getVersion());
    }

    public function testCompleteLifecycle(): void
    {
        $salesOrderId = SalesOrderId::create();
        $salesOrder = SalesOrder::create(
            $salesOrderId,
            SalesOrderNumber::create('SO-LIFECYCLE-001'),
            BeerName::create('Pilsner Premium'),
            Quantity::create(50),
            $this->testUser
        );

        // Initial state
        $this->assertEquals('new', $salesOrder->getStatus());
        $this->assertCount(1, $salesOrder->getUncommittedEvents());

        // Prepare
        $salesOrder->prepare($this->testUser);
        $this->assertEquals('prepared', $salesOrder->getStatus());
        $this->assertCount(2, $salesOrder->getUncommittedEvents());

        // Close
        $salesOrder->close($this->testUser);
        $this->assertEquals('closed', $salesOrder->getStatus());
        $this->assertCount(3, $salesOrder->getUncommittedEvents());

        // Verify all events
        $events = $salesOrder->getUncommittedEvents();
        $this->assertInstanceOf(SalesOrderCreated::class, $events[0]);
        $this->assertInstanceOf(SalesOrderPrepared::class, $events[1]);
        $this->assertInstanceOf(SalesOrderClosed::class, $events[2]);
    }

    private function createTestSalesOrder(): SalesOrder
    {
        return SalesOrder::create(
            SalesOrderId::create(),
            SalesOrderNumber::create('TEST-001'),
            BeerName::create('Test Beer'),
            Quantity::create(10),
            $this->testUser
        );
    }
}
