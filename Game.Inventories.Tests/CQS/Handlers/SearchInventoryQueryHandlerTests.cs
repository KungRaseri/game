#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Inventories.Handlers;
using Game.Inventories.Models;
using Game.Inventories.Queries;
using Game.Inventories.Systems;
using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Inventories.Tests.CQS.Handlers;

public class SearchInventoryQueryHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly SearchInventoryQueryHandler _handler;
    private readonly Material _woodMaterial;
    private readonly Material _stoneMaterial;
    private readonly Material _gemMaterial;
    private readonly Drop _woodDrop;
    private readonly Drop _stoneDrop;
    private readonly Drop _gemDrop;

    public SearchInventoryQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new SearchInventoryQueryHandler(_inventoryManager);
        
        _woodMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Common);
        _stoneMaterial = ItemFactory.CreateMaterial(ItemTypes.IronOre, QualityTier.Common);
        _gemMaterial = ItemFactory.CreateMaterial(ItemTypes.Ruby, QualityTier.Rare);
        _woodDrop = new Drop(_woodMaterial, 10, DateTime.UtcNow);
        _stoneDrop = new Drop(_stoneMaterial, 15, DateTime.UtcNow);
        _gemDrop = new Drop(_gemMaterial, 3, DateTime.UtcNow);
        
        // Pre-populate inventory
        _inventoryManager.AddMaterials(new[] { _woodDrop, _stoneDrop, _gemDrop });
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new SearchInventoryQueryHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithSearchTerm_ReturnsFilteredResults()
    {
        // Arrange
        var criteria = new InventorySearchCriteria(SearchTerm: "wood");
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(1);
        result.Results[0].Material.Should().Be(_woodMaterial);
        result.TotalStacks.Should().Be(1);
        result.TotalMaterials.Should().Be(10);
        result.TotalValue.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HandleAsync_WithCategoryFilter_ReturnsFilteredResults()
    {
        // Arrange
        var criteria = new InventorySearchCriteria(CategoryFilter: Category.Wood);
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(1);
        result.Results[0].Material.Should().Be(_woodMaterial);
        result.Results[0].Material.Category.Should().Be(Category.Wood);
    }

    [Fact]
    public async Task HandleAsync_WithRarityFilter_ReturnsFilteredResults()
    {
        // Arrange
        var criteria = new InventorySearchCriteria(RarityFilter: QualityTier.Rare);
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(1);
        result.Results[0].Material.Should().Be(_gemMaterial);
        result.Results[0].Material.Quality.Should().Be(QualityTier.Rare);
    }

    [Fact]
    public async Task HandleAsync_WithMinQuantityFilter_ReturnsFilteredResults()
    {
        // Arrange
        var criteria = new InventorySearchCriteria(MinQuantity: 12);
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(1);
        result.Results[0].Material.Should().Be(_stoneMaterial);
        result.Results[0].Quantity.Should().Be(15);
    }

    [Fact]
    public async Task HandleAsync_WithMinValueFilter_ReturnsFilteredResults()
    {
        // Arrange
        var criteria = new InventorySearchCriteria(MinValue: 1000);
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(1);
        result.Results[0].Material.Should().Be(_gemMaterial);
    }

    [Fact]
    public async Task HandleAsync_WithSortByName_ReturnsSortedResults()
    {
        // Arrange
        var criteria = new InventorySearchCriteria(
            SortBy: MaterialSortBy.Name,
            SortAscending: true
        );
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(3);
        result.Results[0].Material.Name.Should().Be("Iron Ore"); // Alphabetically first
        result.Results[1].Material.Name.Should().Be("Oak Wood");
        result.Results[2].Material.Name.Should().Be("Ruby");
    }

    [Fact]
    public async Task HandleAsync_WithSortByQuantityDescending_ReturnsSortedResults()
    {
        // Arrange
        var criteria = new InventorySearchCriteria(
            SortBy: MaterialSortBy.Quantity,
            SortAscending: false
        );
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(3);
        result.Results[0].Quantity.Should().Be(15); // Stone - highest quantity
        result.Results[1].Quantity.Should().Be(10); // Wood
        result.Results[2].Quantity.Should().Be(3);  // Gem - lowest quantity
    }

    [Fact]
    public async Task HandleAsync_WithMultipleFilters_ReturnsFilteredResults()
    {
        // Arrange
        var criteria = new InventorySearchCriteria(
            RarityFilter: QualityTier.Common,
            MinQuantity: 5,
            SortBy: MaterialSortBy.Quantity,
            SortAscending: true
        );
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(2); // Wood and Stone, both Common with quantity >= 5
        result.Results[0].Quantity.Should().Be(10); // Wood (lower quantity first)
        result.Results[1].Quantity.Should().Be(15); // Stone
    }

    [Fact]
    public async Task HandleAsync_WithNoMatchingCriteria_ReturnsEmptyResults()
    {
        // Arrange
        var criteria = new InventorySearchCriteria(
            SearchTerm: "nonexistent",
            CategoryFilter: Category.Metal
        );
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().BeEmpty();
        result.TotalStacks.Should().Be(0);
        result.TotalMaterials.Should().Be(0);
        result.TotalValue.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyInventory_ReturnsEmptyResults()
    {
        // Arrange
        var emptyInventoryManager = new InventoryManager();
        var handler = new SearchInventoryQueryHandler(emptyInventoryManager);
        var criteria = new InventorySearchCriteria();
        var query = new SearchInventoryQuery { Criteria = criteria };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().BeEmpty();
        result.TotalStacks.Should().Be(0);
        result.TotalMaterials.Should().Be(0);
        result.TotalValue.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var criteria = new InventorySearchCriteria();
        var query = new SearchInventoryQuery { Criteria = criteria };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(3);
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
