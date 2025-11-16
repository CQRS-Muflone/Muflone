<?php

declare(strict_types=1);

namespace Muflone\Persistence;

use Muflone\Core\IDomainId;
use Muflone\IAggregate;

interface IRepository
{
    /**
     * Get an aggregate by its ID
     *
     * @template T of IAggregate
     * @param class-string<T> $aggregateClass
     * @param IDomainId $id
     * @return T|null
     */
    public function getById(string $aggregateClass, IDomainId $id): ?IAggregate;

    /**
     * Get an aggregate by its ID and version
     *
     * @template T of IAggregate
     * @param class-string<T> $aggregateClass
     * @param IDomainId $id
     * @param int $version
     * @return T|null
     */
    public function getByIdAndVersion(string $aggregateClass, IDomainId $id, int $version): ?IAggregate;

    /**
     * Save an aggregate
     *
     * @param IAggregate $aggregate
     * @param string $commitId
     * @param callable|null $updateHeaders Optional callback to update headers
     * @return void
     */
    public function save(IAggregate $aggregate, string $commitId, ?callable $updateHeaders = null): void;
}
