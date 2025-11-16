<?php

declare(strict_types=1);

namespace Muflone\CustomTypes;

final readonly class Account
{
    public function __construct(
        public string $id,
        public string $name
    ) {
    }
}
