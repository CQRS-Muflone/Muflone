<?php

declare(strict_types=1);

namespace Muflone\Tests\Messages;

use DateTimeImmutable;
use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;
use Muflone\Messages\Events\DomainEvent;
use Muflone\Messages\Events\IntegrationEvent;
use Muflone\Tests\Core\TestDomainId;
use PHPUnit\Framework\TestCase;
use Ramsey\Uuid\Uuid;

final class TestDomainEvent extends DomainEvent
{
    public function __construct(
        TestDomainId $aggregateId,
        public readonly string $eventData,
        string $correlationId = '',
        ?Account $who = null
    ) {
        parent::__construct($aggregateId, $correlationId, $who);
    }
}

final class TestIntegrationEvent extends IntegrationEvent
{
    public function __construct(
        TestDomainId $aggregateId,
        public readonly string $externalData,
        string $correlationId = '',
        ?Account $who = null
    ) {
        parent::__construct($aggregateId, $correlationId, $who);
    }
}

class EventTest extends TestCase
{
    public function testCanCreateDomainEvent(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $event = new TestDomainEvent($aggregateId, 'test event data');

        $this->assertEquals($aggregateId, $event->getAggregateId());
        $this->assertEquals('test event data', $event->eventData);
        $this->assertNotEmpty($event->getMessageId());
    }

    public function testCanCreateIntegrationEvent(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $event = new TestIntegrationEvent($aggregateId, 'external data');

        $this->assertEquals($aggregateId, $event->getAggregateId());
        $this->assertEquals('external data', $event->externalData);
    }

    public function testEventHasDefaultAnonymousAccount(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $event = new TestDomainEvent($aggregateId, 'test');

        $headers = $event->getHeaders();
        $who = $headers->getWho();
        $this->assertEquals('Anonymous', $who->name);
    }

    public function testCanCreateEventWithCustomAccount(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $account = new Account('user-456', 'Jane Smith');
        $event = new TestDomainEvent($aggregateId, 'test', '', $account);

        $headers = $event->getHeaders();
        $who = $headers->getWho();
        $this->assertEquals('user-456', $who->id);
        $this->assertEquals('Jane Smith', $who->name);
    }

    public function testEventHasCorrelationId(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $correlationId = Uuid::uuid4()->toString();
        $event = new TestDomainEvent($aggregateId, 'test', $correlationId);

        $headers = $event->getHeaders();
        $this->assertEquals($correlationId, $headers->getCorrelationId());
    }

    public function testEventHasTimestamp(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $event = new TestDomainEvent($aggregateId, 'test');

        $headers = $event->getHeaders();
        $when = $headers->getWhen();
        $this->assertInstanceOf(When::class, $when);
        $this->assertInstanceOf(DateTimeImmutable::class, $when->getValue());
    }

    public function testEventHasVersion(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $event = new TestDomainEvent($aggregateId, 'test');

        $this->assertEquals(0, $event->getVersion());

        $event->setVersion(5);
        $this->assertEquals(5, $event->getVersion());
    }

    public function testEventHeadersContainAggregateType(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $event = new TestDomainEvent($aggregateId, 'test');

        $headers = $event->getHeaders();
        $this->assertEquals(TestDomainEvent::class, $headers->getAggregateType());
    }

    public function testCanSetCustomHeaderValues(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $event = new TestDomainEvent($aggregateId, 'test');

        $headers = $event->getHeaders();
        $headers->set('customKey', 'customValue');

        $this->assertTrue($headers->containsKey('customKey'));
        $this->assertEquals('customValue', $headers->get('customKey'));
    }

    public function testUserPropertiesContainCorrelationId(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $correlationId = Uuid::uuid4()->toString();
        $event = new TestDomainEvent($aggregateId, 'test', $correlationId);

        $userProperties = $event->getUserProperties();
        $this->assertArrayHasKey('CorrelationId', $userProperties);
        $this->assertEquals($correlationId, $userProperties['CorrelationId']);
    }
}
