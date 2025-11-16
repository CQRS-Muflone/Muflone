<?php

declare(strict_types=1);

namespace Muflone\Core;

use InvalidArgumentException;
use Muflone\IAggregate;
use Muflone\IRouteEvents;
use ReflectionClass;
use ReflectionMethod;

final class ConventionEventRouter implements IRouteEvents
{
    /** @var array<string, callable> */
    private array $handlers = [];

    private ?IAggregate $registered = null;

    public function __construct(
        private readonly bool $throwOnApplyNotFound = true,
        ?IAggregate $aggregate = null
    ) {
        if ($aggregate !== null) {
            $this->registerAggregate($aggregate);
        }
    }

    public function register(callable $handler): void
    {
        // Extract the type from the callable signature
        $reflection = new \ReflectionFunction($handler(...));
        $parameters = $reflection->getParameters();

        if (count($parameters) !== 1) {
            throw new InvalidArgumentException('Handler must accept exactly one parameter');
        }

        $type = $parameters[0]->getType();
        if ($type === null) {
            throw new InvalidArgumentException('Handler parameter must be typed');
        }

        $typeName = $type->getName();
        $this->handlers[$typeName] = $handler;
    }

    public function registerAggregate(IAggregate $aggregate): void
    {
        $this->registered = $aggregate;

        $reflection = new ReflectionClass($aggregate);
        $methods = $reflection->getMethods(ReflectionMethod::IS_PUBLIC | ReflectionMethod::IS_PROTECTED);

        foreach ($methods as $method) {
            $methodName = $method->getName();

            // Look for methods named 'apply' or 'applyEventClassName'
            if (($methodName === 'apply' || str_starts_with($methodName, 'apply'))
                && $method->getNumberOfParameters() === 1) {
                $parameters = $method->getParameters();
                $type = $parameters[0]->getType();

                if ($type !== null && !$type->isBuiltin()) {
                    $typeName = $type->getName();

                    // For apply methods, determine the event type from the parameter
                    if ($methodName === 'apply') {
                        // Skip the generic dispatcher - we'll use specific applyXXX methods
                        continue;
                    }

                    $this->handlers[$typeName] = function (object $event) use ($aggregate, $method) {
                        $method->setAccessible(true);
                        $method->invoke($aggregate, $event);
                    };
                }
            }
        }
    }

    public function dispatch(object $eventMessage): void
    {
        $eventType = get_class($eventMessage);

        if (isset($this->handlers[$eventType])) {
            $this->handlers[$eventType]($eventMessage);
        } elseif ($this->throwOnApplyNotFound && $this->registered !== null) {
            $aggregateType = get_class($this->registered);
            throw new HandlerForDomainEventNotFoundException(
                sprintf(
                    "Aggregate of type '%s' raised an event of type '%s' but no handler could be found to handle the message.",
                    $aggregateType,
                    $eventType
                )
            );
        }
    }
}
