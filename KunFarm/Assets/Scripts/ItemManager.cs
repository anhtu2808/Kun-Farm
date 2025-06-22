using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public Collectable[] collectableItems;

    private Dictionary<CollectableType, Collectable> collectableItemDict = new Dictionary<CollectableType, Collectable>();

    private void Awake()
    {
        foreach (Collectable item in collectableItems)
        {
            AddItem(item);
        }
    }

    private void AddItem(Collectable item)
    {
        if (!collectableItemDict.ContainsKey(item.type))
        {
            collectableItemDict.Add(item.type, item);
        }
    }

    public Collectable GetItemByType(CollectableType type)
    {
        if (collectableItemDict.ContainsKey(type))
        {
            return collectableItemDict[type];
        }
        return null;
    }

}
