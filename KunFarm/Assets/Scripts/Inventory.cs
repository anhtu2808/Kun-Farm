using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory
{

    [System.Serializable]
    public class Slot
    {
        public CollectableType type;
        public int count;
        public int maxAllowed;

        public Slot()
        {
            type = CollectableType.NONE;
            count = 0;
            maxAllowed = 99;
        }

        public bool CanAddItem()
        {
            if (count < maxAllowed) return true;

            return false;
        }

        public void AddItem(CollectableType type) 
        {
            this.type = type;
            count++;
        }

    }

    public List<Slot> slots = new List<Slot>();
    
    public Inventory(int numSlots) 
    {
        for (int i = 0; i < numSlots; i++) 
        {
            Slot slot = new();
            slots.Add(slot);
        }
    }

    public void Add(CollectableType typeToAdd)
    {
        foreach (Slot slot in slots)
        {
            if (slot.type == typeToAdd && slot.CanAddItem())
            {
                slot.AddItem(typeToAdd);
                return;
            }
        }

        foreach (Slot slot in slots)
        {
            if (slot.type == CollectableType.NONE)
            {
                slot.AddItem(typeToAdd);
                return ;
            }
        }

    }

}
