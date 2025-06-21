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
}
