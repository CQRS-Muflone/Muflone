<?php

declare(strict_types=1);

namespace BrewUp\Sales\SharedKernel\CustomTypes;

use Muflone\Core\ValueObject;

/**
 * Beer Name value object
 */
final class BeerName extends ValueObject
{
    public function __construct(
        private readonly string $value
    ) {
        if (empty($value)) {
            throw new \InvalidArgumentException('Beer name cannot be empty');
        }
    }

    public function getValue(): string
    {
        return $this->value;
    }

    public static function create(string $value): self
    {
        return new self($value);
    }

    protected function getEqualityComponents(): array
    {
        return [$this->value];
    }

    public function __toString(): string
    {
        return $this->value;
    }
}
