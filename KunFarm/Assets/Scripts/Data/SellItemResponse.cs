using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SellItemResponse
{
    public int id;
    public string collectableType;
    public int quantity;
    public int price;
    public string icon;

    public bool canBuy;
}

[System.Serializable]
public class BuyShopWrapper
{
      public SellItemResponse[] data;
}