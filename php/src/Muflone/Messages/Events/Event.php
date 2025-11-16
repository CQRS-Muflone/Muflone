<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use DateTimeImmutable;
use Muflone\Core\IDomainId;
use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;
use Muflone\Messages\HeadersNames;
use Ramsey\Uuid\Uuid;

class Event implements IEvent
{
    protected IDomainId $aggregateId;
    protected EventHeaders $headers;
    protected int $version = 0;
    protected string $messageId;
    /** @var array<string, mixed> */
    protected array $userProperties = [];

    public function __construct(
        IDomainId $aggregateId,
        string $correlationId = '',
        ?Account $who = null,
        ?When $when = null
    ) {
        $this->aggregateId = $aggregateId;
        $this->messageId = Uuid::uuid4()->toString();

        $who = $who ?? new Account('', 'Anonymous');
        $when = $when ?? new When(new DateTimeImmutable());

        $this->headers = new EventHeaders();
        $this->headers->setCorrelationId($correlationId);
        $this->headers->setAggregateType(static::class);
        $this->headers->setWho($who);
        $this->headers->setWhen($when);

        $this->userProperties = [
            HeadersNames::CORRELATION_ID => $correlationId,
        ];
    }

    public function getAggregateId(): IDomainId
    {
        return $this->aggregateId;
    }

    public function getHeaders(): EventHeaders
    {
        return $this->headers;
    }

    public function setHeaders(EventHeaders $headers): void
    {
        $this->headers = $headers;
    }

    public function getVersion(): int
    {
        return $this->version;
    }

    public function setVersion(int $version): void
    {
        $this->version = $version;
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
}
