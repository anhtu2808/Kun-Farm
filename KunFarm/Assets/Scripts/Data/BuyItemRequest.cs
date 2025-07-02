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
public class BuyItemRequestList
{
    public List<BuyItemRequest> Items = new List<BuyItemRequest>();
}