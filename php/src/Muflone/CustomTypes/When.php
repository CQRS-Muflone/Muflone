<?php

declare(strict_types=1);

namespace Muflone\CustomTypes;

use DateTimeImmutable;

final readonly class When
{
    private int $microseconds;

    public function __construct(DateTimeImmutable|int $value)
    {
        if ($value instanceof DateTimeImmutable) {
            $this->microseconds = (int)($value->getTimestamp() * 1000000 + (int)$value->format('u'));
        } else {
            $this->microseconds = $value;
        }
    }

    public function getValue(): DateTimeImmutable
    {
        $timestamp = intdiv($this->microseconds, 1000000);
        $microseconds = $this->microseconds % 1000000;

        $dateTime = DateTimeImmutable::createFromFormat('U', (string)$timestamp);
        if ($dateTime === false) {
            $dateTime = new DateTimeImmutable('@' . $timestamp);
        }

        return $dateTime->modify(sprintf('+%d microseconds', $microseconds));
    }

    public function getMicroseconds(): int
    {
        return $this->microseconds;
    }
}
