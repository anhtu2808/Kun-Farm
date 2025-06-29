using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShopData
{
    public static List<ShopItem> GetAvailableItems()
    {
        return new List<ShopItem>
        {
            new ShopItem { type = CollectableType.EGG, icon = GameManager.instance.itemManager.GetItemByType(CollectableType.EGG).icon, price = 20 },
            new ShopItem { type = CollectableType.GRAPE, icon = GameManager.instance.itemManager.GetItemByType(CollectableType.GRAPE).icon, price = 15 }
        };
    }
}
