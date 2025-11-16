<?php

declare(strict_types=1);

namespace Muflone\Tests\Integration;

use DateTimeImmutable;
use Muflone\Core\AggregateRoot;
use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;
use Muflone\Messages\Commands\Command;
use Muflone\Messages\Events\DomainEvent;
use Muflone\Persistence\Serializer;
use Muflone\Tests\Core\TestDomainId;
use PHPUnit\Framework\TestCase;
use Ramsey\Uuid\Uuid;

// Domain Events
final class OrderCreated extends DomainEvent
{
    public function __construct(
        TestDomainId $orderId,
        public readonly string $customerName,
        public readonly float $totalAmount,
        string $correlationId = '',
        ?Account $who = null,
        ?When $when = null
    ) {
        parent::__construct($orderId, $correlationId, $who, $when);
    }
}

final class OrderItemAdded extends DomainEvent
{
    public function __construct(
        TestDomainId $orderId,
        public readonly string $productName,
        public readonly int $quantity,
        public readonly float $price
    ) {
        parent::__construct($orderId);
    }
}

final class OrderCompleted extends DomainEvent
{
    public function __construct(
        TestDomainId $orderId,
        public readonly DateTimeImmutable $completedAt
    ) {
        parent::__construct($orderId);
    }
}

// Commands
final class CreateOrder extends Command
{
    public function __construct(
        TestDomainId $orderId,
        public readonly string $customerName,
        ?string $commitId = null,
        ?Account $who = null
    ) {
        parent::__construct($orderId, $commitId, $who);
    }
}

final class AddOrderItem extends Command
{
    public function __construct(
        TestDomainId $orderId,
        public readonly string $productName,
        public readonly int $quantity,
        public readonly float $price
    ) {
        parent::__construct($orderId);
    }
}

// Aggregate
final class Order extends AggregateRoot
{
    private string $customerName = '';
    private float $totalAmount = 0.0;
    private array $items = [];
    private string $status = 'pending';

    public static function create(TestDomainId $orderId, string $customerName, Account $who): self
    {
        $order = new self();
        $order->raiseEvent(new OrderCreated($orderId, $customerName, 0.0, '', $who));
        return $order;
    }

    public function addItem(string $productName, int $quantity, float $price): void
    {
        if ($this->status === 'completed') {
            throw new \RuntimeException('Cannot add items to completed order');
        }

        $this->raiseEvent(new OrderItemAdded($this->id, $productName, $quantity, $price));
    }

    public function complete(): void
    {
        if ($this->status === 'completed') {
            return;
        }

        $this->raiseEvent(new OrderCompleted($this->id, new DateTimeImmutable()));
    }

    protected function applyOrderCreated(OrderCreated $event): void
    {
        $this->id = $event->getAggregateId();
        $this->customerName = $event->customerName;
        $this->totalAmount = $event->totalAmount;
        $this->status = 'pending';
    }

    protected function applyOrderItemAdded(OrderItemAdded $event): void
    {
        $this->items[] = [
            'product' => $event->productName,
            'quantity' => $event->quantity,
            'price' => $event->price,
        ];
        $this->totalAmount += $event->quantity * $event->price;
    }

    protected function applyOrderCompleted(OrderCompleted $event): void
    {
        $this->status = 'completed';
    }

    public function getCustomerName(): string
    {
        return $this->customerName;
    }

    public function getTotalAmount(): float
    {
        return $this->totalAmount;
    }

    public function getStatus(): string
    {
        return $this->status;
    }

    public function getItemCount(): int
    {
        return count($this->items);
    }
}

/**
 * Integration test demonstrating the complete CQRS/Event Sourcing flow
 */
class CommandEventFlowTest extends TestCase
{
    public function testCompleteOrderWorkflow(): void
    {
        // Create order
        $orderId = TestDomainId::create(Uuid::uuid4()->toString());
        $who = new Account('user-123', 'John Smith');
        $order = Order::create($orderId, 'ACME Corp', $who);

        // Verify initial state
        $this->assertEquals('ACME Corp', $order->getCustomerName());
        $this->assertEquals(0.0, $order->getTotalAmount());
        $this->assertEquals('pending', $order->getStatus());
        $this->assertEquals(1, $order->getVersion());

        // Add items
        $order->addItem('Laptop', 2, 1200.00);
        $order->addItem('Mouse', 5, 25.00);
        $order->addItem('Keyboard', 2, 75.00);

        // Verify updated state
        $this->assertEquals(2675.00, $order->getTotalAmount()); // (2*1200) + (5*25) + (2*75)
        $this->assertEquals(3, $order->getItemCount());
        $this->assertEquals(4, $order->getVersion()); // 1 created + 3 items added

        // Complete order
        $order->complete();
        $this->assertEquals('completed', $order->getStatus());
        $this->assertEquals(5, $order->getVersion());

        // Verify uncommitted events
        $events = $order->getUncommittedEvents();
        $this->assertCount(5, $events);
        $this->assertInstanceOf(OrderCreated::class, $events[0]);
        $this->assertInstanceOf(OrderItemAdded::class, $events[1]);
        $this->assertInstanceOf(OrderItemAdded::class, $events[2]);
        $this->assertInstanceOf(OrderItemAdded::class, $events[3]);
        $this->assertInstanceOf(OrderCompleted::class, $events[4]);
    }

    public function testCannotAddItemsToCompletedOrder(): void
    {
        $orderId = TestDomainId::create(Uuid::uuid4()->toString());
        $who = new Account('user-123', 'Jane Doe');
        $order = Order::create($orderId, 'Test Corp', $who);

        $order->addItem('Product', 1, 100.00);
        $order->complete();

        $this->expectException(\RuntimeException::class);
        $this->expectExceptionMessage('Cannot add items to completed order');

        $order->addItem('Another Product', 1, 50.00);
    }

    public function testEventSourcingReconstruction(): void
    {
        // Create and modify order
        $orderId = TestDomainId::create(Uuid::uuid4()->toString());
        $who = new Account('user-789', 'Bob Wilson');
        $order = Order::create($orderId, 'Tech Solutions', $who);
        $order->addItem('Server', 1, 5000.00);
        $order->addItem('License', 10, 100.00);

        // Get events
        $events = $order->getUncommittedEvents();

        // Reconstruct from events
        $reconstructedOrder = new Order();
        foreach ($events as $event) {
            $reconstructedOrder->applyEvent($event);
        }

        // Verify reconstructed state matches original
        $this->assertEquals($order->getCustomerName(), $reconstructedOrder->getCustomerName());
        $this->assertEquals($order->getTotalAmount(), $reconstructedOrder->getTotalAmount());
        $this->assertEquals($order->getItemCount(), $reconstructedOrder->getItemCount());
        $this->assertEquals($order->getVersion(), $reconstructedOrder->getVersion());
    }

    public function testSerializationOfCompleteWorkflow(): void
    {
        $serializer = new Serializer();
        $orderId = TestDomainId::create(Uuid::uuid4()->toString());
        $who = new Account('user-999', 'Alice Cooper');

        // Create order and add items
        $order = Order::create($orderId, 'Music Store', $who);
        $order->addItem('Guitar', 1, 800.00);
        $order->addItem('Amplifier', 1, 500.00);

        // Serialize all events
        $events = $order->getUncommittedEvents();
        $serializedEvents = [];

        foreach ($events as $event) {
            $serializedEvents[] = $serializer->serialize($event);
        }

        $this->assertCount(3, $serializedEvents);

        // Deserialize and verify
        $deserializedEvent = $serializer->deserialize($serializedEvents[0], OrderCreated::class);
        $this->assertInstanceOf(OrderCreated::class, $deserializedEvent);
        $this->assertEquals('Music Store', $deserializedEvent->customerName);

        $deserializedItem = $serializer->deserialize($serializedEvents[1], OrderItemAdded::class);
        $this->assertInstanceOf(OrderItemAdded::class, $deserializedItem);
        $this->assertEquals('Guitar', $deserializedItem->productName);
        $this->assertEquals(800.00, $deserializedItem->price);
    }

    public function testCommandAndEventMetadata(): void
    {
        $orderId = TestDomainId::create(Uuid::uuid4()->toString());
        $account = new Account('admin-001', 'Administrator');
        $commitId = Uuid::uuid4()->toString();

        $command = new CreateOrder($orderId, 'Test Customer', $commitId, $account);

        // Verify command metadata
        $this->assertEquals($commitId, $command->getMessageId());
        $this->assertEquals('admin-001', $command->getWho()->id);
        $this->assertEquals('Administrator', $command->getWho()->name);
        $this->assertInstanceOf(When::class, $command->getWhen());

        // Create order and verify event metadata
        $order = Order::create($orderId, $command->customerName, $command->getWho());
        $events = $order->getUncommittedEvents();
        $createdEvent = $events[0];

        $this->assertInstanceOf(OrderCreated::class, $createdEvent);
        $this->assertEquals('admin-001', $createdEvent->getHeaders()->getWho()->id);
        $this->assertInstanceOf(When::class, $createdEvent->getHeaders()->getWhen());
    }
}
