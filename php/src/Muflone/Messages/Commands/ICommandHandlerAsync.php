<?php

declare(strict_types=1);

namespace Muflone\Messages\Commands;

use Muflone\Messages\IMessage;
use Muflone\Messages\IMessageHandlerAsync;

interface ICommandHandlerAsync extends IMessageHandlerAsync
{
    /**
     * Handle the command asynchronously
     * Note: Accepts IMessage for interface compatibility, but implementations should type-hint ICommand
     *
     * @param IMessage $command
     * @return void
     */
    public function handleAsync(IMessage $command): void;
}
