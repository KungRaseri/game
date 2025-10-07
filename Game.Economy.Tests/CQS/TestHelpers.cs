#nullable enable

using Game.Core.CQS;
using Game.Core.Extensions;
using Game.Economy.Extensions;
using Game.Economy.Models;
using Game.Items.Models.Materials;
using Game.Items.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Game.Economy.Tests.CQS;

/// <summary>
/// Helper utilities for creating test data and mock objects for economy tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a mock dispatcher for testing handlers in isolation.
    /// </summary>
    public static Mock<IDispatcher> CreateMockDispatcher()
    {
        return new Mock<IDispatcher>();
    }

    /// <summary>
    /// Creates a service provider with all economy services registered for integration testing.
    /// </summary>
    public static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add Core CQS infrastructure
        services.AddCQS();
        
        // Add Economy services
        services.AddEconomyServices();
        
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a test ShopExpense with default values.
    /// </summary>
    public static ShopExpense CreateTestExpense(
        string expenseId = "test-expense-1",
        ExpenseType type = ExpenseType.Rent,
        decimal amount = 100m,
        string description = "Test expense",
        bool isRecurring = false,
        int recurrenceDays = 0)
    {
        return new ShopExpense(
            ExpenseId: expenseId,
            Type: type,
            Amount: amount,
            Description: description,
            ExpenseDate: DateTime.Now,
            IsRecurring: isRecurring,
            RecurrenceDays: recurrenceDays
        );
    }

    /// <summary>
    /// Creates a test InvestmentOpportunity with default values.
    /// </summary>
    public static InvestmentOpportunity CreateTestInvestment(
        string investmentId = "test-investment-1",
        InvestmentType type = InvestmentType.DisplayUpgrade,
        string name = "Test Investment",
        string description = "A test investment opportunity",
        decimal cost = 500m,
        decimal expectedReturn = 750m,
        int paybackPeriodDays = 30,
        bool isAvailable = true)
    {
        return new InvestmentOpportunity(
            InvestmentId: investmentId,
            Type: type,
            Name: name,
            Description: description,
            Cost: cost,
            ExpectedReturn: expectedReturn,
            PaybackPeriodDays: paybackPeriodDays,
            IsAvailable: isAvailable
        );
    }

    /// <summary>
    /// Creates multiple test expenses for bulk testing scenarios.
    /// </summary>
    public static List<ShopExpense> CreateTestExpenses(int count = 3)
    {
        var expenses = new List<ShopExpense>();
        
        for (int i = 1; i <= count; i++)
        {
            expenses.Add(CreateTestExpense(
                expenseId: $"test-expense-{i}",
                type: (ExpenseType)(i % Enum.GetValues<ExpenseType>().Length),
                amount: 50m * i,
                description: $"Test expense {i}"
            ));
        }
        
        return expenses;
    }

    /// <summary>
    /// Creates multiple test investments for bulk testing scenarios.
    /// </summary>
    public static List<InvestmentOpportunity> CreateTestInvestments(int count = 3)
    {
        var investments = new List<InvestmentOpportunity>();
        var investmentTypes = Enum.GetValues<InvestmentType>();
        
        for (int i = 1; i <= count; i++)
        {
            investments.Add(CreateTestInvestment(
                investmentId: $"test-investment-{i}",
                type: investmentTypes[i % investmentTypes.Length],
                name: $"Test Investment {i}",
                description: $"Test investment opportunity {i}",
                cost: 100m * i,
                expectedReturn: 150m * i,
                paybackPeriodDays: 20 + (i * 10)
            ));
        }
        
        return investments;
    }

    /// <summary>
    /// Creates a test FinancialSummary with default values.
    /// </summary>
    public static FinancialSummary CreateTestFinancialSummary(
        decimal currentTreasury = 1000m,
        decimal totalExpenses = 500m,
        decimal dailyExpenses = 50m)
    {
        return new FinancialSummary
        {
            CurrentTreasury = currentTreasury,
            TotalExpenses = totalExpenses,
            DailyExpenses = dailyExpenses,
            CashFlow = -dailyExpenses,
            ExpenseBreakdown = new List<ExpenseCategory>
            {
                new ExpenseCategory("Fixed Costs", 300m, 30m, 3, 60m),
                new ExpenseCategory("Variable Costs", 200m, 20m, 2, 40m)
            },
            FinancialAlerts = new List<string>(),
            FinancialHealth = "Good"
        };
    }

    /// <summary>
    /// Creates test materials for testing scenarios that involve materials.
    /// </summary>
    public static Dictionary<string, Material> CreateTestMaterials()
    {
        return new Dictionary<string, Material>
        {
            { "gold-1", new Material("gold-1", "Gold Coin", "A valuable gold coin", QualityTier.Common, 50, Category.Metal) },
            { "silver-1", new Material("silver-1", "Silver Coin", "A silver coin", QualityTier.Common, 25, Category.Metal) },
            { "gem-1", new Material("gem-1", "Ruby", "A precious ruby", QualityTier.Rare, 100, Category.Gem) }
        };
    }
}
