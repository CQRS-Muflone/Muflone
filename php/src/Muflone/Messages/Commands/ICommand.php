<?php

declare(strict_types=1);

namespace Muflone\Messages\Commands;

use Muflone\Core\IDomainId;
use Muflone\Messages\IMessage;

interface ICommand extends IMessage
{
    public function getAggregateId(): IDomainId;
}
