using UnityEngine;

public class CropGrower : MonoBehaviour
{
    public CropData cropData;
    private SpriteRenderer spriteRenderer;
    private int currentStage = 0;
    private float timer = 0f;
    private bool isMature = false;
    private bool playerNearby = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = cropData.growthStages[0];
    }

    void Update()
    {
        // Cây phát tri?n qua t?ng stage
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

        // Thu ho?ch khi ?? ?i?u ki?n
        if (isMature && playerNearby && Input.GetKeyDown(KeyCode.F))
        {
            Harvest();
        }
    }

    void Harvest()
    {
        Debug.Log("?ã thu ho?ch: " + cropData.cropName);

        foreach (HarvestDrop drop in cropData.harvestDrops)
        {
            for (int i = 0; i < drop.quantity; i++)
            {
                Vector3 dropPos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
                Instantiate(drop.itemPrefab, dropPos, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }


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
