<?php

declare(strict_types=1);

namespace Muflone\Tests\Core;

use PHPUnit\Framework\TestCase;

class DomainIdTest extends TestCase
{
    public function testCanCreateDomainId(): void
    {
        $id = TestDomainId::create('test-id-123');

        $this->assertEquals('test-id-123', $id->getValue());
    }

    public function testToStringReturnsValue(): void
    {
        $id = TestDomainId::create('test-id-456');

        $this->assertEquals('test-id-456', (string)$id);
    }

    public function testEqualsReturnsTrueForSameValue(): void
    {
        $id1 = TestDomainId::create('same-id');
        $id2 = TestDomainId::create('same-id');

        $this->assertTrue($id1->equals($id2));
    }

    public function testEqualsReturnsFalseForDifferentValue(): void
    {
        $id1 = TestDomainId::create('id-1');
        $id2 = TestDomainId::create('id-2');

        $this->assertFalse($id1->equals($id2));
    }

    public function testEqualsReturnsFalseForDifferentTypes(): void
    {
        $id1 = TestDomainId::create('same-id');
        $id2 = AnotherTestDomainId::create('same-id');

        $this->assertFalse($id1->equals($id2));
    }

    public function testEqualsReturnsFalseForNull(): void
    {
        $id = TestDomainId::create('test-id');

        $this->assertFalse($id->equals(null));
    }

    public function testHashIsConsistent(): void
    {
        $id1 = TestDomainId::create('hash-test');
        $id2 = TestDomainId::create('hash-test');

        $this->assertEquals($id1->__hash(), $id2->__hash());
    }
}
