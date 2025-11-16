<?php

declare(strict_types=1);

namespace Muflone\Factories;

use Muflone\Messages\Commands\ICommand;
use Muflone\Messages\Commands\ICommandHandlerAsync;

interface ICommandHandlerFactoryAsync
{
    /**
     * Create a command handler for the specified command type
     *
     * @template T of ICommand
     * @param class-string<T> $commandClass
     * @return ICommandHandlerAsync
     */
    public function createCommandHandler(string $commandClass): ICommandHandlerAsync;
}
