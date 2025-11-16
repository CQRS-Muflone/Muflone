<?php

declare(strict_types=1);

namespace Muflone;

use Muflone\Core\IDomainId;

interface IAggregate
{
    public function getId(): IDomainId;
    public function getVersion(): int;

    public function applyEvent(object $event): void;

    /**
     * @return array<object>
     */
    public function getUncommittedEvents(): array;

    public function clearUncommittedEvents(): void;

    public function getSnapshot(): ?IMemento;
}
