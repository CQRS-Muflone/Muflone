<?php

declare(strict_types=1);

namespace Muflone\Core;

/**
 * Strongly-Typed Domain Identifier
 *
 * C# TO PHP TRANSLATION NOTES:
 * ============================
 *
 * WHAT IS A DOMAIN ID?
 * --------------------
 * A Domain ID is a strongly-typed identifier for domain entities and aggregates.
 * Instead of using raw strings or integers, we wrap IDs in type-safe classes.
 *
 * WHY STRONG TYPING?
 * ------------------
 * Prevents bugs from mixing up different types of IDs:
 *
 * ❌ WEAK TYPING (error-prone):
 * ```php
 * function getSalesOrder(string $id) { ... }
 * function getProduct(string $id) { ... }
 *
 * $salesOrderId = "order-123";
 * $productId = "product-456";
 *
 * getSalesOrder($productId); // ❌ Compiles fine, but wrong at runtime!
 * ```
 *
 * ✅ STRONG TYPING (compile-time safety):
 * ```php
 * function getSalesOrder(SalesOrderId $id) { ... }
 * function getProduct(ProductId $id) { ... }
 *
 * $salesOrderId = new SalesOrderId("order-123");
 * $productId = new ProductId("product-456");
 *
 * getSalesOrder($productId); // ✅ TYPE ERROR - caught by IDE and runtime!
 * ```
 *
 * PATTERN: TYPE-SAFE WRAPPER
 * ---------------------------
 * Each aggregate gets its own ID type:
 * - SalesOrder → SalesOrderId
 * - Product → ProductId
 * - Customer → CustomerId
 *
 * Even though they all wrap strings, the type system prevents mixing them up.
 *
 * C# VS PHP IMPLEMENTATION:
 * -------------------------
 *
 * C# APPROACH (modern - using records):
 * ```csharp
 * public record SalesOrderId(string Value) : DomainId;
 *
 * // Usage:
 * var id = new SalesOrderId(Guid.NewGuid().ToString());
 * ```
 *
 * C# APPROACH (traditional - using classes):
 * ```csharp
 * public class SalesOrderId : DomainId
 * {
 *     public SalesOrderId(string value) : base(value) { }
 *
 *     public static SalesOrderId NewId() =>
 *         new SalesOrderId(Guid.NewGuid().ToString());
 * }
 * ```
 *
 * PHP APPROACH (using readonly class):
 * ```php
 * final readonly class SalesOrderId extends DomainId
 * {
 *     public static function create(): self
 *     {
 *         return new self(\Ramsey\Uuid\Uuid::uuid4()->toString());
 *     }
 *
 *     public static function fromString(string $value): self
 *     {
 *         return new self($value);
 *     }
 * }
 * ```
 *
 * KEY DIFFERENCES:
 * ----------------
 * 1. **Immutability**:
 *    - C#: Use 'record' or class with readonly field
 *    - PHP: Use 'readonly' modifier (PHP 8.1+)
 *
 * 2. **GUID/UUID Generation**:
 *    - C#: Guid.NewGuid()
 *    - PHP: \Ramsey\Uuid\Uuid::uuid4() (external package)
 *
 * 3. **Constructor Property Promotion**:
 *    - Both languages support this in modern versions
 *    - C#: Primary constructor (C# 12+) or regular constructor
 *    - PHP: Constructor property promotion (PHP 8.0+)
 *
 * 4. **Equality**:
 *    - C#: Records have built-in value equality
 *    - PHP: Must implement equals() method explicitly
 *
 * USAGE EXAMPLES:
 * ---------------
 *
 * Creating IDs:
 * ```php
 * // From UUID
 * $id1 = SalesOrderId::create();
 *
 * // From existing string
 * $id2 = SalesOrderId::fromString('123e4567-e89b-12d3-a456-426614174000');
 *
 * // Comparison
 * $id1->equals($id2); // false - different UUIDs
 * ```
 *
 * In Event Sourcing:
 * ```php
 * class SalesOrderCreated extends DomainEvent
 * {
 *     public function __construct(
 *         public readonly SalesOrderId $aggregateId,  // ← Strongly-typed ID
 *         // ...other properties
 *     ) {}
 * }
 * ```
 *
 * BEST PRACTICES:
 * ---------------
 * 1. Make ID classes 'final' to prevent inheritance
 * 2. Make them 'readonly' for immutability
 * 3. Provide factory methods (create(), fromString())
 * 4. Use UUIDs for globally unique identifiers
 * 5. Override __toString() for easy logging/debugging
 */
abstract class DomainId implements IDomainId
{
    /**
     * Constructor with Property Promotion
     *
     * C# TRANSLATION NOTE:
     * Both C# and PHP now support constructor property promotion.
     *
     * C# (modern):
     * ```csharp
     * public record DomainId(string Value);
     * ```
     *
     * PHP 8.0+:
     * ```php
     * public function __construct(
     *     private readonly string $value
     * ) {}
     * ```
     *
     * Both create a private readonly field initialized from constructor parameter.
     *
     * @param string $value The ID value (typically a UUID string)
     */
    public function __construct(
        private readonly string $value
    ) {
    }

    /**
     * Get the underlying ID value
     *
     * C# TRANSLATION NOTE:
     * In C#, this would be a property: public string Value { get; }
     * In PHP, we use a method: getValue()
     *
     * @return string The ID value
     */
    public function getValue(): string
    {
        return $this->value;
    }

    /**
     * Check equality with another Domain ID
     *
     * EQUALITY SEMANTICS:
     * -------------------
     * Two Domain IDs are equal if:
     * 1. They are the exact same class (not parent/child)
     * 2. Their values are identical
     *
     * TYPE SAFETY:
     * SalesOrderId can only equal another SalesOrderId.
     * SalesOrderId != ProductId even if values match.
     *
     * EXAMPLE:
     * ```php
     * $order1 = new SalesOrderId('abc-123');
     * $order2 = new SalesOrderId('abc-123');
     * $order3 = new SalesOrderId('xyz-789');
     * $product = new ProductId('abc-123');
     *
     * $order1->equals($order2);  // true - same type, same value
     * $order1->equals($order3);  // false - same type, different value
     * $order1->equals($product); // false - different types (even though same value)
     * $order1->equals(null);     // false - null is not equal
     * ```
     *
     * C# TRANSLATION NOTE:
     * - C# records have built-in value equality
     * - PHP requires explicit equals() implementation
     * - Both prevent mixing different ID types
     *
     * @param DomainId|null $other The other Domain ID to compare
     * @return bool True if equal, false otherwise
     */
    public function equals(?self $other): bool
    {
        if ($other === null) {
            return false;
        }

        // Must be exact same class AND same value
        return get_class($this) === get_class($other) && $other->value === $this->value;
    }

    /**
     * Convert to string representation
     *
     * Allows easy logging and debugging:
     * ```php
     * $id = SalesOrderId::create();
     * echo "Processing order: " . $id; // Automatically calls __toString()
     * $logger->info("Order ID: {$id}");
     * ```
     *
     * C# TRANSLATION NOTE:
     * C#: Override ToString()
     * PHP: Implement __toString() magic method
     *
     * Both allow implicit string conversion.
     *
     * @return string String representation of the ID
     */
    public function __toString(): string
    {
        return $this->value;
    }

    /**
     * Generate hash code for this Domain ID
     *
     * Used for hash table lookups and equality checks.
     *
     * C# TRANSLATION NOTE:
     * C#: Override GetHashCode()
     * - Returns int (32-bit)
     * - Records auto-generate this
     *
     * PHP: Implement __hash()
     * - Returns string (for flexibility)
     * - Must implement explicitly
     *
     * ALGORITHM:
     * Uses SHA-256 hash of the value string for strong distribution.
     *
     * @return string Hash code as hexadecimal string
     */
    public function __hash(): string
    {
        return hash('sha256', $this->value);
    }
}
