using System.Collections;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public CollectableType type;
    private GameObject chickenPrefab;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        GetComponent<SpriteRenderer>().sortingOrder = 5;
        if (type == CollectableType.EGG)
        {
            // Load prefab gà
            chickenPrefab = Resources.Load<GameObject>("Prelabs/Chicken");

            if (chickenPrefab == null)
            {
                Debug.LogWarning("Không tìm thấy prefab 'Prelabs/Chicken' trong Resources!");
            }
            else
            {
                StartCoroutine(HatchEgg());
            }
        }
    }

    private IEnumerator HatchEgg()
    {
        yield return new WaitForSeconds(10f); // 2 phút

        if (chickenPrefab != null)
        {
            Instantiate(chickenPrefab, transform.position, Quaternion.identity);
        }

        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player)
        {
            player.inventory.Add(this);
            Destroy(this.gameObject);
        }
    }
}

public enum CollectableType
{
    NONE, EGG
}
