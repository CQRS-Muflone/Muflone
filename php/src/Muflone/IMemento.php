<?php

declare(strict_types=1);

namespace Muflone;

interface IMemento
{
    public function getId(): string;
    public function setId(string $id): void;

    public function getVersion(): int;
    public function setVersion(int $version): void;
}
