using Game.Shop.Models;
using Godot;

namespace Game.Scripts.UI.Components;

/// <summary>
/// Helper class to manage individual display slot UI elements.
/// </summary>
public class DisplaySlotUI
{
    public event Action<int>? StockRequested;
#pragma warning disable CS0067 // The event is never used - reserved for future price change functionality
    public event Action<int, decimal>? PriceChangeRequested;
#pragma warning restore CS0067
    public event Action<int>? RemoveRequested;

    private readonly int _slotId;
    private readonly Panel _slotPanel;
    private readonly Label _itemNameLabel;
    private readonly Label _itemPriceLabel;
    private readonly Button _stockButton;

    public DisplaySlotUI(int slotId, Panel slotPanel)
    {
        _slotId = slotId;
        _slotPanel = slotPanel;

        var itemDisplay = slotPanel.GetNode<VBoxContainer>("ItemDisplay");
        _itemNameLabel = itemDisplay.GetNode<Label>("ItemName");
        _itemPriceLabel = itemDisplay.GetNode<Label>("ItemPrice");
        _stockButton = itemDisplay.GetNode<Button>("StockButton");

        _stockButton.Pressed += OnStockButtonPressed;
    }

    public void UpdateDisplay(ShopDisplaySlot shopSlot)
    {
        if (shopSlot.IsOccupied && shopSlot.CurrentItem != null)
        {
            _itemNameLabel.Text = shopSlot.CurrentItem.Name;
            _itemPriceLabel.Text = $"{shopSlot.CurrentPrice:C}";
            _stockButton.Text = "Remove";
            _slotPanel.Modulate = Colors.White;
        }
        else
        {
            _itemNameLabel.Text = "Empty";
            _itemPriceLabel.Text = "0g";
            _stockButton.Text = "Stock Item";
            _slotPanel.Modulate = Colors.LightGray;
        }
    }

    private void OnStockButtonPressed()
    {
        if (_stockButton.Text == "Stock Item")
        {
            StockRequested?.Invoke(_slotId);
        }
        else
        {
            RemoveRequested?.Invoke(_slotId);
        }
    }
}