<?php

declare(strict_types=1);

namespace Muflone\Core;

use InvalidArgumentException;

/**
 * Base class for Value Objects
 * Taken from: https://enterprisecraftsmanship.com/2017/08/28/value-object-a-better-implementation/
 *
 * The class overriding this just needs to return an array of values:
 * return [$this->field1, $this->field2, ..., $this->fieldN];
 *
 * In case of lists, it is enough to do this:
 * $components = [];
 * foreach ($this->items as $item) {
 *     $components[] = $item;
 * }
 * return $components;
 */
abstract class ValueObject
{
    /**
     * Returns the list of objects to compare for equality
     *
     * @return array<mixed>
     */
    abstract protected function getEqualityComponents(): array;

    public function equals(?self $other): bool
    {
        if ($other === null) {
            return false;
        }

        if (get_class($this) !== get_class($other)) {
            throw new InvalidArgumentException(
                sprintf(
                    'Invalid comparison of Value Objects of different types: %s and %s',
                    get_class($this),
                    get_class($other)
                )
            );
        }

        return $this->getEqualityComponents() === $other->getEqualityComponents();
    }

    public function __hash(): string
    {
        $hash = 1;
        foreach ($this->getEqualityComponents() as $component) {
            if ($component !== null) {
                $componentHash = crc32(serialize($component));
                $hash = $hash * 23 + $componentHash;
            }
        }
        return (string) $hash;
    }
}
