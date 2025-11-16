<?php

declare(strict_types=1);

namespace Muflone\Persistence;

use JsonException;

class Serializer implements ISerializer
{
    /**
     * @inheritDoc
     * @throws JsonException
     */
    public function deserialize(string $serializedData, string $className): ?object
    {
        $data = json_decode($serializedData, true, 512, JSON_THROW_ON_ERROR);

        if ($data === null) {
            return null;
        }

        // Check if the data contains type information
        if (isset($data['$type'])) {
            $className = $data['$type'];
            unset($data['$type']);
        }

        // Create instance using reflection to avoid constructor
        $reflection = new \ReflectionClass($className);
        $instance = $reflection->newInstanceWithoutConstructor();

        // Set properties
        foreach ($data as $key => $value) {
            if ($reflection->hasProperty($key)) {
                $property = $reflection->getProperty($key);
                if (!$property->isInitialized($instance) || !$property->isReadOnly()) {
                    $property->setAccessible(true);
                    $property->setValue($instance, $this->deserializeValue($value));
                }
            }
        }

        return $instance;
    }

    /**
     * @inheritDoc
     * @throws JsonException
     */
    public function serialize(object $data): string
    {
        $serialized = $this->serializeObject($data);
        return json_encode($serialized, JSON_THROW_ON_ERROR | JSON_PRESERVE_ZERO_FRACTION);
    }

    /**
     * Serialize an object to an array with type information
     *
     * @param object $object
     * @return array<string, mixed>
     */
    private function serializeObject(object $object): array
    {
        $result = ['$type' => get_class($object)];

        $reflection = new \ReflectionClass($object);
        $properties = $reflection->getProperties();

        foreach ($properties as $property) {
            $property->setAccessible(true);
            $value = $property->getValue($object);
            $result[$property->getName()] = $this->serializeValue($value);
        }

        return $result;
    }

    /**
     * @param mixed $value
     * @return mixed
     */
    private function serializeValue(mixed $value): mixed
    {
        if (is_object($value)) {
            return $this->serializeObject($value);
        }

        if (is_array($value)) {
            return array_map(fn($item) => $this->serializeValue($item), $value);
        }

        return $value;
    }

    /**
     * @param mixed $value
     * @return mixed
     */
    private function deserializeValue(mixed $value): mixed
    {
        if (is_array($value) && isset($value['$type'])) {
            $className = $value['$type'];
            unset($value['$type']);
            return $this->deserialize(json_encode($value, JSON_THROW_ON_ERROR), $className);
        }

        if (is_array($value)) {
            return array_map(fn($item) => $this->deserializeValue($item), $value);
        }

        return $value;
    }
}
