#nullable enable

using Game.Items.Models;
using Game.Shop.Models;

namespace Game.Shop.Tests.Models;

/// <summary>
/// Tests for Customer preference and loyalty system.
/// </summary>
public class CustomerLoyaltyTests
{
    [Theory]
    [InlineData(CustomerType.NoviceAdventurer)]
    [InlineData(CustomerType.VeteranAdventurer)]
    [InlineData(CustomerType.NoblePatron)]
    [InlineData(CustomerType.MerchantTrader)]
    [InlineData(CustomerType.CasualTownsperson)]
    public void ItemPreferences_CreateForType_ReturnsValidPreferences(CustomerType type)
    {
        // Act
        var preferences = ItemPreferences.CreateForType(type);

        // Assert
        Assert.True(preferences.WeaponPreference >= 0f && preferences.WeaponPreference <= 1f);
        Assert.True(preferences.ArmorPreference >= 0f && preferences.ArmorPreference <= 1f);
        Assert.True(preferences.MaterialPreference >= 0f && preferences.MaterialPreference <= 1f);
        Assert.True(preferences.ConsumablePreference >= 0f && preferences.ConsumablePreference <= 1f);

        Assert.True(preferences.CommonQualityTolerance >= 0f && preferences.CommonQualityTolerance <= 1f);
        Assert.True(preferences.UncommonPreference >= 0f && preferences.UncommonPreference <= 1f);
        Assert.True(preferences.RareDesire >= 0f && preferences.RareDesire <= 1f);
        Assert.True(preferences.EpicAspiration >= 0f && preferences.EpicAspiration <= 1f);
        Assert.True(preferences.LegendaryDream >= 0f && preferences.LegendaryDream <= 1f);
    }

    [Fact]
    public void ItemPreferences_GetTypePreference_ReturnsCorrectValues()
    {
        // Arrange
        var preferences = new ItemPreferences(
            WeaponPreference: 0.8f,
            ArmorPreference: 0.6f,
            MaterialPreference: 0.4f,
            ConsumablePreference: 0.7f,
            CommonQualityTolerance: 0.5f,
            UncommonPreference: 0.6f,
            RareDesire: 0.7f,
            EpicAspiration: 0.8f,
            LegendaryDream: 0.9f
        );

        // Act & Assert
        Assert.Equal(0.8f, preferences.GetTypePreference(ItemType.Weapon));
        Assert.Equal(0.6f, preferences.GetTypePreference(ItemType.Armor));
        Assert.Equal(0.4f, preferences.GetTypePreference(ItemType.Material));
        Assert.Equal(0.7f, preferences.GetTypePreference(ItemType.Consumable));
    }

    [Fact]
    public void ItemPreferences_GetQualityPreference_ReturnsCorrectValues()
    {
        // Arrange
        var preferences = new ItemPreferences(
            WeaponPreference: 0.5f,
            ArmorPreference: 0.5f,
            MaterialPreference: 0.5f,
            ConsumablePreference: 0.5f,
            CommonQualityTolerance: 0.9f,
            UncommonPreference: 0.7f,
            RareDesire: 0.5f,
            EpicAspiration: 0.3f,
            LegendaryDream: 0.1f
        );

        // Act & Assert
        Assert.Equal(0.9f, preferences.GetQualityPreference(QualityTier.Common));
        Assert.Equal(0.7f, preferences.GetQualityPreference(QualityTier.Uncommon));
        Assert.Equal(0.5f, preferences.GetQualityPreference(QualityTier.Rare));
        Assert.Equal(0.3f, preferences.GetQualityPreference(QualityTier.Epic));
        Assert.Equal(0.1f, preferences.GetQualityPreference(QualityTier.Legendary));
    }

    [Fact]
    public void ItemPreferences_NoviceAdventurer_HasExpectedPattern()
    {
        // Act
        var preferences = ItemPreferences.CreateForType(CustomerType.NoviceAdventurer);

        // Assert - Novices should prefer weapons/armor and accept common quality
        Assert.True(preferences.WeaponPreference > 0.6f);
        Assert.True(preferences.ArmorPreference > 0.5f);
        Assert.True(preferences.CommonQualityTolerance > 0.8f);
        Assert.True(preferences.LegendaryDream < 0.2f); // Can't afford legendary
    }

    [Fact]
    public void ItemPreferences_NoblePatron_HasExpectedPattern()
    {
        // Act
        var preferences = ItemPreferences.CreateForType(CustomerType.NoblePatron);

        // Assert - Nobles should prefer high quality and avoid common items
        Assert.True(preferences.CommonQualityTolerance < 0.2f);
        Assert.True(preferences.RareDesire > 0.6f);
        Assert.True(preferences.EpicAspiration > 0.9f);
        Assert.True(preferences.LegendaryDream > 0.8f);
    }

    [Fact]
    public void ItemPreferences_MerchantTrader_HasExpectedPattern()
    {
        // Act
        var preferences = ItemPreferences.CreateForType(CustomerType.MerchantTrader);

        // Assert - Merchants should prefer materials for resale
        Assert.True(preferences.MaterialPreference > 0.6f);
        Assert.True(preferences.ConsumablePreference > 0.5f);
        Assert.True(preferences.CommonQualityTolerance > 0.6f); // Volume sales
    }

    [Fact]
    public void CustomerLoyalty_CreateNew_InitializesCorrectly()
    {
        // Act
        var loyalty = CustomerLoyalty.CreateNew();

        // Assert
        Assert.Equal(1, loyalty.VisitCount);
        Assert.Equal(0, loyalty.PurchaseCount);
        Assert.Equal(0m, loyalty.TotalSpent);
        Assert.Equal(CustomerSatisfaction.Neutral, loyalty.LastVisitSatisfaction);
        Assert.Equal(0.5f, loyalty.LoyaltyScore);
        Assert.Equal(CustomerLoyaltyTier.Regular, loyalty.Tier);
        Assert.False(loyalty.QualifiesForDiscount); // Needs 3+ purchases
    }

    [Theory]
    [InlineData(CustomerSatisfaction.Delighted, 0.7f)]
    [InlineData(CustomerSatisfaction.Satisfied, 0.6f)]
    [InlineData(CustomerSatisfaction.Neutral, 0.55f)]
    [InlineData(CustomerSatisfaction.Disappointed, 0.45f)]
    [InlineData(CustomerSatisfaction.Angry, 0.35f)]
    public void CustomerLoyalty_UpdateAfterPurchase_AdjustsLoyaltyScore(
        CustomerSatisfaction satisfaction, float expectedMinScore)
    {
        // Arrange
        var loyalty = CustomerLoyalty.CreateNew();

        // Act
        var updatedLoyalty = loyalty.UpdateAfterPurchase(satisfaction);

        // Assert
        Assert.Equal(1, updatedLoyalty.PurchaseCount);
        Assert.Equal(satisfaction, updatedLoyalty.LastVisitSatisfaction);
        Assert.True(updatedLoyalty.LoyaltyScore >= expectedMinScore - 0.05f); // Allow small variance
        Assert.True(updatedLoyalty.LastVisit > loyalty.LastVisit);
    }

    [Theory]
    [InlineData(CustomerSatisfaction.Delighted, 0.52f)]
    [InlineData(CustomerSatisfaction.Satisfied, 0.51f)]
    [InlineData(CustomerSatisfaction.Neutral, 0.5f)]
    [InlineData(CustomerSatisfaction.Disappointed, 0.45f)]
    [InlineData(CustomerSatisfaction.Angry, 0.4f)]
    public void CustomerLoyalty_UpdateAfterVisit_AdjustsLoyaltyScore(
        CustomerSatisfaction satisfaction, float expectedScore)
    {
        // Arrange
        var loyalty = CustomerLoyalty.CreateNew();

        // Act
        var updatedLoyalty = loyalty.UpdateAfterVisit(satisfaction);

        // Assert
        Assert.Equal(2, updatedLoyalty.VisitCount); // Should increment
        Assert.Equal(0, updatedLoyalty.PurchaseCount); // Should not change
        Assert.Equal(satisfaction, updatedLoyalty.LastVisitSatisfaction);
        Assert.True(Math.Abs(updatedLoyalty.LoyaltyScore - expectedScore) < 0.05f);
    }

    [Theory]
    [InlineData(0.1f, CustomerLoyaltyTier.NewCustomer, 0.0f)]
    [InlineData(0.4f, CustomerLoyaltyTier.Regular, 0.05f)]
    [InlineData(0.6f, CustomerLoyaltyTier.Loyal, 0.10f)]
    [InlineData(0.8f, CustomerLoyaltyTier.VeryLoyal, 0.15f)]
    public void CustomerLoyalty_TierAndDiscount_CalculatesCorrectly(
        float loyaltyScore, CustomerLoyaltyTier expectedTier, float expectedDiscount)
    {
        // Arrange
        var loyalty = new CustomerLoyalty(
            VisitCount: 5,
            PurchaseCount: 3,
            TotalSpent: 500m,
            LastVisitSatisfaction: CustomerSatisfaction.Satisfied,
            FirstVisit: DateTime.Now.AddDays(-30),
            LastVisit: DateTime.Now,
            LoyaltyScore: loyaltyScore
        );

        // Act & Assert
        Assert.Equal(expectedTier, loyalty.Tier);
        Assert.Equal(expectedDiscount, loyalty.DiscountPercentage);

        if (expectedTier >= CustomerLoyaltyTier.Regular)
        {
            Assert.True(loyalty.QualifiesForDiscount);
        }
        else
        {
            Assert.False(loyalty.QualifiesForDiscount);
        }
    }

    [Fact]
    public void CustomerLoyalty_LoyaltyScore_ClampsBounds()
    {
        // Arrange
        var loyalty = new CustomerLoyalty(
            VisitCount: 1,
            PurchaseCount: 0,
            TotalSpent: 0m,
            LastVisitSatisfaction: CustomerSatisfaction.Neutral,
            FirstVisit: DateTime.Now,
            LastVisit: DateTime.Now,
            LoyaltyScore: 0.9f
        );

        // Act - Multiple very satisfied purchases should not exceed 1.0
        var updated = loyalty;
        for (int i = 0; i < 10; i++)
        {
            updated = updated.UpdateAfterPurchase(CustomerSatisfaction.Delighted);
        }

        // Assert
        Assert.True(updated.LoyaltyScore <= 1.0f);

        // Act - Multiple very unsatisfied visits should not go below 0.0
        for (int i = 0; i < 20; i++)
        {
            updated = updated.UpdateAfterVisit(CustomerSatisfaction.Angry);
        }

        // Assert
        Assert.True(updated.LoyaltyScore >= 0.0f);
    }

    [Fact]
    public void CustomerLoyaltyTier_Values_AreCorrectlyDefined()
    {
        // Assert - Verify enum values are in expected order
        Assert.True((int)CustomerLoyaltyTier.NewCustomer < (int)CustomerLoyaltyTier.Occasional);
        Assert.True((int)CustomerLoyaltyTier.Occasional < (int)CustomerLoyaltyTier.Regular);
        Assert.True((int)CustomerLoyaltyTier.Regular < (int)CustomerLoyaltyTier.Loyal);
        Assert.True((int)CustomerLoyaltyTier.Loyal < (int)CustomerLoyaltyTier.VeryLoyal);
    }
}