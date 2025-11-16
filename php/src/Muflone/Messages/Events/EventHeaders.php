<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use Muflone\CustomTypes\Account;
use Muflone\CustomTypes\When;
use Muflone\Messages\HeadersNames;

final class EventHeaders
{
    /** @var array<string, string> */
    private array $standards = [];

    /** @var array<string, string> */
    private array $customs = [];

    public function getCorrelationId(): string
    {
        return $this->standards[HeadersNames::CORRELATION_ID] ?? '';
    }

    public function setCorrelationId(string $correlationId): void
    {
        $this->standards[HeadersNames::CORRELATION_ID] = $correlationId;
    }

    public function getWho(): Account
    {
        return new Account(
            $this->standards[HeadersNames::ACCOUNT_ID] ?? '',
            $this->standards[HeadersNames::ACCOUNT_NAME] ?? ''
        );
    }

    public function setWho(Account $who): void
    {
        $this->standards[HeadersNames::ACCOUNT_ID] = $who->id;
        $this->standards[HeadersNames::ACCOUNT_NAME] = $who->name;
    }

    public function getWhen(): When
    {
        $microseconds = (int)($this->standards[HeadersNames::WHEN] ?? 0);
        return new When($microseconds);
    }

    public function setWhen(When $when): void
    {
        $this->standards[HeadersNames::WHEN] = (string)$when->getMicroseconds();
    }

    public function getAggregateType(): string
    {
        return $this->standards[HeadersNames::AGGREGATE_TYPE] ?? '';
    }

    public function setAggregateType(string $aggregateType): void
    {
        $this->standards[HeadersNames::AGGREGATE_TYPE] = $aggregateType;
    }

    public function containsKey(string $key): bool
    {
        return isset($this->standards[$key]) || isset($this->customs[$key]);
    }

    public function get(string $key): ?string
    {
        return $this->customs[$key] ?? null;
    }

    public function set(string $key, string $value): void
    {
        $this->customs[$key] = $value;
    }

    /**
     * @return array<string, string>
     */
    public function getStandards(): array
    {
        return $this->standards;
    }

    /**
     * @return array<string, string>
     */
    public function getCustoms(): array
    {
        return $this->customs;
    }
}
