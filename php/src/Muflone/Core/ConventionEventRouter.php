<?php

declare(strict_types=1);

namespace Muflone\Core;

use InvalidArgumentException;
use Muflone\IAggregate;
use Muflone\IRouteEvents;
use ReflectionClass;
use ReflectionMethod;

/**
 * Convention-based Event Router
 *
 * C# TO PHP TRANSLATION NOTES:
 * ============================
 *
 * This class implements convention-based event routing, a key pattern in the Muflone framework.
 * The C# version uses method overloading, which PHP does not support. This required a different approach.
 *
 * C# PATTERN (Method Overloading):
 * --------------------------------
 * In C#, aggregates define multiple Apply methods with the same name but different parameter types:
 *
 *   protected void Apply(SalesOrderCreated event) { ... }
 *   protected void Apply(SalesOrderPrepared event) { ... }
 *   protected void Apply(SalesOrderClosed event) { ... }
 *
 * The router uses reflection to find the correct Apply method based on the event type at runtime.
 *
 * PHP PATTERN (Named Methods):
 * ----------------------------
 * Since PHP doesn't support method overloading, we use specifically-named methods instead:
 *
 *   protected function applySalesOrderCreated(SalesOrderCreated $event): void { ... }
 *   protected function applySalesOrderPrepared(SalesOrderPrepared $event): void { ... }
 *   protected function applySalesOrderClosed(SalesOrderClosed $event): void { ... }
 *
 * The router discovers methods that:
 * 1. Start with 'apply' (e.g., applySalesOrderCreated)
 * 2. Accept exactly one parameter
 * 3. Have a typed parameter (not a built-in type)
 *
 * ADVANTAGES OF THE PHP APPROACH:
 * --------------------------------
 * 1. More explicit: Method names clearly indicate which event they handle
 * 2. Better IDE support: Auto-completion shows all event handlers
 * 3. Easier debugging: Stack traces show the specific handler method name
 * 4. No runtime type resolution needed
 *
 * ALTERNATIVE PATTERNS CONSIDERED:
 * --------------------------------
 * 1. Single apply(object $event) with instanceof checks - rejected for poor maintainability
 * 2. Manual registration in constructor - rejected for violating convention-over-configuration
 * 3. Attributes/Annotations - rejected to maintain PHP 8.0+ compatibility without external dependencies
 */
final class ConventionEventRouter implements IRouteEvents
{
    /**
     * Map of event type names to handler callables
     * @var array<string, callable>
     */
    private array $handlers = [];

    /**
     * The aggregate that this router is registered to (for error reporting)
     */
    private ?IAggregate $registered = null;

    /**
     * Constructor
     *
     * @param bool $throwOnApplyNotFound If true, throws exception when no handler found
     *                                   C# uses this for stricter event sourcing validation
     * @param IAggregate|null $aggregate Optional aggregate to register immediately
     */
    public function __construct(
        private readonly bool $throwOnApplyNotFound = true,
        ?IAggregate $aggregate = null
    ) {
        if ($aggregate !== null) {
            $this->registerAggregate($aggregate);
        }
    }

    /**
     * Manually register a handler callable for a specific event type
     *
     * C# TRANSLATION NOTE:
     * The C# version uses Action<T> delegates. In PHP, we use callables with type hints.
     * PHP's reflection API extracts the type from the callable's parameter type hint.
     *
     * @param callable $handler Function that accepts one typed parameter
     * @throws InvalidArgumentException If handler signature is invalid
     */
    public function register(callable $handler): void
    {
        // Extract the type from the callable signature using reflection
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

    /**
     * Register an aggregate and discover its event handler methods by convention
     *
     * C# TRANSLATION NOTE - CRITICAL DIFFERENCE:
     * ------------------------------------------
     * This is where the major C# to PHP translation challenge occurs.
     *
     * C# VERSION:
     * The C# router looks for methods named "Apply" with different parameter types:
     *   - Uses method overloading (same name, different signatures)
     *   - Runtime type resolution based on event type
     *
     * PHP VERSION:
     * Since PHP doesn't support method overloading, we use a naming convention:
     *   - Methods must start with 'apply' (e.g., applySalesOrderCreated)
     *   - Method name doesn't have to match event name exactly
     *   - The parameter type hint determines which events the method handles
     *
     * DISCOVERY ALGORITHM:
     * 1. Use reflection to get all public/protected methods
     * 2. Filter methods that start with 'apply' and have exactly one parameter
     * 3. Extract the parameter's type hint (must be a class, not a built-in type)
     * 4. Register a closure that invokes the method when that event type is dispatched
     *
     * @param IAggregate $aggregate The aggregate whose methods to scan
     */
    public function registerAggregate(IAggregate $aggregate): void
    {
        $this->registered = $aggregate;

        $reflection = new ReflectionClass($aggregate);
        // Get both public and protected methods (C# allows protected Apply methods)
        $methods = $reflection->getMethods(ReflectionMethod::IS_PUBLIC | ReflectionMethod::IS_PROTECTED);

        foreach ($methods as $method) {
            $methodName = $method->getName();

            // Look for methods named 'apply' or starting with 'apply' (e.g., applySalesOrderCreated)
            if (($methodName === 'apply' || str_starts_with($methodName, 'apply'))
                && $method->getNumberOfParameters() === 1) {
                $parameters = $method->getParameters();
                $type = $parameters[0]->getType();

                // Only process methods with class type hints (not string, int, etc.)
                if ($type !== null && !$type->isBuiltin()) {
                    $typeName = $type->getName();

                    // Skip generic 'apply' method - we only use specific applyXXX methods
                    // This prevents ambiguity and makes the PHP pattern clearer
                    if ($methodName === 'apply') {
                        continue;
                    }

                    // Register a closure that makes the method accessible and invokes it
                    // PHP requires setAccessible(true) to call protected methods via reflection
                    $this->handlers[$typeName] = function (object $event) use ($aggregate, $method) {
                        $method->setAccessible(true);
                        $method->invoke($aggregate, $event);
                    };
                }
            }
        }
    }

    /**
     * Dispatch an event to its registered handler
     *
     * C# TRANSLATION NOTE:
     * Both C# and PHP versions use the event's runtime type to find the handler.
     * The difference is in how handlers are registered (see registerAggregate above).
     *
     * ERROR HANDLING:
     * - If throwOnApplyNotFound is true (default), throws exception for missing handlers
     * - This enforces strict event sourcing: all events MUST be handled
     * - In C#, this prevents silent failures during aggregate reconstitution
     * - The same pattern is preserved in PHP for consistency
     *
     * @param object $eventMessage The event to dispatch
     * @throws HandlerForDomainEventNotFoundException If no handler found and throwOnApplyNotFound is true
     */
    public function dispatch(object $eventMessage): void
    {
        // Get the fully-qualified class name of the event
        $eventType = get_class($eventMessage);

        if (isset($this->handlers[$eventType])) {
            // Invoke the registered handler for this event type
            $this->handlers[$eventType]($eventMessage);
        } elseif ($this->throwOnApplyNotFound && $this->registered !== null) {
            // Throw detailed exception to help diagnose missing event handlers
            // This is crucial during development to catch missing applyXXX methods
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
