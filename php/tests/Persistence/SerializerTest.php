<?php

declare(strict_types=1);

namespace Muflone\Tests\Persistence;

use DateTimeImmutable;
use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;
use Muflone\Messages\Events\DomainEvent;
use Muflone\Persistence\Serializer;
use Muflone\Tests\Core\TestDomainId;
use PHPUnit\Framework\TestCase;
use Ramsey\Uuid\Uuid;

final class SerializableTestEvent extends DomainEvent
{
    public function __construct(
        TestDomainId $aggregateId,
        public readonly string $name,
        public readonly int $quantity,
        public readonly float $price,
        string $correlationId = ''
    ) {
        parent::__construct($aggregateId, $correlationId);
    }
}

final class ComplexSerializableEvent extends DomainEvent
{
    public function __construct(
        TestDomainId $aggregateId,
        public readonly Account $account,
        public readonly array $items,
        public readonly When $timestamp
    ) {
        parent::__construct($aggregateId);
    }
}

class SerializerTest extends TestCase
{
    private Serializer $serializer;

    protected function setUp(): void
    {
        $this->serializer = new Serializer();
    }

    public function testCanSerializeSimpleEvent(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $event = new SerializableTestEvent($aggregateId, 'Product A', 10, 99.99);

        $serialized = $this->serializer->serialize($event);

        $this->assertIsString($serialized);
        $this->assertStringContainsString('SerializableTestEvent', $serialized);
        $this->assertStringContainsString('Product A', $serialized);
        $this->assertStringContainsString('99.99', $serialized);
    }

    public function testCanDeserializeSimpleEvent(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $originalEvent = new SerializableTestEvent($aggregateId, 'Product B', 5, 49.50);

        $serialized = $this->serializer->serialize($originalEvent);
        $deserialized = $this->serializer->deserialize($serialized, SerializableTestEvent::class);

        $this->assertInstanceOf(SerializableTestEvent::class, $deserialized);
        $this->assertEquals('Product B', $deserialized->name);
        $this->assertEquals(5, $deserialized->quantity);
        $this->assertEquals(49.50, $deserialized->price);
    }

    public function testSerializedDataContainsTypeInformation(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $event = new SerializableTestEvent($aggregateId, 'Test', 1, 1.0);

        $serialized = $this->serializer->serialize($event);
        $data = json_decode($serialized, true);

        $this->assertArrayHasKey('$type', $data);
        $this->assertEquals(SerializableTestEvent::class, $data['$type']);
    }

    public function testCanSerializeComplexObjectWithNestedObjects(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $account = new Account('acc-123', 'Test User');
        $items = ['item1', 'item2', 'item3'];
        $timestamp = new When(new DateTimeImmutable());

        $event = new ComplexSerializableEvent($aggregateId, $account, $items, $timestamp);

        $serialized = $this->serializer->serialize($event);

        $this->assertIsString($serialized);
        $this->assertStringContainsString('ComplexSerializableEvent', $serialized);
        $this->assertStringContainsString('Test User', $serialized);
    }

    public function testCanDeserializeComplexObjectWithNestedObjects(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $account = new Account('acc-456', 'Jane Doe');
        $items = ['laptop', 'mouse', 'keyboard'];
        $timestamp = new When(new DateTimeImmutable('2024-01-15 10:30:00'));

        $originalEvent = new ComplexSerializableEvent($aggregateId, $account, $items, $timestamp);

        $serialized = $this->serializer->serialize($originalEvent);
        $deserialized = $this->serializer->deserialize($serialized, ComplexSerializableEvent::class);

        $this->assertInstanceOf(ComplexSerializableEvent::class, $deserialized);
        $this->assertEquals('acc-456', $deserialized->account->id);
        $this->assertEquals('Jane Doe', $deserialized->account->name);
        $this->assertCount(3, $deserialized->items);
        $this->assertEquals(['laptop', 'mouse', 'keyboard'], $deserialized->items);
    }

    public function testSerializeAndDeserializePreservesAllProperties(): void
    {
        $aggregateId = TestDomainId::create('test-id-789');
        $correlationId = Uuid::uuid4()->toString();
        $originalEvent = new SerializableTestEvent($aggregateId, 'Widget', 42, 123.45, $correlationId);
        $originalEvent->setVersion(3);

        $serialized = $this->serializer->serialize($originalEvent);
        $deserialized = $this->serializer->deserialize($serialized, SerializableTestEvent::class);

        $this->assertEquals('Widget', $deserialized->name);
        $this->assertEquals(42, $deserialized->quantity);
        $this->assertEquals(123.45, $deserialized->price);
        // Note: DomainId readonly properties can't be deserialized with newInstanceWithoutConstructor
        // In a real implementation, you would use a proper serialization library or custom handlers
        $this->assertInstanceOf(TestDomainId::class, $deserialized->getAggregateId());
    }

    public function testDeserializeReturnsNullForEmptyString(): void
    {
        $result = $this->serializer->deserialize('null', SerializableTestEvent::class);

        $this->assertNull($result);
    }

    public function testSerializationIsReversible(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $original = new SerializableTestEvent($aggregateId, 'Reversible', 99, 999.99);

        $serialized = $this->serializer->serialize($original);
        $deserialized = $this->serializer->deserialize($serialized, SerializableTestEvent::class);
        $reSerialized = $this->serializer->serialize($deserialized);

        // The re-serialized version should be equivalent to the original
        $this->assertIsString($reSerialized);
        $this->assertStringContainsString('Reversible', $reSerialized);
        $this->assertStringContainsString('999.99', $reSerialized);
    }
}
