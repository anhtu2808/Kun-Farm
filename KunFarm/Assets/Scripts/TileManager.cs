using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TileManager : MonoBehaviour
{
    [SerializeField] private Tilemap interactableMap;
    [SerializeField] private Tile hiddenIntaractableTile;
    void Start()
    {
        foreach (var pos in interactableMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = interactableMap.GetTile(pos);
            if (tile != null && tile.name == "Interactable_Visible")
            {
                interactableMap.SetTile(pos, hiddenIntaractableTile);
            }
        }
    }

    public bool IsInteractable(Vector3Int position)
    {
        TileBase tile = interactableMap.GetTile(position);
        if (tile != null)
        {
            if (tile.name == "Interactable")
            {
                return true;
            }
        }
        return false;
    }
}
