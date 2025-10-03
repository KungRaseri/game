#nullable enable

using FluentAssertions;
using Game.Core.Models;

namespace Game.Main.Tests.Models;

/// <summary>
/// Tests for the Budget model.
/// </summary>
public class BudgetTests
{
    [Fact]
    public void Budget_CanBeCreated_WithRecordSyntax()
    {
        // Act
        var budget = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 100,
            TypicalPurchaseRange = 50,
            CurrentFunds = 75
        };

        // Assert
        budget.MinSpendingPower.Should().Be(10);
        budget.MaxSpendingPower.Should().Be(100);
        budget.TypicalPurchaseRange.Should().Be(50);
        budget.CurrentFunds.Should().Be(75);
    }

    [Fact]
    public void CreateForType_NoviceAdventurer_ReturnsAppropriateRange()
    {
        // Act
        var budget = Budget.CreateForType(CustomerType.NoviceAdventurer);

        // Assert
        budget.MinSpendingPower.Should().Be(10);
        budget.MaxSpendingPower.Should().Be(100);
        budget.TypicalPurchaseRange.Should().Be(50);
        budget.CurrentFunds.Should().BeInRange(25, 75);
    }

    [Fact]
    public void CreateForType_VeteranAdventurer_ReturnsAppropriateRange()
    {
        // Act
        var budget = Budget.CreateForType(CustomerType.VeteranAdventurer);

        // Assert
        budget.MinSpendingPower.Should().Be(50);
        budget.MaxSpendingPower.Should().Be(500);
        budget.TypicalPurchaseRange.Should().Be(200);
        budget.CurrentFunds.Should().BeInRange(150, 450);
    }

    [Fact]
    public void CreateForType_NoblePatron_ReturnsAppropriateRange()
    {
        // Act
        var budget = Budget.CreateForType(CustomerType.NoblePatron);

        // Assert
        budget.MinSpendingPower.Should().Be(200);
        budget.MaxSpendingPower.Should().Be(2000);
        budget.TypicalPurchaseRange.Should().Be(800);
        budget.CurrentFunds.Should().BeInRange(500, 1500);
    }

    [Fact]
    public void CreateForType_MerchantTrader_ReturnsAppropriateRange()
    {
        // Act
        var budget = Budget.CreateForType(CustomerType.MerchantTrader);

        // Assert
        budget.MinSpendingPower.Should().Be(100);
        budget.MaxSpendingPower.Should().Be(1000);
        budget.TypicalPurchaseRange.Should().Be(400);
        budget.CurrentFunds.Should().BeInRange(200, 800);
    }

    [Fact]
    public void CreateForType_CasualTownsperson_ReturnsAppropriateRange()
    {
        // Act
        var budget = Budget.CreateForType(CustomerType.CasualTownsperson);

        // Assert
        budget.MinSpendingPower.Should().Be(5);
        budget.MaxSpendingPower.Should().Be(75);
        budget.TypicalPurchaseRange.Should().Be(25);
        budget.CurrentFunds.Should().BeInRange(10, 50);
    }

    [Fact]
    public void CreateForType_InvalidCustomerType_ReturnsDefaultRange()
    {
        // Act
        var budget = Budget.CreateForType((CustomerType)999);

        // Assert
        budget.MinSpendingPower.Should().Be(10);
        budget.MaxSpendingPower.Should().Be(100);
        budget.TypicalPurchaseRange.Should().Be(50);
        budget.CurrentFunds.Should().Be(50);
    }

    [Theory]
    [InlineData(25, true)]
    [InlineData(50, true)]
    [InlineData(75, true)]
    [InlineData(76, false)]
    [InlineData(100, false)]
    public void CanAfford_WithDifferentPrices_ReturnsCorrectResult(decimal price, bool expected)
    {
        // Arrange
        var budget = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 100,
            TypicalPurchaseRange = 50,
            CurrentFunds = 75
        };

        // Act
        var result = budget.CanAfford(price);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(25, true)]
    [InlineData(50, true)]
    [InlineData(51, false)]
    [InlineData(75, false)]
    [InlineData(100, false)]
    public void IsComfortablePrice_WithDifferentPrices_ReturnsCorrectResult(decimal price, bool expected)
    {
        // Arrange
        var budget = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 100,
            TypicalPurchaseRange = 50,
            CurrentFunds = 75
        };

        // Act
        var result = budget.IsComfortablePrice(price);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetMaxNegotiationOffer_NormalPrice_Returns90PercentOfAskingPrice()
    {
        // Arrange
        var budget = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 200,
            TypicalPurchaseRange = 50,
            CurrentFunds = 150
        };
        var askingPrice = 100m;

        // Act
        var maxOffer = budget.GetMaxNegotiationOffer(askingPrice);

        // Assert
        maxOffer.Should().Be(90m); // 90% of 100
    }

    [Fact]
    public void GetMaxNegotiationOffer_PriceExceedsCurrentFunds_ReturnsCurrentFunds()
    {
        // Arrange
        var budget = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 200,
            TypicalPurchaseRange = 50,
            CurrentFunds = 75
        };
        var askingPrice = 200m; // More than current funds

        // Act
        var maxOffer = budget.GetMaxNegotiationOffer(askingPrice);

        // Assert
        maxOffer.Should().Be(75m); // Limited by current funds
    }

    [Fact]
    public void GetMaxNegotiationOffer_LowTypicalRange_AdjustsToTypicalRange()
    {
        // Arrange
        var budget = new Budget
        {
            MinSpendingPower = 5,
            MaxSpendingPower = 100,
            TypicalPurchaseRange = 20,
            CurrentFunds = 80
        };
        var askingPrice = 30m;

        // Act
        var maxOffer = budget.GetMaxNegotiationOffer(askingPrice);

        // Assert
        // Normal 90% would be 27, but typical range adjustment might apply
        // Since 27 > (20 * 0.5 = 10), it should use the normal 90%
        maxOffer.Should().Be(27m);
    }

    [Fact]
    public void GetMaxNegotiationOffer_VeryLowOffer_AdjustsToTypicalRangePercentage()
    {
        // Arrange
        var budget = new Budget
        {
            MinSpendingPower = 1,
            MaxSpendingPower = 50,
            TypicalPurchaseRange = 20,
            CurrentFunds = 25
        };
        var askingPrice = 10m; // 90% would be 9, which is less than typical range * 0.5 (10)

        // Act
        var maxOffer = budget.GetMaxNegotiationOffer(askingPrice);

        // Assert
        // Should use typical range adjustment: min(25, 20 * 0.7) = 14
        maxOffer.Should().Be(14m);
    }

    [Fact]
    public void GetMaxNegotiationOffer_ZeroPrice_ReturnsTypicalRangeBasedOffer()
    {
        // Arrange
        var budget = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 100,
            TypicalPurchaseRange = 50,
            CurrentFunds = 75
        };
        var askingPrice = 0m;

        // Act
        var maxOffer = budget.GetMaxNegotiationOffer(askingPrice);

        // Assert - When asking price is 0, fallback to 70% of typical range
        maxOffer.Should().Be(35m);
    }

    [Fact]
    public void Budget_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var budget1 = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 100,
            TypicalPurchaseRange = 50,
            CurrentFunds = 75
        };

        var budget2 = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 100,
            TypicalPurchaseRange = 50,
            CurrentFunds = 75
        };

        var budget3 = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 100,
            TypicalPurchaseRange = 50,
            CurrentFunds = 80 // Different value
        };

        // Act & Assert
        budget1.Should().Be(budget2);
        budget1.Should().NotBe(budget3);
        (budget1 == budget2).Should().BeTrue();
        (budget1 == budget3).Should().BeFalse();
    }

    [Fact]
    public void Budget_CustomerTypesHaveCorrectSpendingHierarchy()
    {
        // Act
        var casual = Budget.CreateForType(CustomerType.CasualTownsperson);
        var novice = Budget.CreateForType(CustomerType.NoviceAdventurer);
        var veteran = Budget.CreateForType(CustomerType.VeteranAdventurer);
        var merchant = Budget.CreateForType(CustomerType.MerchantTrader);
        var noble = Budget.CreateForType(CustomerType.NoblePatron);

        // Assert - Verify spending power hierarchy
        casual.MaxSpendingPower.Should().BeLessThan(novice.MaxSpendingPower);
        novice.MaxSpendingPower.Should().BeLessThan(veteran.MaxSpendingPower);
        veteran.MaxSpendingPower.Should().BeLessThan(merchant.MaxSpendingPower);
        merchant.MaxSpendingPower.Should().BeLessThan(noble.MaxSpendingPower);

        // Verify typical purchase range hierarchy
        casual.TypicalPurchaseRange.Should().BeLessThan(novice.TypicalPurchaseRange);
        novice.TypicalPurchaseRange.Should().BeLessThan(veteran.TypicalPurchaseRange);
        veteran.TypicalPurchaseRange.Should().BeLessThan(merchant.TypicalPurchaseRange);
        merchant.TypicalPurchaseRange.Should().BeLessThan(noble.TypicalPurchaseRange);
    }

    [Fact]
    public void CreateForType_MultipleCallsSameType_ProducesVariedCurrentFunds()
    {
        // Act - Create multiple budgets for the same type
        var budgets = Enumerable.Range(0, 10)
            .Select(_ => Budget.CreateForType(CustomerType.VeteranAdventurer))
            .ToList();

        // Assert - Should have variety in current funds due to randomization
        var uniqueFundsCount = budgets.Select(b => b.CurrentFunds).Distinct().Count();
        uniqueFundsCount.Should().BeGreaterThan(1, "Random generation should produce varied current funds");

        // All should still be within the expected range
        budgets.Should().AllSatisfy(budget =>
        {
            budget.CurrentFunds.Should().BeInRange(150, 450);
        });
    }

    [Theory]
    [InlineData(CustomerType.NoviceAdventurer, 25, 75)]
    [InlineData(CustomerType.VeteranAdventurer, 150, 450)]
    [InlineData(CustomerType.NoblePatron, 500, 1500)]
    [InlineData(CustomerType.MerchantTrader, 200, 800)]
    [InlineData(CustomerType.CasualTownsperson, 10, 50)]
    public void CreateForType_CurrentFundsAlwaysWithinExpectedRange(CustomerType type, int minExpected, int maxExpected)
    {
        // Act - Create multiple budgets and verify range
        var budgets = Enumerable.Range(0, 20)
            .Select(_ => Budget.CreateForType(type))
            .ToList();

        // Assert
        budgets.Should().AllSatisfy(budget =>
        {
            budget.CurrentFunds.Should().BeInRange(minExpected, maxExpected);
        });
    }

    [Fact]
    public void Budget_PropertiesAreImmutable()
    {
        // Arrange
        var originalBudget = new Budget
        {
            MinSpendingPower = 10,
            MaxSpendingPower = 100,
            TypicalPurchaseRange = 50,
            CurrentFunds = 75
        };

        // Act - Create modified budget (records support 'with' expressions)
        var modifiedBudget = originalBudget with { CurrentFunds = 100 };

        // Assert
        originalBudget.CurrentFunds.Should().Be(75);
        modifiedBudget.CurrentFunds.Should().Be(100);
        originalBudget.Should().NotBe(modifiedBudget);
    }
}
