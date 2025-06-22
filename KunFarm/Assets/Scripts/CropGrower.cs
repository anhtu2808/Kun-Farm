using UnityEngine;

public class CropGrower : MonoBehaviour
{
    public CropData cropData;
    private SpriteRenderer spriteRenderer;
    private int currentStage = 0;
    private float timer = 0f;
    public bool isMature = false; // Đổi thành public để PlayerInteraction có thể đọc
    private bool playerNearby = false;

    // Sử dụng Awake để lấy SpriteRenderer ngay khi đối tượng được tạo
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Không gán sprite ở đây, sẽ gán trong Initialize()
    }

    // Hàm khởi tạo riêng, được gọi ngay sau khi gán CropData từ PlayerInteraction
    public void InitializeCrop(CropData data)
    {
        cropData = data;
        if (cropData == null)
        {
            Debug.LogError("CropData is null for CropGrower!", this);
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // Đảm bảo đã có
        }

        currentStage = 0; // Đảm bảo bắt đầu từ giai đoạn 0
        timer = 0f;
        isMature = false;

        // Gán sprite đầu tiên
        if (cropData.growthStages != null && cropData.growthStages.Length > 0 && cropData.growthStages[0] != null)
        {
            spriteRenderer.sprite = cropData.growthStages[0];
        }
        else
        {
            Debug.LogError("CropData for " + cropData.cropName + " has no initial sprite or growth stages!", this);
        }
    }

    void Update()
    {
        // Các logic cũ của Update vẫn giữ nguyên
        if (!isMature && currentStage < cropData.growthStages.Length - 1)
        {
            timer += Time.deltaTime;
            if (timer >= cropData.stageDurations[currentStage])
            {
                currentStage++;
                spriteRenderer.sprite = cropData.growthStages[currentStage];
                timer = 0f;

                if (currentStage == cropData.growthStages.Length - 1)
                {
                    isMature = true;
                }
            }
        }

        // Thu hoạch khi đủ điều kiện
        // (Bạn đang kiểm tra Input.GetKeyDown(KeyCode.F) ở đây, nhưng chúng ta sẽ dùng PlayerInteraction để thu hoạch)
        // Nếu bạn muốn giữ logic thu hoạch tự động khi người chơi lại gần và ấn F, thì giữ lại.
        // Tuy nhiên, tôi khuyên bạn nên điều khiển việc thu hoạch hoàn toàn từ PlayerInteraction để nhất quán.
        // if (isMature && playerNearby && Input.GetKeyDown(KeyCode.F))
        // {
        //     Harvest();
        // }
    }

    // Đảm bảo là public như đã sửa
    public void Harvest()
    {
        Debug.Log("Đã thu hoạch: " + cropData.cropName);

        foreach (HarvestDrop drop in cropData.harvestDrops)
        {
            for (int i = 0; i < drop.quantity; i++)
            {
                Vector3 dropPos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
                Instantiate(drop.itemPrefab, dropPos, Quaternion.identity);
            }
        }
        // Destroy(gameObject); // Không phá hủy ở đây nữa, PlayerInteraction sẽ xử lý sau khi deregister
    }

    // Các OnTriggerEnter2D/Exit2D giữ nguyên nếu bạn vẫn muốn dùng playerNearby
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}