using UnityEngine;

[System.Serializable] // Rất quan trọng để hiển thị trong Inspector
public class HarvestDrop
{
    public GameObject itemPrefab; // Prefab của vật phẩm sẽ rơi ra (ví dụ: củ cà rốt, hạt giống mới)
    public bool isFruit;
    public int quantity;          // Số lượng vật phẩm
}