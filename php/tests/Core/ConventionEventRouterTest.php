<?php

declare(strict_types=1);

namespace Muflone\Tests\Core;

use Muflone\Core\ConventionEventRouter;
use Muflone\Core\HandlerForDomainEventNotFoundException;
use Muflone\IAggregate;
use PHPUnit\Framework\TestCase;
use Ramsey\Uuid\Uuid;

final class RouterTestEvent
{
    public function __construct(public readonly string $data)
    {
    }
}

final class AnotherRouterTestEvent
{
    public function __construct(public readonly int $number)
    {
    }
}

final class UnhandledEvent
{
    public function __construct(public readonly string $message)
    {
    }
}

final class RouterTestAggregate implements IAggregate
{
    private string $appliedData = '';
    private int $appliedNumber = 0;
    private bool $applyWasCalled = false;

    public function __construct(
        private TestDomainId $id
    ) {
    }

    protected function applyRouterTestEvent(RouterTestEvent $event): void
    {
        $this->appliedData = $event->data;
        $this->applyWasCalled = true;
    }

    protected function applyAnotherRouterTestEvent(AnotherRouterTestEvent $event): void
    {
        $this->appliedNumber = $event->number;
        $this->applyWasCalled = true;
    }

    public function getAppliedData(): string
    {
        return $this->appliedData;
    }

    public function getAppliedNumber(): int
    {
        return $this->appliedNumber;
    }

    public function wasApplyCalled(): bool
    {
        return $this->applyWasCalled;
    }

    public function getId(): \Muflone\Core\IDomainId
    {
        return $this->id;
    }

    public function getVersion(): int
    {
        return 0;
    }

    public function applyEvent(object $event): void
    {
    }

    public function getUncommittedEvents(): array
    {
        return [];
    }

    public function clearUncommittedEvents(): void
    {
    }

    public function getSnapshot(): ?\Muflone\IMemento
    {
        return null;
    }
}

class ConventionEventRouterTest extends TestCase
{
    public function testCanDispatchEventToAggregate(): void
    {
        $id = TestDomainId::create(Uuid::uuid4()->toString());
        $aggregate = new RouterTestAggregate($id);
        $router = new ConventionEventRouter(true, $aggregate);

        $event = new RouterTestEvent('test data');
        $router->dispatch($event);

        $this->assertTrue($aggregate->wasApplyCalled());
        $this->assertEquals('test data', $aggregate->getAppliedData());
    }

    public function testCanDispatchMultipleEventTypes(): void
    {
        $id = TestDomainId::create(Uuid::uuid4()->toString());
        $aggregate = new RouterTestAggregate($id);
        $router = new ConventionEventRouter(true, $aggregate);

        $event1 = new RouterTestEvent('hello');
        $event2 = new AnotherRouterTestEvent(42);

        $router->dispatch($event1);
        $router->dispatch($event2);

        $this->assertEquals('hello', $aggregate->getAppliedData());
        $this->assertEquals(42, $aggregate->getAppliedNumber());
    }

    public function testThrowsExceptionWhenHandlerNotFoundAndThrowOnApplyNotFoundIsTrue(): void
    {
        $id = TestDomainId::create(Uuid::uuid4()->toString());
        $aggregate = new RouterTestAggregate($id);
        $router = new ConventionEventRouter(true, $aggregate);

        $event = new UnhandledEvent('unhandled');

        $this->expectException(HandlerForDomainEventNotFoundException::class);
        $this->expectExceptionMessageMatches('/RouterTestAggregate.*UnhandledEvent/');

        $router->dispatch($event);
    }

    public function testDoesNotThrowWhenHandlerNotFoundAndThrowOnApplyNotFoundIsFalse(): void
    {
        $id = TestDomainId::create(Uuid::uuid4()->toString());
        $aggregate = new RouterTestAggregate($id);
        $router = new ConventionEventRouter(false, $aggregate);

        $event = new UnhandledEvent('unhandled');

        // Should not throw
        $router->dispatch($event);

        $this->assertTrue(true); // If we get here, the test passed
    }

    public function testCanRegisterCustomHandler(): void
    {
        $router = new ConventionEventRouter(false);
        $handledData = '';

        $router->register(function (RouterTestEvent $event) use (&$handledData) {
            $handledData = $event->data;
        });

        $event = new RouterTestEvent('custom handler');
        $router->dispatch($event);

        $this->assertEquals('custom handler', $handledData);
    }
}
