#nullable enable

using FluentAssertions;
using Game.Economy.Models;

namespace Game.Economy.Tests.Models;

public class InvestmentOpportunityTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Premium Display Cases",
            Description: "Upgrade to premium display cases",
            Cost: 500m,
            ExpectedReturn: 800m,
            PaybackPeriodDays: 30,
            IsAvailable: true
        );

        // Assert
        investment.InvestmentId.Should().Be("INV001");
        investment.Type.Should().Be(InvestmentType.DisplayUpgrade);
        investment.Name.Should().Be("Premium Display Cases");
        investment.Description.Should().Be("Upgrade to premium display cases");
        investment.Cost.Should().Be(500m);
        investment.ExpectedReturn.Should().Be(800m);
        investment.PaybackPeriodDays.Should().Be(30);
        investment.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void ExpectedProfit_CalculatesCorrectly()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 300m,
            ExpectedReturn: 500m,
            PaybackPeriodDays: 20
        );

        // Act
        var profit = investment.ExpectedProfit;

        // Assert
        profit.Should().Be(200m); // 500 - 300
    }

    [Fact]
    public void ROIPercentage_CalculatesCorrectly()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 400m,
            ExpectedReturn: 600m,
            PaybackPeriodDays: 25
        );

        // Act
        var roi = investment.ROIPercentage;

        // Assert
        roi.Should().Be(50m); // (200 profit / 400 cost) * 100
    }

    [Fact]
    public void ROIPercentage_WithZeroCost_ReturnsZero()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 0m,
            ExpectedReturn: 500m,
            PaybackPeriodDays: 25
        );

        // Act
        var roi = investment.ROIPercentage;

        // Assert
        roi.Should().Be(0m);
    }

    [Fact]
    public void DailyReturnRate_CalculatesCorrectly()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 400m,
            ExpectedReturn: 600m,
            PaybackPeriodDays: 30
        );

        // Act
        var dailyReturn = investment.DailyReturnRate;

        // Assert
        dailyReturn.Should().Be(20m); // 600 / 30
    }

    [Fact]
    public void DailyReturnRate_WithZeroPaybackPeriod_ReturnsZero()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 400m,
            ExpectedReturn: 600m,
            PaybackPeriodDays: 0
        );

        // Act
        var dailyReturn = investment.DailyReturnRate;

        // Assert
        dailyReturn.Should().Be(0m);
    }

    [Fact]
    public void GetRiskCategory_WithHighROI_ReturnsHighRisk()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 200m, // 100% ROI
            PaybackPeriodDays: 10
        );

        // Act
        var risk = investment.GetRiskCategory();

        // Assert
        risk.Should().Be("High Risk/High Reward");
    }

    [Fact]
    public void GetRiskCategory_WithModerateROI_ReturnsModerateRisk()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 130m, // 30% ROI
            PaybackPeriodDays: 20
        );

        // Act
        var risk = investment.GetRiskCategory();

        // Assert
        risk.Should().Be("Moderate Risk");
    }

    [Fact]
    public void GetRiskCategory_WithLowROI_ReturnsLowRisk()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 115m, // 15% ROI
            PaybackPeriodDays: 30
        );

        // Act
        var risk = investment.GetRiskCategory();

        // Assert
        risk.Should().Be("Low Risk");
    }

    [Fact]
    public void GetRiskCategory_WithVeryLowROI_ReturnsConservative()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 105m, // 5% ROI
            PaybackPeriodDays: 40
        );

        // Act
        var risk = investment.GetRiskCategory();

        // Assert
        risk.Should().Be("Conservative");
    }

    [Fact]
    public void GetBenefitsDescription_ForDisplayUpgrade_ReturnsCorrectDescription()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20
        );

        // Act
        var benefits = investment.GetBenefitsDescription();

        // Assert
        benefits.Should().Contain("customer interest");
    }

    [Fact]
    public void GetBenefitsDescription_ForShopExpansion_ReturnsCorrectDescription()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.ShopExpansion,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20
        );

        // Act
        var benefits = investment.GetBenefitsDescription();

        // Assert
        benefits.Should().Contain("displaying more items");
    }

    [Fact]
    public void GetBenefitsDescription_ForStaffHiring_ReturnsCorrectDescription()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.StaffHiring,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20
        );

        // Act
        var benefits = investment.GetBenefitsDescription();

        // Assert
        benefits.Should().Contain("customer service");
    }

    [Fact]
    public void GetBenefitsDescription_ForMarketingCampaign_ReturnsCorrectDescription()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.MarketingCampaign,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20
        );

        // Act
        var benefits = investment.GetBenefitsDescription();

        // Assert
        benefits.Should().Contain("foot traffic");
    }

    [Fact]
    public void IsAffordable_WithSufficientGold_ReturnsTrue()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20,
            IsAvailable: true
        );

        // Act
        var affordable = investment.IsAffordable(200m);

        // Assert
        affordable.Should().BeTrue();
    }

    [Fact]
    public void IsAffordable_WithInsufficientGold_ReturnsFalse()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20,
            IsAvailable: true
        );

        // Act
        var affordable = investment.IsAffordable(50m);

        // Assert
        affordable.Should().BeFalse();
    }

    [Fact]
    public void IsAffordable_WithUnavailableInvestment_ReturnsFalse()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20,
            IsAvailable: false
        );

        // Act
        var affordable = investment.IsAffordable(200m);

        // Assert
        affordable.Should().BeFalse();
    }

    [Fact]
    public void IsAffordable_WithExactGoldAmount_ReturnsTrue()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20,
            IsAvailable: true
        );

        // Act
        var affordable = investment.IsAffordable(100m);

        // Assert
        affordable.Should().BeTrue();
    }

    [Fact]
    public void DefaultIsAvailable_IsTrue()
    {
        // Arrange & Act
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20
        );

        // Assert
        investment.IsAvailable.Should().BeTrue();
    }

    [Theory]
    [InlineData(InvestmentType.SecurityUpgrade, "theft risk")]
    [InlineData(InvestmentType.InventoryExpansion, "higher-value items")]
    [InlineData(InvestmentType.AestheticUpgrade, "customer satisfaction")]
    [InlineData(InvestmentType.TechnologyUpgrade, "operating costs")]
    [InlineData(InvestmentType.StorageUpgrade, "inventory capacity")]
    public void GetBenefitsDescription_ForVariousTypes_ReturnsAppropriateDescription(InvestmentType type,
        string expectedKeyword)
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: type,
            Name: "Test Investment",
            Description: "Test",
            Cost: 100m,
            ExpectedReturn: 150m,
            PaybackPeriodDays: 20
        );

        // Act
        var benefits = investment.GetBenefitsDescription();

        // Assert
        benefits.Should().Contain(expectedKeyword);
    }

    [Fact]
    public void NegativeROI_CalculatesCorrectly()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 500m,
            ExpectedReturn: 300m, // Loss
            PaybackPeriodDays: 30
        );

        // Act
        var roi = investment.ROIPercentage;
        var profit = investment.ExpectedProfit;

        // Assert
        roi.Should().Be(-40m); // (300-500)/500 * 100 = -40%
        profit.Should().Be(-200m); // 300 - 500
    }

    [Fact]
    public void GetRiskCategory_WithNegativeROI_ReturnsConservative()
    {
        // Arrange
        var investment = new InvestmentOpportunity(
            InvestmentId: "INV001",
            Type: InvestmentType.DisplayUpgrade,
            Name: "Test Investment",
            Description: "Test",
            Cost: 500m,
            ExpectedReturn: 400m, // Negative ROI
            PaybackPeriodDays: 30
        );

        // Act
        var risk = investment.GetRiskCategory();

        // Assert
        risk.Should().Be("Conservative");
    }
}