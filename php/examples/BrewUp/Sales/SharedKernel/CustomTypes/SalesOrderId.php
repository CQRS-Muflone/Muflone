<?php

declare(strict_types=1);

namespace BrewUp\Sales\SharedKernel\CustomTypes;

use Muflone\Core\DomainId;

/**
 * Sales Order unique identifier
 */
final class SalesOrderId extends DomainId
{
    public static function create(): self
    {
        return new self(\Ramsey\Uuid\Uuid::uuid4()->toString());
    }

    public static function fromString(string $id): self
    {
        return new self($id);
    }
}
