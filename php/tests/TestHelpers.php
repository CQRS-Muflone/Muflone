<?php

declare(strict_types=1);

namespace Muflone\Tests\Core;

use Muflone\Core\DomainId;

/**
 * Test helper classes used across multiple test files
 */
class TestDomainId extends DomainId
{
    public static function create(string $value): self
    {
        return new self($value);
    }
}

class AnotherTestDomainId extends DomainId
{
    public static function create(string $value): self
    {
        return new self($value);
    }
}
