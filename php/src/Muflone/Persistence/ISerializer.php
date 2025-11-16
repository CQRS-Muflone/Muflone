<?php

declare(strict_types=1);

namespace Muflone\Persistence;

interface ISerializer
{
    /**
     * Deserialize data from a string
     *
     * @template T
     * @param string $serializedData
     * @param class-string<T> $className
     * @return T|null
     */
    public function deserialize(string $serializedData, string $className): ?object;

    /**
     * Serialize data to a string
     *
     * @param object $data
     * @return string
     */
    public function serialize(object $data): string;
}
