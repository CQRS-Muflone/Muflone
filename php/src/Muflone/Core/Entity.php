<?php

declare(strict_types=1);

namespace Muflone\Core;

abstract class Entity
{
    protected readonly IDomainId $id;

    public function __construct(?IDomainId $id = null)
    {
        if ($id !== null) {
            $this->id = $id;
        }
    }

    public function getId(): IDomainId
    {
        return $this->id;
    }

    public function equals(?self $other): bool
    {
        if ($other === null) {
            return false;
        }

        return get_class($this) === get_class($other)
            && $other->id->getValue() === $this->id->getValue();
    }

    public function __hash(): string
    {
        return hash('sha256', $this->id->getValue());
    }
}
