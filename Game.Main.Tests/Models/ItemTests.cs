using Game.Items.Models;

namespace Game.Main.Tests.Models;

public class ItemTests
{
    [Fact]
    public void Constructor_SetsValues_Correctly()
    {
        // Arrange & Act
        var item = new Items(
            itemId: "test_item_001",
            name: "Test Item",
            description: "A test item",
            itemType: ItemType.Material,
            quality: QualityTier.Rare,
            value: 100
        );

        // Assert
        Assert.Equal("test_item_001", item.ItemId);
        Assert.Equal("Test Item", item.Name);
        Assert.Equal("A test item", item.Description);
        Assert.Equal(ItemType.Material, item.ItemType);
        Assert.Equal(QualityTier.Rare, item.Quality);
        Assert.Equal(100, item.Value);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenItemIdIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Items(
            itemId: null!,
            name: "Test",
            description: "Test",
            itemType: ItemType.Material,
            quality: QualityTier.Common,
            value: 10
        ));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNameIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Items(
            itemId: "test_001",
            name: null!,
            description: "Test",
            itemType: ItemType.Material,
            quality: QualityTier.Common,
            value: 10
        ));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenDescriptionIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Items(
            itemId: "test_001",
            name: "Test",
            description: null!,
            itemType: ItemType.Material,
            quality: QualityTier.Common,
            value: 10
        ));
    }

    [Fact]
    public void Constructor_ClampsNegativeValue_ToZero()
    {
        // Arrange & Act
        var item = new Items(
            itemId: "test_001",
            name: "Test",
            description: "Test",
            itemType: ItemType.Material,
            quality: QualityTier.Common,
            value: -50
        );

        // Assert
        Assert.Equal(0, item.Value);
    }

    [Fact]
    public void Value_Setter_ClampsNegativeValue_ToZero()
    {
        // Arrange
        var item = new Items("test_001", "Test", "Test", ItemType.Material, QualityTier.Common, 100);

        // Act
        item.Value = -20;

        // Assert
        Assert.Equal(0, item.Value);
    }

    [Fact]
    public void Value_Setter_AcceptsZero()
    {
        // Arrange
        var item = new Items("test_001", "Test", "Test", ItemType.Material, QualityTier.Common, 100);

        // Act
        item.Value = 0;

        // Assert
        Assert.Equal(0, item.Value);
    }

    [Fact]
    public void Value_Setter_AcceptsPositiveValue()
    {
        // Arrange
        var item = new Items("test_001", "Test", "Test", ItemType.Material, QualityTier.Common, 10);

        // Act
        item.Value = 500;

        // Assert
        Assert.Equal(500, item.Value);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var item = new Items("test_001", "Magic Stone", "Test", ItemType.Material, QualityTier.Epic, 200);

        // Act
        var result = item.ToString();

        // Assert
        Assert.Equal("Magic Stone (Epic Material) - 200g", result);
    }
}
