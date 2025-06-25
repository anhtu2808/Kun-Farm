// CropData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewCropData", menuName = "Crops/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName;
    public Sprite[] growthStages;     // Các hình ảnh cây theo giai đoạn
    public float[] stageDurations;   // Thời gian mỗi giai đoạn
    public HarvestDrop[] harvestDrops; // Mảng vật phẩm rơi (sử dụng class HarvestDrop)

    public GameObject cropPrefab; // Prefab của cây trực quan (ví dụ: Prefab Grape)
}