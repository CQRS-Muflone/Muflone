<?php

declare(strict_types=1);

namespace BrewUp\Sales\SharedKernel\CustomTypes;

use Muflone\Core\ValueObject;

/**
 * Quantity value object
 */
final class Quantity extends ValueObject
{
    public function __construct(
        private readonly int $value
    ) {
        if ($value < 0) {
            throw new \InvalidArgumentException('Quantity cannot be negative');
        }
    }

    public function getValue(): int
    {
        return $this->value;
    }

    public static function create(int $value): self
    {
        return new self($value);
    }

    protected function getEqualityComponents(): array
    {
        return [$this->value];
    }

    public function __toString(): string
    {
        return (string)$this->value;
    }
}
