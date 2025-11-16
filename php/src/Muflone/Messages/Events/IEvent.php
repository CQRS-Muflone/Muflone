<?php

declare(strict_types=1);

namespace Muflone\Messages\Events;

use Muflone\Messages\IMessage;

interface IEvent extends IMessage
{
    public function getVersion(): int;
    public function setVersion(int $version): void;
}
