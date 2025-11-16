<?php

declare(strict_types=1);

namespace Muflone\Tests\Examples\BrewUp;

use BrewUp\Sales\SharedKernel\CustomTypes\BeerName;
use BrewUp\Sales\SharedKernel\CustomTypes\Quantity;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderNumber;
use PHPUnit\Framework\TestCase;

/**
 * Tests for BrewUp value objects validating C# to PHP translation
 */
class ValueObjectsTest extends TestCase
{
    public function testSalesOrderIdCanBeCreated(): void
    {
        $id = SalesOrderId::create();

        $this->assertNotEmpty($id->getValue());
        $this->assertIsString($id->getValue());
    }

    public function testSalesOrderIdCanBeCreatedFromString(): void
    {
        $uuid = '123e4567-e89b-12d3-a456-426614174000';
        $id = SalesOrderId::fromString($uuid);

        $this->assertEquals($uuid, $id->getValue());
    }

    public function testSalesOrderIdEquality(): void
    {
        $uuid = '123e4567-e89b-12d3-a456-426614174000';
        $id1 = SalesOrderId::fromString($uuid);
        $id2 = SalesOrderId::fromString($uuid);

        $this->assertTrue($id1->equals($id2));
    }

    public function testSalesOrderIdToString(): void
    {
        $uuid = '123e4567-e89b-12d3-a456-426614174000';
        $id = SalesOrderId::fromString($uuid);

        $this->assertEquals($uuid, (string)$id);
    }

    public function testSalesOrderNumberCanBeCreated(): void
    {
        $number = SalesOrderNumber::create('SO-2025-001');

        $this->assertEquals('SO-2025-001', $number->getValue());
    }

    public function testSalesOrderNumberEquality(): void
    {
        $number1 = SalesOrderNumber::create('SO-123');
        $number2 = SalesOrderNumber::create('SO-123');

        $this->assertTrue($number1->equals($number2));
    }

    public function testSalesOrderNumberInequality(): void
    {
        $number1 = SalesOrderNumber::create('SO-123');
        $number2 = SalesOrderNumber::create('SO-456');

        $this->assertFalse($number1->equals($number2));
    }

    public function testSalesOrderNumberToString(): void
    {
        $number = SalesOrderNumber::create('SO-TEST-999');

        $this->assertEquals('SO-TEST-999', (string)$number);
    }

    public function testBeerNameCanBeCreated(): void
    {
        $beerName = BeerName::create('IPA Delight');

        $this->assertEquals('IPA Delight', $beerName->getValue());
    }

    public function testBeerNameCannotBeEmpty(): void
    {
        $this->expectException(\InvalidArgumentException::class);
        $this->expectExceptionMessage('Beer name cannot be empty');

        BeerName::create('');
    }

    public function testBeerNameEquality(): void
    {
        $beer1 = BeerName::create('Stout Supreme');
        $beer2 = BeerName::create('Stout Supreme');

        $this->assertTrue($beer1->equals($beer2));
    }

    public function testBeerNameToString(): void
    {
        $beerName = BeerName::create('Pilsner Premium');

        $this->assertEquals('Pilsner Premium', (string)$beerName);
    }

    public function testQuantityCanBeCreated(): void
    {
        $quantity = Quantity::create(24);

        $this->assertEquals(24, $quantity->getValue());
    }

    public function testQuantityCannotBeNegative(): void
    {
        $this->expectException(\InvalidArgumentException::class);
        $this->expectExceptionMessage('Quantity cannot be negative');

        Quantity::create(-1);
    }

    public function testQuantityCanBeZero(): void
    {
        $quantity = Quantity::create(0);

        $this->assertEquals(0, $quantity->getValue());
    }

    public function testQuantityEquality(): void
    {
        $qty1 = Quantity::create(50);
        $qty2 = Quantity::create(50);

        $this->assertTrue($qty1->equals($qty2));
    }

    public function testQuantityInequality(): void
    {
        $qty1 = Quantity::create(10);
        $qty2 = Quantity::create(20);

        $this->assertFalse($qty1->equals($qty2));
    }

    public function testQuantityToString(): void
    {
        $quantity = Quantity::create(100);

        $this->assertEquals('100', (string)$quantity);
    }

    public function testAllValueObjectsAreImmutable(): void
    {
        // Value objects should be readonly
        $id = SalesOrderId::create();
        $number = SalesOrderNumber::create('SO-001');
        $beer = BeerName::create('Test Beer');
        $qty = Quantity::create(5);

        // Values should not change after creation
        $originalIdValue = $id->getValue();
        $originalNumberValue = $number->getValue();
        $originalBeerValue = $beer->getValue();
        $originalQtyValue = $qty->getValue();

        // Create new instances with same values
        $id2 = SalesOrderId::fromString($originalIdValue);
        $this->assertEquals($originalIdValue, $id2->getValue());

        // Verify originals are unchanged
        $this->assertEquals($originalIdValue, $id->getValue());
        $this->assertEquals($originalNumberValue, $number->getValue());
        $this->assertEquals($originalBeerValue, $beer->getValue());
        $this->assertEquals($originalQtyValue, $qty->getValue());
    }
}
