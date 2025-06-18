using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Sprite openDoorSprite;
    public Sprite closedDoorSprite;

    private SpriteRenderer spriteRenderer;
    // Thay đổi: Tạo một tham chiếu riêng cho Collider chặn vật lý
    public Collider2D blockingCollider; // Kéo Collider 2D chặn vật lý vào đây từ Inspector

    public bool isOpen = false;

    private bool playerIsAtDoor = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // blockingCollider đã được gán qua Inspector

        spriteRenderer.sprite = closedDoorSprite;
        if (blockingCollider != null)
        {
            blockingCollider.enabled = true; // Đảm bảo collider chặn vật lý bật khi bắt đầu
        }
    }

    void Update()
    {
        if (playerIsAtDoor && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsAtDoor = true;
            Debug.Log("Người chơi đã ở gần cửa. Nhấn E để tương tác.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsAtDoor = false;
            Debug.Log("Người chơi đã rời khỏi cửa.");
        }
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            spriteRenderer.sprite = openDoorSprite;
            if (blockingCollider != null)
            {
                blockingCollider.enabled = false; // TẮT collider chặn khi cửa mở
            }
            Debug.Log("Cửa đã mở.");
        }
        else
        {
            spriteRenderer.sprite = closedDoorSprite;
            if (blockingCollider != null)
            {
                blockingCollider.enabled = true; // BẬT collider chặn khi cửa đóng
            }
            Debug.Log("Cửa đã đóng.");
        }
    }
}