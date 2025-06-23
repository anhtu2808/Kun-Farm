using System.Collections;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public CollectableType type;
    private GameObject chickenPrefab;
    public Sprite icon;
    public Rigidbody2D rb2d;

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
    // Đợi 10 giây (hoặc 120f cho 2 phút)
    yield return new WaitForSeconds(10f);

    if (chickenPrefab != null)
    {
        // Tạo vị trí mới với z = -10
        Vector3 spawnPos = new Vector3(0f, 0f, -10f);
        Instantiate(chickenPrefab, spawnPos, Quaternion.identity);
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
    NONE, EGG, WHEAT, GRAPE, APPLETREE, WHEATSEED, GRAPESEED, APPLETREESEED
}
