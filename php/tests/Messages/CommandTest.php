<?php

declare(strict_types=1);

namespace Muflone\Tests\Messages;

use DateTimeImmutable;
use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;
use Muflone\Messages\Commands\Command;
use Muflone\Tests\Core\TestDomainId;
use PHPUnit\Framework\TestCase;
use Ramsey\Uuid\Uuid;

final class TestCommand extends Command
{
    public function __construct(
        TestDomainId $aggregateId,
        public readonly string $data,
        ?string $commitId = null,
        ?Account $who = null,
        ?When $when = null
    ) {
        parent::__construct($aggregateId, $commitId, $who, $when);
    }
}

class CommandTest extends TestCase
{
    public function testCanCreateCommand(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $command = new TestCommand($aggregateId, 'test data');

        $this->assertEquals($aggregateId, $command->getAggregateId());
        $this->assertEquals('test data', $command->data);
        $this->assertNotEmpty($command->getMessageId());
    }

    public function testCommandHasDefaultAnonymousAccount(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $command = new TestCommand($aggregateId, 'test');

        $who = $command->getWho();
        $this->assertEquals('Anonymous', $who->name);
    }

    public function testCanCreateCommandWithCustomAccount(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $account = new Account('user-123', 'John Doe');
        $command = new TestCommand($aggregateId, 'test', null, $account);

        $who = $command->getWho();
        $this->assertEquals('user-123', $who->id);
        $this->assertEquals('John Doe', $who->name);
    }

    public function testCanCreateCommandWithCustomCommitId(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $commitId = Uuid::uuid4()->toString();
        $command = new TestCommand($aggregateId, 'test', $commitId);

        $this->assertEquals($commitId, $command->getMessageId());
    }

    public function testCommandHasTimestamp(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $command = new TestCommand($aggregateId, 'test');

        $when = $command->getWhen();
        $this->assertInstanceOf(When::class, $when);
        $this->assertInstanceOf(DateTimeImmutable::class, $when->getValue());
    }

    public function testCanSetUserProperties(): void
    {
        $aggregateId = TestDomainId::create(Uuid::uuid4()->toString());
        $command = new TestCommand($aggregateId, 'test');

        $command->setUserProperties(['key1' => 'value1', 'key2' => 'value2']);

        $properties = $command->getUserProperties();
        $this->assertEquals('value1', $properties['key1']);
        $this->assertEquals('value2', $properties['key2']);
    }
}
