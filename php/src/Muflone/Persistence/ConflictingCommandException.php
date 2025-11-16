<?php

declare(strict_types=1);

namespace Muflone\Persistence;

use Exception;

/**
 * Represents a command that could not be executed because it conflicted with the command of another user or actor.
 */
class ConflictingCommandException extends Exception
{
}
