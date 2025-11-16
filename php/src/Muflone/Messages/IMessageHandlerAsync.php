<?php

declare(strict_types=1);

namespace Muflone\Messages;

interface IMessageHandlerAsync
{
    /**
     * Handle the message asynchronously
     *
     * @param IMessage $message
     * @return void
     */
    public function handleAsync(IMessage $message): void;
}
