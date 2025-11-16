<?php

declare(strict_types=1);

namespace Muflone\Persistence;

use JsonException;

/**
 * Event and Aggregate Serializer for Event Sourcing
 *
 * C# TO PHP TRANSLATION NOTES:
 * ============================
 *
 * PURPOSE:
 * --------
 * Serializes events and aggregates to JSON for storage in the event store.
 * Deserializes them back to PHP objects when reconstructing aggregate state.
 *
 * THE READONLY PROPERTY CHALLENGE:
 * ---------------------------------
 * This is a MAJOR PHP-specific challenge not present in C#.
 *
 * PROBLEM:
 * Events use readonly properties for immutability:
 *   final readonly class SalesOrderCreated {
 *       public function __construct(
 *           public readonly SalesOrderId $aggregateId,
 *           public readonly SalesOrderNumber $salesOrderNumber,
 *           // ...
 *       ) {}
 *   }
 *
 * Readonly properties can ONLY be set:
 * 1. In the constructor
 * 2. Once per property lifetime
 *
 * But when deserializing from event store, we:
 * 1. Need to create object without calling constructor (don't have original parameters)
 * 2. Need to set readonly properties from stored data
 *
 * SOLUTION:
 * Use reflection to bypass readonly restrictions:
 * - newInstanceWithoutConstructor() - create object without calling constructor
 * - setAccessible(true) - allow setting private/readonly properties
 * - setValue() - set property value via reflection
 *
 * C# COMPARISON:
 * --------------
 * C# doesn't have this problem because:
 * 1. C# serializers (like Json.NET) have built-in support for readonly/init properties
 * 2. C# has 'init' accessors that allow setting during object initialization
 * 3. C# reflection can modify readonly fields without special handling
 *
 * PHP EQUIVALENT IN C#:
 * In C#, you'd use JsonConvert.DeserializeObject<T>(json) and it just works.
 * In PHP, we need to manually handle the reflection logic.
 *
 * TYPE INFORMATION:
 * -----------------
 * Events are stored with a $type field containing the fully-qualified class name.
 * This allows polymorphic deserialization (knowing which class to create).
 *
 * C# equivalents:
 * - C#: TypeNameHandling.Auto in Json.NET
 * - PHP: Manual $type field in serialized data
 */
class Serializer implements ISerializer
{
    /**
     * Deserialize JSON data to a PHP object
     *
     * C# TRANSLATION NOTE - KEY DIFFERENCE:
     * -------------------------------------
     * This method uses reflection to handle readonly properties, which is unique to PHP.
     *
     * DESERIALIZATION PROCESS:
     * 1. Parse JSON to array
     * 2. Extract $type field to determine target class
     * 3. Create instance WITHOUT calling constructor (newInstanceWithoutConstructor)
     * 4. Use reflection to set each property, including readonly ones
     * 5. Recursively deserialize nested objects
     *
     * READONLY PROPERTY HANDLING:
     * ---------------------------
     * The check: !$property->isInitialized($instance) || !$property->isReadOnly()
     *
     * Meaning:
     * - If property is NOT initialized → we can set it (even if readonly)
     * - OR if property is NOT readonly → we can set it
     *
     * This ensures:
     * - Readonly properties can be set during initial deserialization
     * - Already-initialized readonly properties won't be modified (would cause error)
     *
     * @param string $serializedData JSON string from event store
     * @param string $className Fully-qualified class name to deserialize to
     * @return object|null Deserialized object or null if data is null
     * @throws JsonException If JSON is invalid
     */
    public function deserialize(string $serializedData, string $className): ?object
    {
        // Parse JSON with strict error handling
        $data = json_decode($serializedData, true, 512, JSON_THROW_ON_ERROR);

        if ($data === null) {
            return null;
        }

        // Check if the data contains type information (polymorphic deserialization)
        // This allows events to be stored and retrieved without knowing their exact type upfront
        if (isset($data['$type'])) {
            $className = $data['$type'];
            unset($data['$type']);
        }

        // CRITICAL PHP REFLECTION TECHNIQUE:
        // Create instance without calling constructor
        // This is necessary because we don't have the original constructor parameters
        $reflection = new \ReflectionClass($className);
        $instance = $reflection->newInstanceWithoutConstructor();

        // Set each property using reflection
        foreach ($data as $key => $value) {
            if ($reflection->hasProperty($key)) {
                $property = $reflection->getProperty($key);

                // READONLY PROPERTY BYPASS:
                // Check if we can set this property
                // - Uninitialized properties can be set (even if readonly)
                // - Non-readonly properties can always be set
                if (!$property->isInitialized($instance) || !$property->isReadOnly()) {
                    $property->setAccessible(true); // Bypass private/protected/readonly
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
