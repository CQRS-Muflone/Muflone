<?php

declare(strict_types=1);

namespace Muflone\Tests\Core;

use DateTimeImmutable;
use Muflone\Core\AggregateRoot;
use Muflone\CustomTypes\Account;
use Muflone\Messages\Events\DomainEvent;
use PHPUnit\Framework\TestCase;
use Ramsey\Uuid\Uuid;

final class TestAggregateId extends TestDomainId
{
}

final class TestAggregateCreated extends DomainEvent
{
    public function __construct(
        TestDomainId $aggregateId,
        public readonly string $name
    ) {
        parent::__construct($aggregateId);
    }
}

final class TestAggregateNameChanged extends DomainEvent
{
    public function __construct(
        TestDomainId $aggregateId,
        public readonly string $newName
    ) {
        parent::__construct($aggregateId);
    }
}

final class TestAggregate extends AggregateRoot
{
    private string $name = '';

    public static function create(TestDomainId $id, string $name): self
    {
        $aggregate = new self();
        $aggregate->raiseEvent(new TestAggregateCreated($id, $name));
        return $aggregate;
    }

    public function changeName(string $newName): void
    {
        if ($newName !== $this->name) {
            $this->raiseEvent(new TestAggregateNameChanged($this->id, $newName));
        }
    }

    public function getName(): string
    {
        return $this->name;
    }

    protected function applyTestAggregateCreated(TestAggregateCreated $event): void
    {
        $this->id = $event->getAggregateId();
        $this->name = $event->name;
    }

    protected function applyTestAggregateNameChanged(TestAggregateNameChanged $event): void
    {
        $this->name = $event->newName;
    }

    // Dispatcher method for the ConventionEventRouter
    protected function apply(object $event): void
    {
        match (true) {
            $event instanceof TestAggregateCreated => $this->applyTestAggregateCreated($event),
            $event instanceof TestAggregateNameChanged => $this->applyTestAggregateNameChanged($event),
            default => throw new \RuntimeException('Unknown event type: ' . get_class($event))
        };
    }
}

class AggregateRootTest extends TestCase
{
    public function testCanCreateAggregate(): void
    {
        $id = TestAggregateId::create(Uuid::uuid4()->toString());
        $aggregate = TestAggregate::create($id, 'Test Aggregate');

        $this->assertEquals($id->getValue(), $aggregate->getId()->getValue());
        $this->assertEquals('Test Aggregate', $aggregate->getName());
    }

    public function testRaisedEventsAreRecorded(): void
    {
        $id = TestAggregateId::create(Uuid::uuid4()->toString());
        $aggregate = TestAggregate::create($id, 'Test');

        $uncommittedEvents = $aggregate->getUncommittedEvents();

        $this->assertCount(1, $uncommittedEvents);
        $this->assertInstanceOf(TestAggregateCreated::class, $uncommittedEvents[0]);
    }

    public function testVersionIsIncrementedWhenEventIsApplied(): void
    {
        $id = TestAggregateId::create(Uuid::uuid4()->toString());
        $aggregate = TestAggregate::create($id, 'Test');

        $this->assertEquals(1, $aggregate->getVersion());

        $aggregate->changeName('New Name');

        $this->assertEquals(2, $aggregate->getVersion());
    }

    public function testEventsAreAppliedToAggregate(): void
    {
        $id = TestAggregateId::create(Uuid::uuid4()->toString());
        $aggregate = TestAggregate::create($id, 'Original Name');

        $this->assertEquals('Original Name', $aggregate->getName());

        $aggregate->changeName('Updated Name');

        $this->assertEquals('Updated Name', $aggregate->getName());
    }

    public function testClearUncommittedEventsRemovesEvents(): void
    {
        $id = TestAggregateId::create(Uuid::uuid4()->toString());
        $aggregate = TestAggregate::create($id, 'Test');

        $this->assertCount(1, $aggregate->getUncommittedEvents());

        $aggregate->clearUncommittedEvents();

        $this->assertCount(0, $aggregate->getUncommittedEvents());
    }

    public function testMultipleEventsAreRecorded(): void
    {
        $id = TestAggregateId::create(Uuid::uuid4()->toString());
        $aggregate = TestAggregate::create($id, 'Name 1');
        $aggregate->changeName('Name 2');
        $aggregate->changeName('Name 3');

        $events = $aggregate->getUncommittedEvents();

        $this->assertCount(3, $events);
        $this->assertInstanceOf(TestAggregateCreated::class, $events[0]);
        $this->assertInstanceOf(TestAggregateNameChanged::class, $events[1]);
        $this->assertInstanceOf(TestAggregateNameChanged::class, $events[2]);
    }

    public function testAggregateEqualityBasedOnId(): void
    {
        $id = TestAggregateId::create(Uuid::uuid4()->toString());
        $aggregate1 = TestAggregate::create($id, 'Test 1');
        $aggregate2 = TestAggregate::create($id, 'Test 2');

        $this->assertTrue($aggregate1->equals($aggregate2));
    }

    public function testAggregateInequalityWithDifferentIds(): void
    {
        $id1 = TestAggregateId::create(Uuid::uuid4()->toString());
        $id2 = TestAggregateId::create(Uuid::uuid4()->toString());
        $aggregate1 = TestAggregate::create($id1, 'Test 1');
        $aggregate2 = TestAggregate::create($id2, 'Test 2');

        $this->assertFalse($aggregate1->equals($aggregate2));
    }

    public function testGetSnapshotReturnsNullByDefault(): void
    {
        $id = TestAggregateId::create(Uuid::uuid4()->toString());
        $aggregate = TestAggregate::create($id, 'Test');

        $snapshot = $aggregate->getSnapshot();

        $this->assertNull($snapshot);
    }
}
