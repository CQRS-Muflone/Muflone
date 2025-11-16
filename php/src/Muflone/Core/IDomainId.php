<?php

declare(strict_types=1);

namespace Muflone\Core;

interface IDomainId
{
    public function getValue(): string;
}
