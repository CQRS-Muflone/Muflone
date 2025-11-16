<?php

declare(strict_types=1);

/**
 * BrewUp Sales - Complete CQRS/ES Example
 *
 * This example demonstrates a complete CQRS and Event Sourcing workflow
 * using the Muflone PHP library with the BrewUp Sales domain.
 */

require_once __DIR__ . '/../../vendor/autoload.php';

use BrewUp\Sales\Domain\SalesOrder;
use BrewUp\Sales\SharedKernel\Commands\CreateSalesOrder;
use BrewUp\Sales\SharedKernel\Commands\PrepareSalesOrder;
use BrewUp\Sales\SharedKernel\Commands\CloseSalesOrder;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderId;
use BrewUp\Sales\SharedKernel\CustomTypes\SalesOrderNumber;
use BrewUp\Sales\SharedKernel\CustomTypes\BeerName;
use BrewUp\Sales\SharedKernel\CustomTypes\Quantity;
use Muflone\CustomTypes\Account;

echo "╔══════════════════════════════════════════════════════════╗\n";
echo "║  BrewUp Sales - CQRS/Event Sourcing Example             ║\n";
echo "║  Using Muflone PHP Library                               ║\n";
echo "╚══════════════════════════════════════════════════════════╝\n\n";

// ============================================================================
// Step 1: Create a Sales Order
// ============================================================================

echo "📝 Step 1: Creating a new Sales Order\n";
echo str_repeat("-", 60) . "\n";

$salesOrderId = SalesOrderId::create();
$who = new Account('user-123', 'John Brewer');

echo "  Order ID: {$salesOrderId->getValue()}\n";
echo "  Created by: {$who->name}\n\n";

// Create the aggregate using the factory method
$salesOrder = SalesOrder::create(
    $salesOrderId,
    SalesOrderNumber::create('SO-2025-001'),
    BeerName::create('IPA Delight'),
    Quantity::create(24),
    $who
);

echo "✓ Sales Order Created!\n";
echo "  Order Number: {$salesOrder->getSalesOrderNumber()}\n";
echo "  Beer: {$salesOrder->getBeerName()}\n";
echo "  Quantity: {$salesOrder->getQuantity()}\n";
echo "  Status: {$salesOrder->getStatus()}\n";
echo "  Version: {$salesOrder->getVersion()}\n";

// Show uncommitted events
$events = $salesOrder->getUncommittedEvents();
echo "  Uncommitted Events: " . count($events) . "\n";
foreach ($events as $event) {
    echo "    - " . get_class($event) . "\n";
}
echo "\n";

// ============================================================================
// Step 2: Prepare the Sales Order
// ============================================================================

echo "📦 Step 2: Preparing the Sales Order\n";
echo str_repeat("-", 60) . "\n";

try {
    $salesOrder->prepare($who);
    echo "✓ Sales Order Prepared!\n";
    echo "  Status: {$salesOrder->getStatus()}\n";
    echo "  Version: {$salesOrder->getVersion()}\n";

    $events = $salesOrder->getUncommittedEvents();
    echo "  Total Uncommitted Events: " . count($events) . "\n";
    foreach ($events as $event) {
        echo "    - " . get_class($event) . "\n";
    }
    echo "\n";
} catch (\DomainException $e) {
    echo "✗ Error: {$e->getMessage()}\n\n";
}

// ============================================================================
// Step 3: Close the Sales Order
// ============================================================================

echo "🎯 Step 3: Closing the Sales Order\n";
echo str_repeat("-", 60) . "\n";

try {
    $salesOrder->close($who);
    echo "✓ Sales Order Closed!\n";
    echo "  Status: {$salesOrder->getStatus()}\n";
    echo "  Version: {$salesOrder->getVersion()}\n";

    $events = $salesOrder->getUncommittedEvents();
    echo "  Total Uncommitted Events: " . count($events) . "\n";
    foreach ($events as $event) {
        echo "    - " . get_class($event) . "\n";
    }
    echo "\n";
} catch (\DomainException $e) {
    echo "✗ Error: {$e->getMessage()}\n\n";
}

// ============================================================================
// Step 4: Demonstrate Business Rule Enforcement
// ============================================================================

echo "🛡️  Step 4: Demonstrating Business Rules\n";
echo str_repeat("-", 60) . "\n";

// Try to close an already closed order (should be idempotent)
echo "Attempting to close an already closed order...\n";
try {
    $salesOrder->close($who);
    echo "✓ Idempotent behavior: Closing already closed order is a no-op\n";
    echo "  Version: {$salesOrder->getVersion()} (unchanged)\n\n";
} catch (\DomainException $e) {
    echo "✗ Error: {$e->getMessage()}\n\n";
}

// Try to violate business rules
echo "Attempting to prepare a new order and then prepare again...\n";
$newOrder = SalesOrder::create(
    SalesOrderId::create(),
    SalesOrderNumber::create('SO-2025-002'),
    BeerName::create('Stout Supreme'),
    Quantity::create(12),
    $who
);
$newOrder->clearUncommittedEvents(); // Clear for demonstration

$newOrder->prepare($who);
echo "✓ First preparation successful\n";

try {
    $newOrder->prepare($who);
    echo "✗ Should have thrown exception!\n";
} catch (\DomainException $e) {
    echo "✓ Business rule enforced: {$e->getMessage()}\n\n";
}

// ============================================================================
// Step 5: Event Sourcing - Rebuild from Events
// ============================================================================

echo "🔄 Step 5: Event Sourcing - Rebuilding from Events\n";
echo str_repeat("-", 60) . "\n";

// Get all events from the original order
$allEvents = $salesOrder->getUncommittedEvents();
echo "Rebuilding aggregate from " . count($allEvents) . " events...\n";

// Create a new instance and replay events
$rebuiltOrder = new class extends SalesOrder {
    // Make constructor accessible for testing
    public function __construct() {
        parent::__construct();
    }
};

foreach ($allEvents as $event) {
    $rebuiltOrder->applyEvent($event);
}

echo "✓ Aggregate rebuilt from event stream!\n";
echo "  Order Number: {$rebuiltOrder->getSalesOrderNumber()}\n";
echo "  Beer: {$rebuiltOrder->getBeerName()}\n";
echo "  Quantity: {$rebuiltOrder->getQuantity()}\n";
echo "  Status: {$rebuiltOrder->getStatus()}\n";
echo "  Version: {$rebuiltOrder->getVersion()}\n";
echo "  Matches original: " . ($rebuiltOrder->getStatus() === $salesOrder->getStatus() ? 'Yes ✓' : 'No ✗') . "\n\n";

// ============================================================================
// Summary
// ============================================================================

echo "╔══════════════════════════════════════════════════════════╗\n";
echo "║  Summary                                                 ║\n";
echo "╚══════════════════════════════════════════════════════════╝\n\n";

echo "✓ Created sales order for 24 bottles of IPA Delight\n";
echo "✓ Prepared the order for fulfillment\n";
echo "✓ Closed the order successfully\n";
echo "✓ Enforced business rules (cannot prepare non-new orders)\n";
echo "✓ Demonstrated event sourcing by rebuilding aggregate\n";
echo "✓ Generated " . count($allEvents) . " domain events\n\n";

echo "This example demonstrates:\n";
echo "  • Aggregate Root pattern with business logic\n";
echo "  • Event sourcing with event application\n";
echo "  • Command pattern for behavior\n";
echo "  • Value Objects for type safety\n";
echo "  • Business rule enforcement\n";
echo "  • Event-based state reconstruction\n\n";

echo "Next steps:\n";
echo "  1. Implement IRepository to persist events\n";
echo "  2. Create command handlers with proper DI\n";
echo "  3. Build read models (projections) for queries\n";
echo "  4. Add integration/domain event handlers\n";
echo "  5. Implement event bus for cross-aggregate communication\n\n";
