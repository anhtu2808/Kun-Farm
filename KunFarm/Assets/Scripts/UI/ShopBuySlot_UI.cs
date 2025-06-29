using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopBuySlot_UI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI priceText;

    private CollectableType itemType;
    private int price;
    private Player player;

    public void SetData(CollectableType type, Sprite iconSprite, int itemPrice, Player playerRef)
    {
        itemType = type;
        price = itemPrice;
        player = playerRef;

        icon.sprite = iconSprite;
        icon.color = Color.white;
        priceText.text = price.ToString();
    }

    public void OnBuyClick()
    {
        if (player.wallet.Money >= price)
        {
            var item = GameManager.instance.itemManager.GetItemByType(itemType);
            if (item != null)
            {
                player.wallet.Spend(price);
                player.inventory.Add(item);
                Debug.Log($"✅ Đã mua {itemType} với giá {price}");
            }
            else
            {
                Debug.LogWarning("⚠ Không tìm thấy vật phẩm tương ứng!");
            }
        }
        else
        {
            Debug.LogWarning("❌ Không đủ tiền để mua vật phẩm!");
        }
    }
}
