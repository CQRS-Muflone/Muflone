<?php

declare(strict_types=1);

namespace Muflone\Tests\Core;

use Muflone\Core\Entity;
use Muflone\Core\IDomainId;
use PHPUnit\Framework\TestCase;

final class TestEntity extends Entity
{
    public function __construct(IDomainId $id)
    {
        parent::__construct($id);
    }
}

final class AnotherTestEntity extends Entity
{
    public function __construct(IDomainId $id)
    {
        parent::__construct($id);
    }
}

class EntityTest extends TestCase
{
    public function testCanCreateEntity(): void
    {
        $id = TestDomainId::create('entity-1');
        $entity = new TestEntity($id);

        $this->assertEquals('entity-1', $entity->getId()->getValue());
    }

    public function testEqualsReturnsTrueForSameId(): void
    {
        $id = TestDomainId::create('same-id');
        $entity1 = new TestEntity($id);
        $entity2 = new TestEntity($id);

        $this->assertTrue($entity1->equals($entity2));
    }

    public function testEqualsReturnsFalseForDifferentId(): void
    {
        $id1 = TestDomainId::create('id-1');
        $id2 = TestDomainId::create('id-2');
        $entity1 = new TestEntity($id1);
        $entity2 = new TestEntity($id2);

        $this->assertFalse($entity1->equals($entity2));
    }

    public function testEqualsReturnsFalseForDifferentEntityTypes(): void
    {
        $id = TestDomainId::create('same-id');
        $entity1 = new TestEntity($id);
        $entity2 = new AnotherTestEntity($id);

        $this->assertFalse($entity1->equals($entity2));
    }

    public function testEqualsReturnsFalseForNull(): void
    {
        $id = TestDomainId::create('test-id');
        $entity = new TestEntity($id);

        $this->assertFalse($entity->equals(null));
    }

    public function testHashIsBasedOnId(): void
    {
        $id = TestDomainId::create('hash-test');
        $entity1 = new TestEntity($id);
        $entity2 = new TestEntity($id);

        $this->assertEquals($entity1->__hash(), $entity2->__hash());
    }
}
