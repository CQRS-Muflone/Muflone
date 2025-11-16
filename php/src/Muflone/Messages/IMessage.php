<?php

declare(strict_types=1);

namespace Muflone\Messages;

/**
 * Interface IMessage
 * Base class of ICommand and IDomainEvent. A message that can be handled by the Command Processor/Dispatcher
 */
interface IMessage
{
    public function getMessageId(): string;
    public function setMessageId(string $messageId): void;

    /**
     * @return array<string, mixed>
     */
    public function getUserProperties(): array;

    /**
     * @param array<string, mixed> $userProperties
     */
    public function setUserProperties(array $userProperties): void;
}
