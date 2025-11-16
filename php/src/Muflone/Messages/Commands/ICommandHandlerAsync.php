<?php

declare(strict_types=1);

namespace Muflone\Messages\Commands;

use Muflone\Messages\IMessageHandlerAsync;

interface ICommandHandlerAsync extends IMessageHandlerAsync
{
    /**
     * Handle the command asynchronously
     *
     * @param ICommand $command
     * @return void
     */
    public function handleAsync(ICommand $command): void;
}
