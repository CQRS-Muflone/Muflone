<?php

declare(strict_types=1);

namespace Muflone\Core;

abstract class DomainId implements IDomainId
{
    public function __construct(
        private readonly string $value
    ) {
    }

    public function getValue(): string
    {
        return $this->value;
    }

    public function equals(?self $other): bool
    {
        if ($other === null) {
            return false;
        }

        return get_class($this) === get_class($other) && $other->value === $this->value;
    }

    public function __toString(): string
    {
        return $this->value;
    }

    public function __hash(): string
    {
        return hash('sha256', $this->value);
    }
}
