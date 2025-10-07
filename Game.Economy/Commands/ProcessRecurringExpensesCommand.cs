#nullable enable

using Game.Core.CQS;

namespace Game.Economy.Commands;

/// <summary>
/// Command to process all recurring expenses that are due.
/// </summary>
public record ProcessRecurringExpensesCommand : ICommand
{
}
