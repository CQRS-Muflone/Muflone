<?php

declare(strict_types=1);

namespace Muflone\Core;

use InvalidArgumentException;

/**
 * Value Object Base Class
 *
 * C# TO PHP TRANSLATION NOTES:
 * ============================
 *
 * WHAT IS A VALUE OBJECT?
 * -----------------------
 * A Value Object is a DDD pattern representing an immutable object defined entirely by its values,
 * not by identity. Two Value Objects are equal if all their properties are equal.
 *
 * EXAMPLES:
 * - Money: $10 USD == $10 USD (same value, interchangeable)
 * - Email Address: "user@example.com" == "user@example.com"
 * - Temperature: 23°C == 23°C
 * - BeerName: "IPA" == "IPA"
 *
 * CONTRAST WITH ENTITIES:
 * - Entity: Two people with same name are NOT the same person (identity matters)
 * - Value Object: Two $10 bills have the same value (identity doesn't matter)
 *
 * VALUE OBJECT CHARACTERISTICS:
 * ------------------------------
 * 1. **Immutable**: Once created, values never change
 * 2. **Value Equality**: Two objects are equal if their values are equal
 * 3. **No Identity**: No ID property; equality is based on all properties
 * 4. **Interchangeable**: Can swap one for another with same value
 *
 * C# VS PHP IMPLEMENTATION:
 * -------------------------
 *
 * C# APPROACH (using records):
 * ```csharp
 * public record BeerName(string Value) : ValueObject
 * {
 *     protected override IEnumerable<object> GetEqualityComponents()
 *     {
 *         yield return Value;
 *     }
 * }
 * ```
 *
 * PHP APPROACH (using readonly class):
 * ```php
 * final readonly class BeerName extends ValueObject
 * {
 *     public function __construct(
 *         private readonly string $value
 *     ) {}
 *
 *     protected function getEqualityComponents(): array
 *     {
 *         return [$this->value];
 *     }
 * }
 * ```
 *
 * KEY DIFFERENCES:
 * ----------------
 * 1. **Immutability**:
 *    - C#: Use 'record' or readonly properties with init accessors
 *    - PHP: Use 'readonly' modifier on class or properties (PHP 8.1+)
 *
 * 2. **Equality Components**:
 *    - C#: Uses 'yield return' to lazily generate components (IEnumerable)
 *    - PHP: Returns array directly (no lazy evaluation needed)
 *
 * 3. **Hash Code**:
 *    - C#: Uses GetHashCode() override with prime number multiplication
 *    - PHP: Uses __hash() magic method with crc32 for performance
 *
 * TEMPLATE METHOD PATTERN:
 * ------------------------
 * This class uses the Template Method pattern:
 * - Base class defines the algorithm (equals, __hash)
 * - Child classes implement specific steps (getEqualityComponents)
 *
 * USAGE PATTERN:
 * --------------
 * When creating a Value Object:
 * 1. Extend ValueObject
 * 2. Make it 'final' to prevent inheritance
 * 3. Use 'readonly' for immutability
 * 4. Implement getEqualityComponents() returning all property values
 * 5. Validate in constructor if needed
 *
 * EXAMPLE - Complete Value Object:
 * ```php
 * final readonly class Money extends ValueObject
 * {
 *     public function __construct(
 *         private readonly float $amount,
 *         private readonly string $currency
 *     ) {
 *         if ($amount < 0) {
 *             throw new \InvalidArgumentException('Amount cannot be negative');
 *         }
 *     }
 *
 *     protected function getEqualityComponents(): array
 *     {
 *         return [$this->amount, $this->currency];
 *     }
 *
 *     public function getAmount(): float { return $this->amount; }
 *     public function getCurrency(): string { return $this->currency; }
 * }
 *
 * // Usage:
 * $price1 = new Money(10.00, 'USD');
 * $price2 = new Money(10.00, 'USD');
 * $price1->equals($price2); // true - same value
 * ```
 *
 * REFERENCE:
 * Based on: https://enterprisecraftsmanship.com/2017/08/28/value-object-a-better-implementation/
 */
abstract class ValueObject
{
    /**
     * Returns the list of values to compare for equality
     *
     * IMPLEMENTATION GUIDE:
     * ---------------------
     * Return an array containing all properties that define this value object.
     *
     * SIMPLE CASE (single property):
     * ```php
     * protected function getEqualityComponents(): array {
     *     return [$this->value];
     * }
     * ```
     *
     * MULTIPLE PROPERTIES:
     * ```php
     * protected function getEqualityComponents(): array {
     *     return [$this->amount, $this->currency];
     * }
     * ```
     *
     * COLLECTIONS/ARRAYS:
     * ```php
     * protected function getEqualityComponents(): array {
     *     $components = [];
     *     foreach ($this->items as $item) {
     *         $components[] = $item;
     *     }
     *     return $components;
     * }
     * ```
     *
     * C# TRANSLATION NOTE:
     * In C#, this would use 'yield return' for lazy evaluation.
     * PHP doesn't have yield in this context, so we return a complete array.
     *
     * @return array<mixed> Array of values that define equality
     */
    abstract protected function getEqualityComponents(): array;

    /**
     * Check equality with another Value Object
     *
     * VALUE EQUALITY SEMANTICS:
     * -------------------------
     * Two Value Objects are equal if:
     * 1. They are the same class (not just parent/child)
     * 2. All their equality components are equal
     *
     * EXAMPLE:
     * ```php
     * $beer1 = new BeerName('IPA');
     * $beer2 = new BeerName('IPA');
     * $beer3 = new BeerName('Stout');
     *
     * $beer1->equals($beer2); // true - same value
     * $beer1->equals($beer3); // false - different value
     * $beer1->equals(null);   // false - null is not equal
     * ```
     *
     * C# TRANSLATION NOTE:
     * - C# would override Equals(object obj) and implement IEquatable<T>
     * - PHP uses explicit equals() method (no operator overloading)
     * - Both use === for strict component comparison
     *
     * TYPE SAFETY:
     * Throws exception if comparing different Value Object types.
     * This catches developer errors at runtime:
     * ```php
     * $money = new Money(10, 'USD');
     * $beer = new BeerName('IPA');
     * $money->equals($beer); // ❌ Throws InvalidArgumentException
     * ```
     *
     * @param ValueObject|null $other The other Value Object to compare
     * @return bool True if equal, false otherwise
     * @throws InvalidArgumentException If comparing different types
     */
    public function equals(?self $other): bool
    {
        // Null is never equal to a value
        if ($other === null) {
            return false;
        }

        // Different classes are not equal, even if same parent
        // This ensures Money doesn't equal Temperature
        if (get_class($this) !== get_class($other)) {
            throw new InvalidArgumentException(
                sprintf(
                    'Invalid comparison of Value Objects of different types: %s and %s',
                    get_class($this),
                    get_class($other)
                )
            );
        }

        // Compare all equality components
        // PHP's === does deep array comparison
        return $this->getEqualityComponents() === $other->getEqualityComponents();
    }

    /**
     * Generate hash code for this Value Object
     *
     * HASH CODE PURPOSE:
     * ------------------
     * Used for:
     * 1. Hash table lookups (if Value Objects are used as array keys)
     * 2. Quick inequality checks (different hashes = definitely not equal)
     * 3. Set/Dictionary membership tests
     *
     * ALGORITHM:
     * ----------
     * Uses prime number multiplication (23) to combine component hashes.
     * This is a common hash code algorithm that provides good distribution.
     *
     * Formula: hash = ((hash * 23) + component1_hash) * 23 + component2_hash...
     *
     * C# TRANSLATION NOTE:
     * --------------------
     * C#: Uses GetHashCode() override
     * - Return type: int (32-bit integer)
     * - Uses hash('sha256', ...) or HashCode.Combine() in modern C#
     *
     * PHP: Uses __hash() magic method
     * - Return type: string (for flexibility)
     * - Uses crc32() for performance (returns 32-bit integer)
     *
     * WHY crc32 INSTEAD OF sha256?
     * - Hash codes should be fast to compute
     * - crc32 is much faster than cryptographic hashes
     * - 32-bit is sufficient for hash table distribution
     * - Not used for security, so cryptographic strength not needed
     *
     * @return string String representation of hash code
     */
    public function __hash(): string
    {
        $hash = 1; // Start with 1 (not 0) to handle empty components

        foreach ($this->getEqualityComponents() as $component) {
            if ($component !== null) {
                // Serialize component to string, then hash it
                // This handles objects, arrays, and primitives uniformly
                $componentHash = crc32(serialize($component));

                // Combine using prime number multiplication (23 is conventional)
                // This provides good hash distribution
                $hash = $hash * 23 + $componentHash;
            }
        }

        return (string) $hash;
    }
}
