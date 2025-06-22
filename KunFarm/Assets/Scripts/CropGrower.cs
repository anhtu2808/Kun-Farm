using UnityEngine;

public class CropGrower : MonoBehaviour
{
    public CropData cropData;
    private SpriteRenderer spriteRenderer;

    private int currentStage = 0;
    private float timer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = cropData.growthStages[0];
    }

    void Update()
    {
        if (currentStage < cropData.growthStages.Length - 1)
        {
            timer += Time.deltaTime;
            if (timer >= cropData.stageDurations[currentStage])
            {
                currentStage++;
                spriteRenderer.sprite = cropData.growthStages[currentStage];
                timer = 0f;
            }
        }
    }
}
