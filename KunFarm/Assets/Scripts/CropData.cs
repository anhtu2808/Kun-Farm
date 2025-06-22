using UnityEngine;

[CreateAssetMenu(fileName = "NewCropData", menuName = "Crops/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName;
    public Sprite[] growthStages;       // các hình ảnh cây theo giai đoạn
    public float[] stageDurations;      // thời gian mỗi giai đoạn
    public HarvestDrop[] harvestDrops;  // 🆕 Mảng vật phẩm rơi
}
