using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopBuy_UI : MonoBehaviour
{
    public GameObject shopSlotPrefab;
    public Transform slotContainer;
    public Player player;

    void Start()
    {
        var shopItems = ShopData.GetAvailableItems(); // List<ShopItem> chứa type, icon, price

        foreach (var item in shopItems)
        {
            GameObject slotGO = Instantiate(shopSlotPrefab, slotContainer);
            ShopBuySlot_UI slotUI = slotGO.GetComponent<ShopBuySlot_UI>();

            slotUI.SetData(item.type, item.icon, item.price, player);

            // Nếu bạn gán sự kiện tại runtime (không gán sẵn trong Button)
            Button button = slotGO.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(slotUI.OnBuyClick);
        }
    }
}
