using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopResponse
{
    public int code;
    public string message;
    public List<ShopSlotData> data;
}

[System.Serializable]
public class ShopSlotData
{
    public int slotId;
    public string collectableType;
    public string itemName;
    public string icon;
    public bool canBuy;
    public int buyPrice;
    public int stockLimit;
    public int currentStock;
}

[Serializable]
public class ShopResponseWrapper
{
    public ShopSlotData[] data;
}