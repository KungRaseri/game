#nullable enable

using FluentAssertions;
using Game.Economy.Handlers;
using Game.Economy.Models;
using Game.Economy.Queries;
using Game.Economy.Systems;
using Game.Economy.Tests.CQS;
using Moq;

namespace Game.Economy.Tests.Handlers;

/// <summary>
/// Unit tests for query handlers in the Economy module.
/// </summary>
public class QueryHandlerTests
{
    private readonly Mock<ITreasuryManager> _mockTreasuryManager;

    public QueryHandlerTests()
    {
        _mockTreasuryManager = new Mock<ITreasuryManager>();
    }

    #region GetFinancialSummaryQueryHandler Tests

    [Fact]
    public void GetFinancialSummaryQueryHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetFinancialSummaryQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetFinancialSummaryQueryHandler_HandleAsync_ReturnsFinancialSummary()
    {
        // Arrange
        var expectedSummary = TestHelpers.CreateTestFinancialSummary();
        var query = new GetFinancialSummaryQuery();
        var handler = new GetFinancialSummaryQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.GetFinancialSummary())
            .Returns(expectedSummary);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedSummary);
        _mockTreasuryManager.Verify(x => x.GetFinancialSummary(), Times.Once);
    }

    #endregion

    #region GetCurrentGoldQueryHandler Tests

    [Fact]
    public void GetCurrentGoldQueryHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetCurrentGoldQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetCurrentGoldQueryHandler_HandleAsync_ReturnsCurrentGold()
    {
        // Arrange
        var expectedGold = 1500m;
        var query = new GetCurrentGoldQuery();
        var handler = new GetCurrentGoldQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.CurrentGold)
            .Returns(expectedGold);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().Be(expectedGold);
        _mockTreasuryManager.Verify(x => x.CurrentGold, Times.Once);
    }

    #endregion

    #region GetExpenseHistoryQueryHandler Tests

    [Fact]
    public void GetExpenseHistoryQueryHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetExpenseHistoryQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetExpenseHistoryQueryHandler_HandleAsync_WithoutFilters_ReturnsAllExpenses()
    {
        // Arrange
        var expectedExpenses = TestHelpers.CreateTestExpenses(3);
        var query = new GetExpenseHistoryQuery();
        var handler = new GetExpenseHistoryQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.GetExpenseHistory(null, null, null, false))
            .Returns(expectedExpenses);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeEquivalentTo(expectedExpenses);
        _mockTreasuryManager.Verify(x => x.GetExpenseHistory(null, null, null, false), Times.Once);
    }

    [Fact]
    public async Task GetExpenseHistoryQueryHandler_HandleAsync_WithFilters_ReturnsFilteredExpenses()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var expectedExpenses = TestHelpers.CreateTestExpenses(2);
        var query = new GetExpenseHistoryQuery
        {
            ExpenseType = ExpenseType.Rent,
            StartDate = startDate,
            EndDate = endDate,
            RecurringOnly = true
        };
        var handler = new GetExpenseHistoryQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.GetExpenseHistory(ExpenseType.Rent, startDate, endDate, true))
            .Returns(expectedExpenses);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeEquivalentTo(expectedExpenses);
        _mockTreasuryManager.Verify(x => x.GetExpenseHistory(ExpenseType.Rent, startDate, endDate, true), Times.Once);
    }

    #endregion

    #region GetAvailableInvestmentsQueryHandler Tests

    [Fact]
    public void GetAvailableInvestmentsQueryHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetAvailableInvestmentsQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAvailableInvestmentsQueryHandler_HandleAsync_WithoutFilters_ReturnsAllInvestments()
    {
        // Arrange
        var expectedInvestments = TestHelpers.CreateTestInvestments(3);
        var query = new GetAvailableInvestmentsQuery();
        var handler = new GetAvailableInvestmentsQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.GetAvailableInvestments(false, null))
            .Returns(expectedInvestments);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeEquivalentTo(expectedInvestments);
        _mockTreasuryManager.Verify(x => x.GetAvailableInvestments(false, null), Times.Once);
    }

    [Fact]
    public async Task GetAvailableInvestmentsQueryHandler_HandleAsync_WithFilters_ReturnsFilteredInvestments()
    {
        // Arrange
        var expectedInvestments = TestHelpers.CreateTestInvestments(2);
        var query = new GetAvailableInvestmentsQuery
        {
            AffordableOnly = true,
            InvestmentType = InvestmentType.DisplayUpgrade
        };
        var handler = new GetAvailableInvestmentsQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.GetAvailableInvestments(true, InvestmentType.DisplayUpgrade))
            .Returns(expectedInvestments);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeEquivalentTo(expectedInvestments);
        _mockTreasuryManager.Verify(x => x.GetAvailableInvestments(true, InvestmentType.DisplayUpgrade), Times.Once);
    }

    #endregion

    #region GetCompletedInvestmentsQueryHandler Tests

    [Fact]
    public void GetCompletedInvestmentsQueryHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetCompletedInvestmentsQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetCompletedInvestmentsQueryHandler_HandleAsync_ReturnsCompletedInvestments()
    {
        // Arrange
        var expectedInvestments = TestHelpers.CreateTestInvestments(2);
        var query = new GetCompletedInvestmentsQuery();
        var handler = new GetCompletedInvestmentsQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.CompletedInvestments)
            .Returns(expectedInvestments);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedInvestments);
        _mockTreasuryManager.Verify(x => x.CompletedInvestments, Times.Once);
    }

    #endregion

    #region GetRecommendedInvestmentsQueryHandler Tests

    [Fact]
    public void GetRecommendedInvestmentsQueryHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetRecommendedInvestmentsQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetRecommendedInvestmentsQueryHandler_HandleAsync_ReturnsRecommendedInvestments()
    {
        // Arrange
        var allRecommendations = TestHelpers.CreateTestInvestments(7);
        var query = new GetRecommendedInvestmentsQuery { MaxRecommendations = 3 };
        var handler = new GetRecommendedInvestmentsQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.GetRecommendedInvestments())
            .Returns(allRecommendations);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(3);
        _mockTreasuryManager.Verify(x => x.GetRecommendedInvestments(), Times.Once);
    }

    [Fact]
    public async Task GetRecommendedInvestmentsQueryHandler_HandleAsync_DefaultMaxRecommendations_ReturnsUpToFive()
    {
        // Arrange
        var allRecommendations = TestHelpers.CreateTestInvestments(3);
        var query = new GetRecommendedInvestmentsQuery(); // Default MaxRecommendations = 5
        var handler = new GetRecommendedInvestmentsQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.GetRecommendedInvestments())
            .Returns(allRecommendations);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(3); // Should return all 3 since it's less than max of 5
        _mockTreasuryManager.Verify(x => x.GetRecommendedInvestments(), Times.Once);
    }

    #endregion

    #region GetMonthlyBudgetQueryHandler Tests

    [Fact]
    public void GetMonthlyBudgetQueryHandler_Constructor_WithNullTreasuryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetMonthlyBudgetQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetMonthlyBudgetQueryHandler_HandleAsync_ExistingBudget_ReturnsBudgetAmount()
    {
        // Arrange
        var expectedBudget = 200m;
        var query = new GetMonthlyBudgetQuery { ExpenseType = ExpenseType.Rent };
        var handler = new GetMonthlyBudgetQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.GetMonthlyBudget(ExpenseType.Rent))
            .Returns(expectedBudget);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().Be(expectedBudget);
        _mockTreasuryManager.Verify(x => x.GetMonthlyBudget(ExpenseType.Rent), Times.Once);
    }

    [Fact]
    public async Task GetMonthlyBudgetQueryHandler_HandleAsync_NonExistingBudget_ReturnsNull()
    {
        // Arrange
        var query = new GetMonthlyBudgetQuery { ExpenseType = ExpenseType.Rent };
        var handler = new GetMonthlyBudgetQueryHandler(_mockTreasuryManager.Object);

        _mockTreasuryManager.Setup(x => x.GetMonthlyBudget(ExpenseType.Rent))
            .Returns((decimal?)null);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
        _mockTreasuryManager.Verify(x => x.GetMonthlyBudget(ExpenseType.Rent), Times.Once);
    }

    #endregion

    #region General Tests

    [Fact]
    public async Task AllQueryHandlers_HandleAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        
        _mockTreasuryManager.Setup(x => x.GetFinancialSummary())
            .Returns(TestHelpers.CreateTestFinancialSummary());
        _mockTreasuryManager.Setup(x => x.CurrentGold)
            .Returns(1000m);
        _mockTreasuryManager.Setup(x => x.GetExpenseHistory(It.IsAny<ExpenseType?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<bool>()))
            .Returns(TestHelpers.CreateTestExpenses());
        _mockTreasuryManager.Setup(x => x.GetAvailableInvestments(It.IsAny<bool>(), It.IsAny<InvestmentType?>()))
            .Returns(TestHelpers.CreateTestInvestments());
        _mockTreasuryManager.Setup(x => x.CompletedInvestments)
            .Returns(TestHelpers.CreateTestInvestments());
        _mockTreasuryManager.Setup(x => x.GetRecommendedInvestments())
            .Returns(TestHelpers.CreateTestInvestments());
        _mockTreasuryManager.Setup(x => x.GetMonthlyBudget(It.IsAny<ExpenseType>()))
            .Returns(100m);

        var summaryHandler = new GetFinancialSummaryQueryHandler(_mockTreasuryManager.Object);
        var goldHandler = new GetCurrentGoldQueryHandler(_mockTreasuryManager.Object);
        var expenseHandler = new GetExpenseHistoryQueryHandler(_mockTreasuryManager.Object);
        var investmentHandler = new GetAvailableInvestmentsQueryHandler(_mockTreasuryManager.Object);
        var completedHandler = new GetCompletedInvestmentsQueryHandler(_mockTreasuryManager.Object);
        var recommendedHandler = new GetRecommendedInvestmentsQueryHandler(_mockTreasuryManager.Object);
        var budgetHandler = new GetMonthlyBudgetQueryHandler(_mockTreasuryManager.Object);

        // Act & Assert
        await summaryHandler.HandleAsync(new GetFinancialSummaryQuery(), cancellationToken);
        await goldHandler.HandleAsync(new GetCurrentGoldQuery(), cancellationToken);
        await expenseHandler.HandleAsync(new GetExpenseHistoryQuery(), cancellationToken);
        await investmentHandler.HandleAsync(new GetAvailableInvestmentsQuery(), cancellationToken);
        await completedHandler.HandleAsync(new GetCompletedInvestmentsQuery(), cancellationToken);
        await recommendedHandler.HandleAsync(new GetRecommendedInvestmentsQuery(), cancellationToken);
        await budgetHandler.HandleAsync(new GetMonthlyBudgetQuery { ExpenseType = ExpenseType.Rent }, cancellationToken);

        // All handlers should complete without throwing
    }

    #endregion
}
