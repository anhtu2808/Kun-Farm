using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Inventory inventory;
    private TileManager tileManager;

    private void Awake()
    {
        inventory = new Inventory(21);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        if (tileManager != null)
    //        {
    //            Vector3Int position = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);

    //            string tileName = tileManager.GetTileName(position);

    //            if (!string.IsNullOrWhiteSpace(tileName))
    //            {
    //                if (tileName == "interactable" && inventory.toolbar.selectedSlot.itemName == "Hoe")
    //                {
    //                    tileManager.SetInteracted(position);
    //                }
    //            }
    //        }
    //    }
    //}
}
