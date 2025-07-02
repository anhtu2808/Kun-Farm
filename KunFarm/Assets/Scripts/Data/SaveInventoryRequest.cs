using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveInventoryRequest
{
    public int SlotIndex;
    public string CollectableType;
    public int quantity;
}

[System.Serializable]
public class InventorySaveList
{
    public List<SaveInventoryRequest> data;

    public InventorySaveList(List<SaveInventoryRequest> list)
    {
        data = list;
    }
}