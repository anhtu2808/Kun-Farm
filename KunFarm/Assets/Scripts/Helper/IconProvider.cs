using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IconProvider
{
    /// <summary>
    /// Load Sprite từ thư mục Resources/Sprites theo key.
    /// </summary>
    public static Sprite GetIcon(string iconKey)
    {
        // ví dụ iconKey = "Basic_Grass_item_0" hoặc "Crops/Wheat_Seed"
        string path = $"Sprites/{iconKey}";
        Sprite s = Resources.Load<Sprite>(path);
        if (s == null)
            Debug.LogError($"Không tìm thấy Resources/{path}");
        return s;
    }
}
