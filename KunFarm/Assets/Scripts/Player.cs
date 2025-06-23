using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3Int position = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
            if (GameManager.instance.tileManager.IsInteractable(position))
            {
                Debug.Log("Tile is interactable");
                // Add interaction logic here
            }
            else
            {
                Debug.Log("No interactable tile at position: " + position);
            }
        }
    }
    public Inventory inventory;

    private void Awake()
    {
        inventory = new Inventory(21);
    }

  public void DropItem(Collectable item)
{
    Debug.Log("DropItem called for: " + item.name);

    Vector2 spawnLocation = transform.position;
    Vector2 spawnOffset = UnityEngine.Random.insideUnitCircle * 3f;

    Collectable droppedItem = Instantiate(item, spawnLocation + spawnOffset, Quaternion.identity);

    if (droppedItem.rb2d == null)
    {
        Debug.LogError("rb2d is null! Did not assign or init properly.");
    }
    else
    {
        droppedItem.rb2d.AddForce(spawnOffset * 2f, ForceMode2D.Impulse);
        Debug.Log("Force applied to dropped item");
    }
}

}
