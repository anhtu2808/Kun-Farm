using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopBuySlot_UI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI stockText;
    private SellItemResponse slotData;
    private OnlBuyShopManager shopManager;
    private Player player;

    public void Setup(SellItemResponse data, OnlBuyShopManager manager)
    {
        slotData = data;
        shopManager = manager;

        priceText.text = $"{slotData.price}G";
        // stockText.text = $"{data.currentStock}/{data.stockLimit}";
        stockText.text = $"{slotData.quantity}"; // Assuming Quantity is the current stock and limit
        Sprite icon = Resources.Load<Sprite>($"Sprites/{slotData.icon}");
        itemIcon.sprite = icon;
    }

    public void OnBuyClicked()
    {
        if (slotData != null && shopManager != null)
        {
            if (!slotData.canBuy)
            {
                Debug.LogWarning("Item hết hàng!");
                return;
            }
            shopManager.BuyItem(slotData);
            Destroy(gameObject);
        }
    }
}
