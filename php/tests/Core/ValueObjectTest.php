<?php

declare(strict_types=1);

namespace Muflone\Tests\Core;

use InvalidArgumentException;
use Muflone\Core\ValueObject;
use PHPUnit\Framework\TestCase;

final class TestMoney extends ValueObject
{
    public function __construct(
        private readonly float $amount,
        private readonly string $currency
    ) {
    }

    public function getAmount(): float
    {
        return $this->amount;
    }

    public function getCurrency(): string
    {
        return $this->currency;
    }

    protected function getEqualityComponents(): array
    {
        return [$this->amount, $this->currency];
    }
}

final class TestAddress extends ValueObject
{
    public function __construct(
        private readonly string $street,
        private readonly string $city,
        private readonly string $zipCode
    ) {
    }

    protected function getEqualityComponents(): array
    {
        return [$this->street, $this->city, $this->zipCode];
    }
}

class ValueObjectTest extends TestCase
{
    public function testEqualsReturnsTrueForSameValues(): void
    {
        $money1 = new TestMoney(100.50, 'USD');
        $money2 = new TestMoney(100.50, 'USD');

        $this->assertTrue($money1->equals($money2));
    }

    public function testEqualsReturnsFalseForDifferentValues(): void
    {
        $money1 = new TestMoney(100.50, 'USD');
        $money2 = new TestMoney(200.00, 'USD');

        $this->assertFalse($money1->equals($money2));
    }

    public function testEqualsReturnsFalseForDifferentCurrency(): void
    {
        $money1 = new TestMoney(100.00, 'USD');
        $money2 = new TestMoney(100.00, 'EUR');

        $this->assertFalse($money1->equals($money2));
    }

    public function testEqualsReturnsFalseForNull(): void
    {
        $money = new TestMoney(100.00, 'USD');

        $this->assertFalse($money->equals(null));
    }

    public function testEqualsThrowsExceptionForDifferentTypes(): void
    {
        $money = new TestMoney(100.00, 'USD');
        $address = new TestAddress('123 Main St', 'New York', '10001');

        $this->expectException(InvalidArgumentException::class);
        $this->expectExceptionMessage('Invalid comparison of Value Objects of different types');

        $money->equals($address);
    }

    public function testHashIsConsistentForSameValues(): void
    {
        $money1 = new TestMoney(100.00, 'USD');
        $money2 = new TestMoney(100.00, 'USD');

        $this->assertEquals($money1->__hash(), $money2->__hash());
    }

    public function testComplexValueObjectEquality(): void
    {
        $address1 = new TestAddress('123 Main St', 'New York', '10001');
        $address2 = new TestAddress('123 Main St', 'New York', '10001');
        $address3 = new TestAddress('456 Oak Ave', 'New York', '10001');

        $this->assertTrue($address1->equals($address2));
        $this->assertFalse($address1->equals($address3));
    }
}
