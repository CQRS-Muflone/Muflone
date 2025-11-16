<?php

declare(strict_types=1);

namespace Muflone\Tests\Examples\BrewUp;

use BrewUp\Sales\Application\CommandHandlers\CloseSalesOrderHandler;
use BrewUp\Sales\Application\CommandHandlers\CreateSalesOrderHandler;
use BrewUp\Sales\Application\CommandHandlers\PrepareSalesOrderHandler;
use BrewUp\Sales\Domain\SalesOrder;
use BrewUp\Sales\SharedKernel\Commands\CloseSalesOrder;
use BrewUp\Sales\SharedKernel\Commands\CreateSalesOrder;
use BrewUp\Sales\SharedKernel\Commands\PrepareSalesOrder;
use BrewUp\Sales\SharedKernel\CustomTypes\BeerName;
use BrewUp\Sales\SharedKernel\CustomTypes\Quantity;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderNumber;
use BrewUp\Sales\SharedKernel\Events\SalesOrderClosed;
use BrewUp\Sales\SharedKernel\Events\SalesOrderCreated;
use BrewUp\Sales\SharedKernel\Events\SalesOrderPrepared;
use Muflone\Core\IDomainId;
use Muflone\CustomTypes\Account;
use Muflone\IAggregate;
use Muflone\Persistence\IRepository;
use PHPUnit\Framework\TestCase;
use Psr\Log\LoggerInterface;

/**
 * Tests for command handlers validating C# to PHP translation
 * Uses in-memory repository for testing
 */
class CommandHandlersTest extends TestCase
{
    private InMemoryRepository $repository;
    private LoggerInterface $logger;
    private Account $testUser;

    protected function setUp(): void
    {
        $this->repository = new InMemoryRepository();
        $this->logger = new TestLogger();
        $this->testUser = new Account('test-user', 'Test User');
    }

    public function testCreateSalesOrderHandler(): void
    {
        $handler = new CreateSalesOrderHandler($this->repository, $this->logger);

        $salesOrderId = SalesOrderId::create();
        $command = new CreateSalesOrder(
            $salesOrderId,
            SalesOrderNumber::create('SO-001'),
            BeerName::create('IPA'),
            Quantity::create(24),
            null,
            $this->testUser
        );

        $handler->handleAsync($command);

        // Verify aggregate was saved
        $savedAggregate = $this->repository->getById(SalesOrder::class, $salesOrderId);
        $this->assertNotNull($savedAggregate);
        $this->assertInstanceOf(SalesOrder::class, $savedAggregate);
        $this->assertEquals('new', $savedAggregate->getStatus());
    }

    public function testPrepareSalesOrderHandler(): void
    {
        // First create an order
        $salesOrderId = SalesOrderId::create();
        $createHandler = new CreateSalesOrderHandler($this->repository, $this->logger);
        $createCommand = new CreateSalesOrder(
            $salesOrderId,
            SalesOrderNumber::create('SO-002'),
            BeerName::create('Stout'),
            Quantity::create(12),
            null,
            $this->testUser
        );
        $createHandler->handleAsync($createCommand);

        // Now prepare it
        $prepareHandler = new PrepareSalesOrderHandler($this->repository, $this->logger);
        $prepareCommand = new PrepareSalesOrder($salesOrderId, null, $this->testUser);

        $prepareHandler->handleAsync($prepareCommand);

        // Verify order was prepared
        $preparedOrder = $this->repository->getById(SalesOrder::class, $salesOrderId);
        $this->assertNotNull($preparedOrder);
        $this->assertEquals('prepared', $preparedOrder->getStatus());
    }

    public function testCloseSalesOrderHandler(): void
    {
        // Create and prepare an order
        $salesOrderId = SalesOrderId::create();
        $createHandler = new CreateSalesOrderHandler($this->repository, $this->logger);
        $prepareHandler = new PrepareSalesOrderHandler($this->repository, $this->logger);

        $createCommand = new CreateSalesOrder(
            $salesOrderId,
            SalesOrderNumber::create('SO-003'),
            BeerName::create('Lager'),
            Quantity::create(6),
            null,
            $this->testUser
        );
        $createHandler->handleAsync($createCommand);

        $prepareCommand = new PrepareSalesOrder($salesOrderId, null, $this->testUser);
        $prepareHandler->handleAsync($prepareCommand);

        // Now close it
        $closeHandler = new CloseSalesOrderHandler($this->repository, $this->logger);
        $closeCommand = new CloseSalesOrder($salesOrderId, null, $this->testUser);

        $closeHandler->handleAsync($closeCommand);

        // Verify order was closed
        $closedOrder = $this->repository->getById(SalesOrder::class, $salesOrderId);
        $this->assertNotNull($closedOrder);
        $this->assertEquals('closed', $closedOrder->getStatus());
    }

    public function testPrepareSalesOrderHandlerThrowsWhenOrderNotFound(): void
    {
        $handler = new PrepareSalesOrderHandler($this->repository, $this->logger);
        $command = new PrepareSalesOrder(SalesOrderId::create(), null, $this->testUser);

        $this->expectException(\RuntimeException::class);
        $this->expectExceptionMessage('Sales order not found');

        $handler->handleAsync($command);
    }

    public function testCloseSalesOrderHandlerThrowsWhenOrderNotFound(): void
    {
        $handler = new CloseSalesOrderHandler($this->repository, $this->logger);
        $command = new CloseSalesOrder(SalesOrderId::create(), null, $this->testUser);

        $this->expectException(\RuntimeException::class);
        $this->expectExceptionMessage('Sales order not found');

        $handler->handleAsync($command);
    }

    public function testCompleteWorkflowThroughHandlers(): void
    {
        $salesOrderId = SalesOrderId::create();

        // Create
        $createHandler = new CreateSalesOrderHandler($this->repository, $this->logger);
        $createHandler->handleAsync(new CreateSalesOrder(
            $salesOrderId,
            SalesOrderNumber::create('SO-WORKFLOW-001'),
            BeerName::create('Ale'),
            Quantity::create(36),
            null,
            $this->testUser
        ));

        // Prepare
        $prepareHandler = new PrepareSalesOrderHandler($this->repository, $this->logger);
        $prepareHandler->handleAsync(new PrepareSalesOrder($salesOrderId, null, $this->testUser));

        // Close
        $closeHandler = new CloseSalesOrderHandler($this->repository, $this->logger);
        $closeHandler->handleAsync(new CloseSalesOrder($salesOrderId, null, $this->testUser));

        // Verify final state
        $finalOrder = $this->repository->getById(SalesOrder::class, $salesOrderId);
        $this->assertNotNull($finalOrder);
        $this->assertEquals('closed', $finalOrder->getStatus());
        $this->assertEquals(3, $finalOrder->getVersion());
    }
}

/**
 * In-memory repository for testing
 * Simulates event store behavior
 */
class InMemoryRepository implements IRepository
{
    /** @var array<string, array<object>> */
    private array $eventStreams = [];

    public function getById(string $aggregateClass, IDomainId $id): ?IAggregate
    {
        $streamKey = $id->getValue();

        if (!isset($this->eventStreams[$streamKey])) {
            return null;
        }

        // Reconstruct aggregate from events
        $aggregate = new $aggregateClass();

        foreach ($this->eventStreams[$streamKey] as $event) {
            $aggregate->applyEvent($event);
        }

        $aggregate->clearUncommittedEvents();

        return $aggregate;
    }

    public function getByIdAndVersion(string $aggregateClass, IDomainId $id, int $version): ?IAggregate
    {
        $aggregate = $this->getById($aggregateClass, $id);

        if ($aggregate === null || $aggregate->getVersion() !== $version) {
            return null;
        }

        return $aggregate;
    }

    public function save(IAggregate $aggregate, string $commitId, ?callable $updateHeaders = null): void
    {
        $streamKey = $aggregate->getId()->getValue();

        if (!isset($this->eventStreams[$streamKey])) {
            $this->eventStreams[$streamKey] = [];
        }

        // Append new events to stream
        $events = $aggregate->getUncommittedEvents();
        foreach ($events as $event) {
            $this->eventStreams[$streamKey][] = $event;
        }
    }
}

/**
 * Test logger that captures log messages
 */
class TestLogger implements LoggerInterface
{
    /** @var array<array{level: string, message: string, context: array}> */
    public array $logs = [];

    public function emergency(\Stringable|string $message, array $context = []): void
    {
        $this->log('emergency', $message, $context);
    }

    public function alert(\Stringable|string $message, array $context = []): void
    {
        $this->log('alert', $message, $context);
    }

    public function critical(\Stringable|string $message, array $context = []): void
    {
        $this->log('critical', $message, $context);
    }

    public function error(\Stringable|string $message, array $context = []): void
    {
        $this->log('error', $message, $context);
    }

    public function warning(\Stringable|string $message, array $context = []): void
    {
        $this->log('warning', $message, $context);
    }

    public function notice(\Stringable|string $message, array $context = []): void
    {
        $this->log('notice', $message, $context);
    }

    public function info(\Stringable|string $message, array $context = []): void
    {
        $this->log('info', $message, $context);
    }

    public function debug(\Stringable|string $message, array $context = []): void
    {
        $this->log('debug', $message, $context);
    }

    public function log($level, \Stringable|string $message, array $context = []): void
    {
        $this->logs[] = [
            'level' => $level,
            'message' => (string)$message,
            'context' => $context,
        ];
    }
}
