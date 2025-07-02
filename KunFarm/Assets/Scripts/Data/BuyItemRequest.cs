using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BuyItemRequest
{
    public int SlotId;

    public int Quantity;
    public int TotalPrice;
}

[System.Serializable]
public class BuyItemList
{
    public List<BuyItemRequest> data;

    public BuyItemList(List<BuyItemRequest> list)
    {
        data = list;
    }
}