<?php

declare(strict_types=1);

namespace Muflone\Messages\Commands;

use Muflone\Messages\IMessage;
use Muflone\Persistence\IRepository;
use Psr\Log\LoggerInterface;

/**
 * Base class for command handlers
 *
 * @template TCommand of ICommand
 */
abstract class CommandHandlerAsync implements ICommandHandlerAsync
{
    public function __construct(
        protected readonly IRepository $repository,
        protected readonly LoggerInterface $logger
    ) {
    }

    /**
     * Handle the command asynchronously
     *
     * @param ICommand $command
     * @return void
     */
    abstract public function handleAsync(IMessage $command): void;
}
