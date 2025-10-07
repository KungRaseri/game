#nullable enable

using FluentAssertions;
using Game.Economy.Commands;
using Game.Economy.Handlers;
using Game.Economy.Models;
using Game.Economy.Systems;
using Moq;

namespace Game.Economy.Tests.Handlers;

/// <summary>
/// Unit tests for command handlers in the Economy module.
/// </summary>
public class CommandHandlerTests
{
    private readonly Mock<ITreasuryManager> _mockTreasuryManager;

    public CommandHandlerTests()
    {
        _mockTreasuryManager = new Mock<ITreasuryManager>();
    }

    #region AddRevenueCommandHandler Tests

    [Fact]
    public void AddRevenueCommandHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new AddRevenueCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task AddRevenueCommandHandler_HandleAsync_ValidCommand_AddsRevenue()
    {
        // Arrange
        var command = new AddRevenueCommand { Amount = 100m, Source = "Test Sale" };
        var handler = new AddRevenueCommandHandler(_mockTreasuryManager.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockTreasuryManager.Verify(x => x.AddRevenue(100m, "Test Sale"), Times.Once);
    }

    [Fact]
    public async Task AddRevenueCommandHandler_HandleAsync_InvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var command = new AddRevenueCommand { Amount = -10m, Source = "Invalid" };
        var handler = new AddRevenueCommandHandler(_mockTreasuryManager.Object);

        // Act & Assert
        var action = () => handler.HandleAsync(command);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region ProcessExpenseCommandHandler Tests

    [Fact]
    public void ProcessExpenseCommandHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new ProcessExpenseCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessExpenseCommandHandler_HandleAsync_ValidCommand_ProcessesExpense()
    {
        // Arrange
        var command = new ProcessExpenseCommand
        {
            Type = ExpenseType.Rent,
            Amount = 200m,
            Description = "Monthly rent",
            IsRecurring = true,
            RecurrenceDays = 30
        };
        var handler = new ProcessExpenseCommandHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.ProcessExpense(
            ExpenseType.Rent, 200m, "Monthly rent", true, 30))
            .Returns(true);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
        _mockTreasuryManager.Verify(x => x.ProcessExpense(
            ExpenseType.Rent, 200m, "Monthly rent", true, 30), Times.Once);
    }

    [Fact]
    public async Task ProcessExpenseCommandHandler_HandleAsync_InvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var command = new ProcessExpenseCommand
        {
            Type = ExpenseType.Rent,
            Amount = -100m,
            Description = "Invalid expense"
        };
        var handler = new ProcessExpenseCommandHandler(_mockTreasuryManager.Object);

        // Act & Assert
        var action = () => handler.HandleAsync(command);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ProcessExpenseCommandHandler_HandleAsync_EmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        var command = new ProcessExpenseCommand
        {
            Type = ExpenseType.Rent,
            Amount = 100m,
            Description = ""
        };
        var handler = new ProcessExpenseCommandHandler(_mockTreasuryManager.Object);

        // Act & Assert
        var action = () => handler.HandleAsync(command);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region MakeInvestmentCommandHandler Tests

    [Fact]
    public void MakeInvestmentCommandHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new MakeInvestmentCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task MakeInvestmentCommandHandler_HandleAsync_ValidCommand_MakesInvestment()
    {
        // Arrange
        var command = new MakeInvestmentCommand { InvestmentId = "test-investment-1" };
        var handler = new MakeInvestmentCommandHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.MakeInvestment("test-investment-1"))
            .Returns(true);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
        _mockTreasuryManager.Verify(x => x.MakeInvestment("test-investment-1"), Times.Once);
    }

    [Fact]
    public async Task MakeInvestmentCommandHandler_HandleAsync_EmptyInvestmentId_ThrowsArgumentException()
    {
        // Arrange
        var command = new MakeInvestmentCommand { InvestmentId = "" };
        var handler = new MakeInvestmentCommandHandler(_mockTreasuryManager.Object);

        // Act & Assert
        var action = () => handler.HandleAsync(command);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region SetMonthlyBudgetCommandHandler Tests

    [Fact]
    public void SetMonthlyBudgetCommandHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new SetMonthlyBudgetCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SetMonthlyBudgetCommandHandler_HandleAsync_ValidCommand_SetsBudget()
    {
        // Arrange
        var command = new SetMonthlyBudgetCommand
        {
            ExpenseType = ExpenseType.Marketing,
            Amount = 150m
        };
        var handler = new SetMonthlyBudgetCommandHandler(_mockTreasuryManager.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockTreasuryManager.Verify(x => x.SetMonthlyBudget(ExpenseType.Marketing, 150m), Times.Once);
    }

    [Fact]
    public async Task SetMonthlyBudgetCommandHandler_HandleAsync_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var command = new SetMonthlyBudgetCommand
        {
            ExpenseType = ExpenseType.Marketing,
            Amount = -50m
        };
        var handler = new SetMonthlyBudgetCommandHandler(_mockTreasuryManager.Object);

        // Act & Assert
        var action = () => handler.HandleAsync(command);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region ProcessRecurringExpensesCommandHandler Tests

    [Fact]
    public void ProcessRecurringExpensesCommandHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new ProcessRecurringExpensesCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessRecurringExpensesCommandHandler_HandleAsync_ValidCommand_ProcessesRecurringExpenses()
    {
        // Arrange
        var command = new ProcessRecurringExpensesCommand();
        var handler = new ProcessRecurringExpensesCommandHandler(_mockTreasuryManager.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockTreasuryManager.Verify(x => x.ProcessRecurringExpenses(), Times.Once);
    }

    #endregion

    #region General Tests

    [Fact]
    public async Task AllCommandHandlers_HandleAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        
        _mockTreasuryManager.Setup(x => x.ProcessExpense(It.IsAny<ExpenseType>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
            .Returns(true);
        _mockTreasuryManager.Setup(x => x.MakeInvestment(It.IsAny<string>()))
            .Returns(true);

        var addRevenueHandler = new AddRevenueCommandHandler(_mockTreasuryManager.Object);
        var processExpenseHandler = new ProcessExpenseCommandHandler(_mockTreasuryManager.Object);
        var makeInvestmentHandler = new MakeInvestmentCommandHandler(_mockTreasuryManager.Object);
        var setBudgetHandler = new SetMonthlyBudgetCommandHandler(_mockTreasuryManager.Object);
        var processRecurringHandler = new ProcessRecurringExpensesCommandHandler(_mockTreasuryManager.Object);

        // Act & Assert
        await addRevenueHandler.HandleAsync(new AddRevenueCommand { Amount = 100m }, cancellationToken);
        await processExpenseHandler.HandleAsync(new ProcessExpenseCommand { Type = ExpenseType.Rent, Amount = 100m, Description = "Test" }, cancellationToken);
        await makeInvestmentHandler.HandleAsync(new MakeInvestmentCommand { InvestmentId = "test" }, cancellationToken);
        await setBudgetHandler.HandleAsync(new SetMonthlyBudgetCommand { ExpenseType = ExpenseType.Rent, Amount = 100m }, cancellationToken);
        await processRecurringHandler.HandleAsync(new ProcessRecurringExpensesCommand(), cancellationToken);

        // All handlers should complete without throwing
    }

    #endregion
}
