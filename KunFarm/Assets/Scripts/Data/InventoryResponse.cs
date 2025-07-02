using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryResponse
{
    public int code;
    public string message;
    public List<InventorySlotData> data;
}

[System.Serializable]
public class InventorySlotData
{
    public int id;
    public int slotIndex;
    public int itemId;
    public string collectableType;
    public string icon;
    public int quantity;
}
