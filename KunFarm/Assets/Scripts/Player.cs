using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Inventory inventory;
    private PickupAudio pickupAudio;
    [SerializeField]
    public Wallet wallet;

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


    private void Awake()
    {
        if (wallet == null)
            wallet = FindObjectOfType<Wallet>();
        if (inventory == null)
            inventory = FindObjectOfType<Inventory>();
        pickupAudio = GetComponent<PickupAudio>();
        if (pickupAudio == null)
        {
            pickupAudio = gameObject.AddComponent<PickupAudio>();
        }
    }

    public void PlayPickupSound()
    {
        if (pickupAudio != null)
        {
            pickupAudio.PlayPickupSound();
        }
    }

    public void DropItem(Collectable item)
    {
        Debug.Log("DropItem called for: " + item.name);

        // Drop item ngay tại vị trí player
        Vector2 spawnLocation = transform.position;
        Collectable droppedItem = Instantiate(item, spawnLocation, Quaternion.identity);

        // Ensure Rigidbody2D is properly initialized (for pickup physics if needed)
        if (droppedItem.rb2d == null)
        {
            // Try to get existing Rigidbody2D component
            droppedItem.rb2d = droppedItem.GetComponent<Rigidbody2D>();
            
            // If still null, add one
            if (droppedItem.rb2d == null)
            {
                droppedItem.rb2d = droppedItem.gameObject.AddComponent<Rigidbody2D>();
                droppedItem.rb2d.gravityScale = 0f; // No gravity for farming items
                droppedItem.rb2d.drag = 2f; // Add some drag
                droppedItem.rb2d.bodyType = RigidbodyType2D.Static; // Keep item stationary after drop
            }
        }
        else
        {
            // Make existing rigidbody static so item doesn't move
            droppedItem.rb2d.bodyType = RigidbodyType2D.Static;
            droppedItem.rb2d.velocity = Vector2.zero;
        }

        Debug.Log($"Dropped {item.type} at player position");
    }
    public bool TryBuy(int price) => wallet.Spend(price);
    public void Earn(int amount) => wallet.Add(amount);

}
