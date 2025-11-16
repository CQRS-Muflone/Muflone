<?php

declare(strict_types=1);

namespace Muflone\Messages\Commands;

use DateTimeImmutable;
use Muflone\Core\IDomainId;
use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;
use Ramsey\Uuid\Uuid;

/**
 * A command is an imperative instruction to do something.
 * We expect only one receiver of a command because it is point-to-point
 */
abstract class Command implements ICommand
{
    protected IDomainId $aggregateId;
    protected string $messageId;
    /** @var array<string, mixed> */
    protected array $userProperties = [];
    protected Account $who;
    protected When $when;

    public function __construct(
        IDomainId $aggregateId,
        ?string $commitId = null,
        ?Account $who = null,
        ?When $when = null
    ) {
        $this->aggregateId = $aggregateId;
        $this->messageId = $commitId ?? Uuid::uuid4()->toString();
        $this->who = $who ?? new Account(Uuid::uuid4()->toString(), 'Anonymous');
        $this->when = $when ?? new When(new DateTimeImmutable());
    }

    public function getAggregateId(): IDomainId
    {
        return $this->aggregateId;
    }

    public function getMessageId(): string
    {
        return $this->messageId;
    }

    public function setMessageId(string $messageId): void
    {
        $this->messageId = $messageId;
    }

    /**
     * @return array<string, mixed>
     */
    public function getUserProperties(): array
    {
        return $this->userProperties;
    }

    /**
     * @param array<string, mixed> $userProperties
     */
    public function setUserProperties(array $userProperties): void
    {
        $this->userProperties = $userProperties;
    }

    public function getWho(): Account
    {
        return $this->who;
    }

    public function getWhen(): When
    {
        return $this->when;
    }
}
